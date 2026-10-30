using StarVanguard.UI;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Headless Play Mode smoke test for Star Vanguard.
    /// Run with:
    ///   Unity -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run
    /// It opens MainMenuScene, enters Play Mode, loads GameplayScene exactly like the
    /// "LAUNCH MISSION" button does, then asserts the mission actually boots.
    /// Exits 0 when every check passes, 1 otherwise (2 on timeout).
    /// </summary>
    public static class StarVanguardPlayModeSmoke
    {
        private const string k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
        private const string k_MainMenuSceneName = "MainMenuScene";
        private const string k_GameplaySceneName = "GameplayScene";
        private const double k_WatchdogSeconds = 120.0;

        private static int s_Passed;
        private static int s_Failed;
        private static int s_Step;
        private static double s_StartTime;
        private static float s_StepGameTime;
        private static double s_LastDiagTime;

        public static void Run()
        {
            s_Passed = 0;
            s_Failed = 0;
            s_Step = 0;

            Debug.Log("[STARVANGUARD-SMOKE] ===== BEGIN PLAY MODE SMOKE =====");

            // Start from BootstrapScene, like a real player launching the game
            EditorSceneManager.OpenScene(k_BootstrapScenePath, OpenSceneMode.Single);
            Debug.Log($"[STARVANGUARD-SMOKE] opened {k_BootstrapScenePath}");

            EditorApplication.update += Tick;
            s_StartTime = EditorApplication.timeSinceStartup;
            s_StepGameTime = 0f;

            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup - s_StartTime > k_WatchdogSeconds)
            {
                Fail("watchdog timeout - play mode never reached the expected state");
                Finish(2);
                return;
            }

            // In -batchmode there is no editor window to repaint, so the player loop only
            // advances when we explicitly ask for it. Without this, Time.time stays frozen
            // and every WaitForSeconds in the game never completes.
            if (EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPaused)
                {
                    EditorApplication.isPaused = true;
                }
                EditorApplication.Step();
            }

            switch (s_Step)
            {
                case 0:
                    // Wait for BootstrapScene to load MainMenuScene and build uGUI hierarchy.
                    if (EditorApplication.isPlaying && SceneManager.GetActiveScene().name == k_MainMenuSceneName && Time.frameCount > 5 && Time.time > 1.0f)
                    {
                        Assert(SceneManager.GetActiveScene().name == k_MainMenuSceneName, "transitioned from BootstrapScene to MainMenuScene");
                        CheckMenuScene();

                        // Open the controls manual the same way the button does.
                        UIPanelRouter.Instance?.Push(UIPanelId.Controls);
                        s_Step = 1;
                        s_StepGameTime = Time.time;
                        s_LastDiagTime = EditorApplication.timeSinceStartup;
                    }
                    break;

                case 1:
                    // Wait out the 0.12 s fade: a CanvasGroup at alpha 0 does not raycast.
                    if (Time.time - s_StepGameTime > 0.4f)
                    {
                        CheckControlsPanel();
                        UIPanelRouter.Instance?.Push(UIPanelId.Settings);
                        s_Step = 2;
                        s_StepGameTime = Time.time;
                        s_LastDiagTime = EditorApplication.timeSinceStartup;
                    }
                    break;

                case 2:
                    if (Time.time - s_StepGameTime > 0.4f)
                    {
                        CheckSettingsPanel();

                        Debug.Log("[STARVANGUARD-SMOKE] loading GameplayScene (same call as the menu button)");
                        SceneManager.LoadScene(k_GameplaySceneName);
                        s_Step = 3;
                        s_StepGameTime = Time.time;
                        s_LastDiagTime = EditorApplication.timeSinceStartup;
                    }
                    break;

                case 3:
                    // Report every 2 s of wall clock so a frozen clock explains itself
                    if (EditorApplication.timeSinceStartup - s_LastDiagTime > 2.0)
                    {
                        s_LastDiagTime = EditorApplication.timeSinceStartup;
                        Debug.Log($"[STARVANGUARD-SMOKE] diag step3 timeScale={Time.timeScale} " +
                                  $"state={(GameManager.Instance != null ? GameManager.Instance.GameState.ToString() : "no-manager")} " +
                                  $"time={Time.time:F2} unscaled={Time.unscaledTime:F2} " +
                                  $"delta={Time.deltaTime:F4} frames={Time.frameCount}");
                    }

                    if (Time.time - s_StepGameTime > 1.0f)
                    {
                        CheckMissionBooted();
                        s_Step = 4;
                        s_StepGameTime = Time.time;
                    }
                    break;

                case 4:
                    // Wave_Line waits 2 s before spawning, so give it 6 s of game time
                    if (Time.time - s_StepGameTime > 6.0f)
                    {
                        CheckWavesRunning();
                        CheckOverheatRuntime();
                        Finish(s_Failed == 0 ? 0 : 1);
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------- checks

        private static void CheckMenuScene()
        {
            Debug.Log($"[STARVANGUARD-SMOKE] diag menu timeScale={Time.timeScale} time={Time.time:F2} " +
                      $"panels(settings)={IsActive("SettingsPanel")} hud={Exists("HUDContainer")} " +
                      $"waveDirector={Exists("WaveDirector")}");

            Assert(Object.FindAnyObjectByType<MainMenuScreen>() != null, "MainMenuScreen runs in MainMenuScene");
            Assert(Exists("MenuRoot"), "menu hierarchy (MenuRoot) was built");
            Assert(IsActive("MenuRoot"), "menu is visible on boot");

            DumpVisualHierarchy();
            CheckNoLeftoverCanvasObjects();

            // The whole point of the split: the menu no longer carries the game.
            Assert(!Exists("HUDContainer"), "HUD is not in the menu scene");
            Assert(!Exists("WaveDirector"), "WaveDirector is not in the menu scene");
            Assert(!Exists("Player"), "player ship is not in the menu scene");
            Assert(!IsActive("SettingsPanel"), "SettingsPanel starts hidden");
            Assert(!IsActive("LeaderboardPanel"), "LeaderboardPanel starts hidden");

            // Menu scene must not carry local persistent managers (isolated in BootstrapScene)
            bool hasLocalMenuPersistent = false;
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "GameManager" || t.name == "PoolManager" || t.name == "AudioManager" ||
                        t.name == "LeaderboardManager" || t.name == "SettingsManager")
                    {
                        hasLocalMenuPersistent = true;
                    }
                }
            }
            Assert(!hasLocalMenuPersistent, "MainMenuScene contains NO local copies of persistent managers");

            Assert(Object.FindObjectsByType<Canvas>().Length == 1, "menu has exactly one Canvas");
            Assert(Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>().Length == 1,
                "menu has exactly one EventSystem");

            int activePanels = 0;
            string[] panelNames =
            {
                "MenuRoot", "SettingsPanel", "ControlsPanel", "LeaderboardPanel",
                "CoopPanel", "PausePanel", "GameOverPanel", "NewHighScorePanel",
            };
            foreach (string panelName in panelNames)
            {
                if (IsActive(panelName)) activePanels++;
            }

            Assert(activePanels == 1, $"exactly one menu panel is visible (actual={activePanels})");
            Assert(UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Home,
                "router reports Home as the current panel");

            Button launch = FindButton("LaunchButton");
            Assert(launch != null && IsTopMostClickable(launch), "LAUNCH MISSION button is the top-most raycast target");

            // Difficulty picked on the menu must reach the game manager.
            GameManager.Instance?.SetSelectedDifficulty(3);
            Assert(GameManager.Instance != null && GameManager.Instance.SelectedDifficulty == 3,
                "difficulty selection (ACE) reaches GameManager");
            GameManager.Instance?.SetSelectedDifficulty(2);
        }

        /// <summary>
        /// Proves the panel can actually be used: every control must be the first thing a
        /// pointer ray hits. This is what caught the old controls panel, whose 1680x820
        /// backdrop was drawn on top of its own Back button.
        /// </summary>
        private static void CheckControlsPanel()
        {
            UIPanelRouter router = UIPanelRouter.Instance;
            Assert(router != null && router.Current == UIPanelId.Controls,
                "CONTROLS panel opened through the router");

            RectTransform card = FindRect("Card");
            if (Assert(card != null, "controls card exists"))
            {
                float width = card.rect.width;
                float height = card.rect.height;
                Debug.Log($"[STARVANGUARD-SMOKE] diag controls card={width:F0}x{height:F0}");
                Assert(width <= 1920f && height <= 1080f,
                    $"controls card fits a 1080p screen (actual={width:F0}x{height:F0})");
            }

            Button back = FindButton("BackButton");
            if (Assert(back != null, "BACK button exists"))
            {
                Assert(IsTopMostClickable(back), "BACK button is not covered by another graphic");
                back.onClick.Invoke();
                Assert(router.Current == UIPanelId.Home, "BACK returns to the main menu");
            }
        }

        /// <summary>
        /// The Settings panel's BACK button used to be wired to a controller with an empty
        /// target, so it silently did nothing. This drives it for real.
        /// </summary>
        private static void CheckSettingsPanel()
        {
            UIPanelRouter router = UIPanelRouter.Instance;
            UIPanelRouter.s_DebugTrace = true;
            Assert(router != null && router.Current == UIPanelId.Settings, "SETTINGS panel opened through the router");

            RectTransform settings = FindRect("SettingsPanel");
            if (Assert(settings != null, "SettingsPanel found"))
            {
                float height = settings.rect.height;
                Assert(height <= 1080f, $"SettingsPanel fits the screen (height={height:F0})");
            }

            Button back = FindButton("BackButton");
            if (Assert(back != null, "settings BACK button exists"))
            {
                Assert(IsTopMostClickable(back), "settings BACK is not covered by another graphic");

                System.Text.StringBuilder calls = new System.Text.StringBuilder();
                for (int i = 0; i < back.onClick.GetPersistentEventCount(); i++)
                {
                    UnityEngine.Object target = back.onClick.GetPersistentTarget(i);
                    string targetName = target != null ? target.name : "null";
                    calls.Append($"[{targetName}.{back.onClick.GetPersistentMethodName(i)}] ");
                }
                Debug.Log($"[STARVANGUARD-SMOKE] diag settings back listeners={back.onClick.GetPersistentEventCount()} {calls}" +
                          $" routerNull={UIPanelRouter.Instance == null} before={router.Current} depth={router.StackDepth}");

                try
                {
                    back.onClick.Invoke();
                }
                catch (System.Exception exception)
                {
                    Debug.LogError($"[STARVANGUARD-SMOKE] settings BACK threw: {exception}");
                }

                UIPanelRouter.s_DebugTrace = false;
                Debug.Log($"[STARVANGUARD-SMOKE] diag settings back after invoke: current={router.Current} depth={router.StackDepth}");
                Assert(router.Current == UIPanelId.Home, "settings BACK returns to the main menu");
            }
        }

        /// <summary>
        /// Prints what actually gets rendered on boot: every Canvas, Camera and the menu
        /// roots themselves. Two menus drawn on top of each other are impossible to see in
        /// a headless run, so the hierarchy has to be described in text.
        /// </summary>
        private static void DumpVisualHierarchy()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>();
            Debug.Log($"[STARVANGUARD-SMOKE] diag canvases={canvases.Length}");
            foreach (Canvas canvas in canvases)
            {
                RectTransform rect = canvas.transform as RectTransform;
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                Debug.Log($"[STARVANGUARD-SMOKE] diag canvas '{Path(canvas.transform)}' root={(canvas.rootCanvas != null ? canvas.rootCanvas.name : "-")} " +
                          $"mode={canvas.renderMode} order={canvas.sortingOrder} scale={canvas.scaleFactor:F3} " +
                          $"size={(rect != null ? rect.rect.width.ToString("F0") + "x" + rect.rect.height.ToString("F0") : "-")} " +
                          $"scaler={(scaler != null ? $"{scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight:F2}" : "none")}");
            }

            Camera[] cameras = Object.FindObjectsByType<Camera>();
            Debug.Log($"[STARVANGUARD-SMOKE] diag cameras={cameras.Length} screen={Screen.width}x{Screen.height}");
            foreach (Camera camera in cameras)
            {
                Rect rect = camera.rect;
                Debug.Log($"[STARVANGUARD-SMOKE] diag camera '{camera.name}' depth={camera.depth:F1} clear={camera.clearFlags} " +
                          $"ortho={camera.orthographicSize:F2} rect=({rect.x:F2},{rect.y:F2},{rect.width:F2},{rect.height:F2}) active={camera.isActiveAndEnabled}");
            }

            GameObject[] roots = Resources.FindObjectsOfTypeAll<GameObject>();
            int menuRoots = 0;
            foreach (GameObject candidate in roots)
            {
                if (candidate.name != "MenuRoot" || !candidate.scene.IsValid()) continue;
                menuRoots++;
                RectTransform rect = candidate.transform as RectTransform;
                Vector3[] corners = new Vector3[4];
                if (rect != null) rect.GetWorldCorners(corners);
                CanvasGroup group = candidate.GetComponent<CanvasGroup>();
                Debug.Log($"[STARVANGUARD-SMOKE] diag MenuRoot#{menuRoots} path='{Path(candidate.transform)}' active={candidate.activeInHierarchy} " +
                          $"size={(rect != null ? rect.rect.width.ToString("F0") + "x" + rect.rect.height.ToString("F0") : "-")} " +
                          $"lossyScale={candidate.transform.lossyScale} corners=({corners[0].x:F0},{corners[0].y:F0})-({corners[2].x:F0},{corners[2].y:F0}) " +
                          $"alpha={(group != null ? group.alpha.ToString("F2") : "no-group")}");
            }

            Assert(menuRoots == 1, $"the menu hierarchy exists exactly once (actual={menuRoots})");
        }

        private static readonly string[] k_AllowedCanvasChildren =
        {
            "MenuRoot", "CoopPanel", "ControlsPanel",
            "SettingsPanel", "LeaderboardPanel", "PausePanel", "GameOverPanel", "NewHighScorePanel",
        };

        /// <summary>
        /// A menu baked into an older version of the scene keeps drawing underneath the one built
        /// at runtime: the screen looks doubled and the old full-screen backdrop swallows clicks.
        /// Nothing on the menu canvas may be visible unless the menu or the router put it there.
        /// </summary>
        private static void CheckNoLeftoverCanvasObjects()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (!Assert(canvas != null, "canvas exists for the leftover check")) return;

            System.Text.StringBuilder leftovers = new System.Text.StringBuilder();
            foreach (Transform child in canvas.transform)
            {
                if (!child.gameObject.activeSelf) continue;
                if (System.Array.IndexOf(k_AllowedCanvasChildren, child.name) >= 0) continue;
                leftovers.Append(child.name).Append(' ');
            }

            Assert(leftovers.Length == 0, $"no leftover UI is drawn on the menu canvas (found: {leftovers})");
        }

        private static string Path(Transform i_Transform)
        {
            string result = i_Transform.name;
            Transform current = i_Transform.parent;
            while (current != null)
            {
                result = current.name + "/" + result;
                current = current.parent;
            }

            return result;
        }

        private static RectTransform FindRect(string i_Name)
        {
            GameObject found = GameObject.Find(i_Name);
            return found != null ? found.GetComponent<RectTransform>() : null;
        }

        private static Button FindButton(string i_Name)
        {
            GameObject found = GameObject.Find(i_Name);
            return found != null ? found.GetComponent<Button>() : null;
        }

        private static bool IsTopMostClickable(Component i_Target)
        {
            if (i_Target == null) return false;

            GraphicRaycaster raycaster = Object.FindAnyObjectByType<GraphicRaycaster>();
            if (raycaster == null) return false;

            RectTransform rect = i_Target.transform as RectTransform;
            if (rect == null) return false;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPoint };
            List<RaycastResult> hits = new List<RaycastResult>();
            raycaster.Raycast(eventData, hits);

            if (hits.Count == 0) return false;

            GameObject hit = hits[0].gameObject;
            return hit == i_Target.gameObject || hit.transform.IsChildOf(i_Target.transform);
        }

        private static bool Exists(string i_Name) => GameObject.Find(i_Name) != null;

        private static bool IsActive(string i_Name)
        {
            GameObject found = GameObject.Find(i_Name);
            return found != null && found.activeInHierarchy;
        }

        private static void CheckMissionBooted()
        {
            GameManager manager = GameManager.Instance;
            if (Assert(manager != null, "GameManager.Instance survives the scene load"))
            {
                Assert(manager.GameState == eGameState.Playing,
                    $"GameState is Playing after loading GameplayScene (actual={manager.GameState})");
            }

            Assert(Object.FindAnyObjectByType<PlayerRespawn>() != null, "player ship spawned in GameplayScene");
            Assert(Object.FindAnyObjectByType<WaveDirector>() != null, "WaveDirector present in GameplayScene");

            CheckSingletonOwnership();
        }

        /// <summary>
        /// Scene-bound managers must belong to the scene the player is in. A manager invented
        /// while the menu scene unloaded used to win this, so gameplay ran with a ParallaxManager
        /// whose layers (and every ParallaxLayer's serialized reference) were already gone.
        /// </summary>
        private static void CheckSingletonOwnership()
        {
            ParallaxManager parallax = Object.FindAnyObjectByType<ParallaxManager>();
            if (Assert(parallax != null, "ParallaxManager exists after loading GameplayScene"))
            {
                int parallaxCount = Object.FindObjectsByType<ParallaxManager>().Length;
                Debug.Log($"[STARVANGUARD-SMOKE] diag parallax scene='{parallax.gameObject.scene.name}' count={parallaxCount}");

                Assert(parallax.gameObject.scene.name == k_GameplaySceneName,
                    $"ParallaxManager belongs to the gameplay scene (actual='{parallax.gameObject.scene.name}')");
                Assert(parallaxCount == 1,
                    $"exactly one ParallaxManager survives the menu to gameplay transition (actual={parallaxCount})");
            }

            if (Assert(UIManager.Instance != null, "UIManager instance exists after loading GameplayScene"))
            {
                Assert(UIManager.Instance.gameObject.scene.name == k_GameplaySceneName,
                    $"UIManager belongs to the gameplay scene (actual='{UIManager.Instance.gameObject.scene.name}')");
            }

            if (Assert(WaveDirector.Instance != null, "WaveDirector instance exists after loading GameplayScene"))
            {
                Assert(WaveDirector.Instance.gameObject.scene.name == k_GameplaySceneName,
                    $"WaveDirector belongs to the gameplay scene (actual='{WaveDirector.Instance.gameObject.scene.name}')");
            }

            // Persistent managers must live in DontDestroyOnLoad (originated from BootstrapScene)
            if (Assert(GameManager.Instance != null, "GameManager instance exists"))
            {
                Assert(GameManager.Instance.gameObject.scene.name == "DontDestroyOnLoad",
                    "GameManager lives in DontDestroyOnLoad (from BootstrapScene)");
            }

            if (Assert(PoolManager.Instance != null, "PoolManager instance exists"))
            {
                Assert(PoolManager.Instance.gameObject.scene.name == "DontDestroyOnLoad",
                    "PoolManager lives in DontDestroyOnLoad (from BootstrapScene)");
            }

            // Assert that GameplayScene contains NO local copies of persistent managers
            Scene gameplayScene = SceneManager.GetActiveScene();
            bool hasLocalPersistentMgr = false;
            foreach (GameObject root in gameplayScene.GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "GameManager" || child.name == "PoolManager" || child.name == "AudioManager" ||
                        child.name == "LeaderboardManager" || child.name == "SettingsManager")
                    {
                        hasLocalPersistentMgr = true;
                    }
                }
            }
            Assert(!hasLocalPersistentMgr, "GameplayScene contains NO local copies of persistent managers");
        }

        private static void CheckWavesRunning()
        {
            // Diagnostics so a failure explains itself instead of needing another run
            Camera mainCamera = Camera.main;
            Debug.Log($"[STARVANGUARD-SMOKE] diag timeScale={Time.timeScale} time={Time.time:F2} " +
                      $"delta={Time.deltaTime:F4} cameraMain={(mainCamera != null ? mainCamera.name : "NULL")} " +
                      $"cameras={Camera.allCamerasCount}");

            WaveDirector director = WaveDirector.Instance;
            if (director != null)
            {
                FieldInfo camField = typeof(WaveDirector).GetField("m_Camera",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo playerField = typeof(WaveDirector).GetField("m_Player",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                bool cameraOk = camField != null && camField.GetValue(director) is Camera c && c != null;
                bool playerOk = playerField != null && playerField.GetValue(director) is Transform t && t != null;
                int available = PoolManager.Instance != null ? PoolManager.Instance.GetAvailableEnemyCount() : -1;

                Debug.Log($"[STARVANGUARD-SMOKE] diag director.camera={(cameraOk ? "ok" : "STALE/NULL")} " +
                          $"director.player={(playerOk ? "ok" : "STALE/NULL")} pool.available={available}");
            }

            if (Assert(director != null, "WaveDirector.Instance available"))
            {
                FieldInfo field = typeof(WaveDirector).GetField("m_WaveCoroutine",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                object coroutine = field != null ? field.GetValue(director) : null;
                Assert(coroutine != null, "wave coroutine is running (mission started)");
            }

            EnemyController[] enemies = Object.FindObjectsByType<EnemyController>();
            EnemyController[] allEnemies = Object.FindObjectsByType<EnemyController>(
                FindObjectsInactive.Include);
            Assert(enemies.Length > 0,
                $"wave 1 spawned enemies (active={enemies.Length}, includingInactive={allEnemies.Length})");
        }

        private static void CheckOverheatRuntime()
        {
            WeaponHeatController heat = Object.FindAnyObjectByType<WeaponHeatController>();
            if (!Assert(heat != null, "WeaponHeatController exists on the live player ship")) return;

            const int shots = 40; // 0.05 heat per shot at level 1 => overheats after 20
            for (int i = 0; i < shots; i++)
            {
                heat.TryFire(1);
            }

            Assert(heat.IsOverheated, $"weapon overheats after {shots} sustained shots");
            Assert(heat.CurrentHeat >= 0.999f, $"heat gauge reached maximum (actual={heat.CurrentHeat:F3})");
            Assert(!heat.TryFire(1), "weapon refuses to fire while overheated (jam works)");

            heat.ResetHeat();
            Assert(!heat.IsOverheated && heat.CurrentHeat <= 0.001f, "ResetHeat unlocks the weapon again");
        }

        // ---------------------------------------------------------------- helpers

        private static void Finish(int exitCode)
        {
            EditorApplication.update -= Tick;
            Debug.Log($"[STARVANGUARD-SMOKE] SUMMARY passed={s_Passed} failed={s_Failed} exitCode={exitCode}");

            if (EditorApplication.isPlaying)
            {
                EditorApplication.ExitPlaymode();
            }

            EditorApplication.Exit(exitCode);
        }

        private static bool Assert(bool condition, string label)
        {
            if (condition)
            {
                s_Passed++;
                Debug.Log($"[STARVANGUARD-SMOKE] PASS {label}");
            }
            else
            {
                s_Failed++;
                Debug.LogError($"[STARVANGUARD-SMOKE] FAIL {label}");
            }

            return condition;
        }

        private static void Fail(string label) => Assert(false, label);
    }
}
