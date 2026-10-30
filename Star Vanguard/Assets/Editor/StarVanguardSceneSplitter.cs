using StarVanguard.UI;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// One-shot maintenance tool that splits the duplicated monolithic scenes into
    /// a menu scene and a gameplay scene.
    ///
    /// All three scenes in this project used to be byte-identical copies of one
    /// 345 KB scene that contained EVERY panel and EVERY gameplay object, with all
    /// panels left active. That is why the menu looked like a pile of overlapping
    /// cards. This tool reports the structure first (Report), then rewrites the
    /// scenes (Apply) and rebuilds the build settings.
    /// </summary>
    public static class StarVanguardSceneSplitter
    {
        private const string k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
        private const string k_MenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string k_GameplayScenePath = "Assets/Scenes/GameplayScene.unity";
        private const string k_SampleScenePath = "Assets/Scenes/SampleScene.unity";

        // Persistent managers isolated in BootstrapScene; stripped from both Gameplay and MainMenu scenes.
        private static readonly string[] k_PersistentManagers =
        {
            "GameManager", "AudioManager", "PoolManager", "LeaderboardManager", "SettingsManager"
        };

        // Panels/HUD that must never be active by default inside a scene file.
        private static readonly string[] k_AlwaysSleepingInScene =
        {
            "HomeScreenPanel", "HomeScreenPanelButtons",
            "SettingsPanel", "AudioSettingsContainer", "ControlsSettingsContainer",
            "ControlsPanel", "MouseControlsContainer", "KeyboardControlsContainer",
            "LeaderboardPanel", "NewHighScorePanel",
            "PausePanel", "PausePanelButtons",
            "GameOverPanel", "GameOverPanelButtons",
            "InterWaveMessagePanel",
            "HUDContainer", "HUDBombsContainer", "HUDLivesContainer",
        };

        // Objects that belong to gameplay only and must be removed from the menu scene.
        // WaveDirector is scene-owned (PersistAcrossScenes = false) and used to start
        // spawning waves inside the menu scene, which is what made the menu feel frozen.
        private static readonly string[] k_GameplayOnlyObjects =
        {
            "Player", "Boundaries",
            "WaveDirector", "InterWaveMessageManager",
            "HUDContainer", "HUDBombsContainer", "HUDLivesContainer",
            "PausePanel", "PausePanelButtons",
            "GameOverPanel", "GameOverPanelButtons",
            "NewHighScorePanel", "InterWaveMessagePanel",
        };

        // Legacy menu cards. The menu, the co-op sheet and the controls manual are rebuilt
        // in code by MainMenuScreen / ControlsScreen.
        private static readonly string[] k_LegacyMenuCards =
        {
            "HomeScreenPanel", "HomeScreenPanelButtons",
            "ControlsPanel", "MouseControlsContainer", "KeyboardControlsContainer",
        };

        // UI panels that only the menu needs; removed from the gameplay scene.
        // SettingsPanel stays in gameplay because the pause menu opens it.
        private static readonly string[] k_MenuOnlyUI =
        {
            "HomeScreenPanel", "HomeScreenPanelButtons",
            "ControlsPanel", "MouseControlsContainer", "KeyboardControlsContainer",
        };

        private const string k_BackupFolder = "SceneBackups";

        [MenuItem("Star Vanguard/Maintenance/Report Scene Structure")]
        public static void Report()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string path in new[] { k_MenuScenePath, k_GameplayScenePath })
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                sb.AppendLine($"[SPLIT-REPORT] === {path} ===");
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    sb.AppendLine($"[SPLIT-REPORT] root: {root.name} active={root.activeSelf} children={root.transform.childCount}");
                    foreach (Transform child in root.transform)
                    {
                        sb.AppendLine($"[SPLIT-REPORT]    - {child.name} active={child.gameObject.activeSelf} kids={child.childCount}");
                    }
                }
            }

            Debug.Log(sb.ToString());
        }

        [MenuItem("Star Vanguard/Maintenance/Apply Scene Split")]
        public static void Apply()
        {
            StringBuilder sb = new StringBuilder();
            Directory.CreateDirectory(k_BackupFolder);

            SplitScene(k_MenuScenePath, true, sb);
            SplitScene(k_GameplayScenePath, false, sb);
            RemoveSampleScene(sb);

            sb.AppendLine("[SPLIT] build settings: " + string.Join(", ", System.Array.ConvertAll(
                EditorBuildSettings.scenes, s => s.path)));
            Debug.Log(sb.ToString());
        }

        private static void SplitScene(string i_Path, bool i_IsMenuScene, StringBuilder io_Log)
        {
            io_Log.AppendLine($"[SPLIT] === {i_Path} (menu={i_IsMenuScene}) ===");

            // Back up first: this tool edits scenes in place.
            string backupName = Path.Combine(k_BackupFolder, Path.GetFileName(i_Path) + ".bak");
            if (!File.Exists(backupName))
            {
                File.Copy(i_Path, backupName, false);
                io_Log.AppendLine($"[SPLIT] backup -> {backupName}");
            }

            Scene scene = EditorSceneManager.OpenScene(i_Path, OpenSceneMode.Single);

            GameObject canvas = FindRoot(scene, "Canvas");
            if (canvas == null)
            {
                io_Log.AppendLine("[SPLIT] ERROR: no Canvas root found");
                return;
            }

            EnsureComponent<UIPanelRouter>(canvas, io_Log);
            if (i_IsMenuScene)
            {
                EnsureComponent<MainMenuScreen>(canvas, io_Log);
            }

            string[] removals = i_IsMenuScene
                ? Concat(Concat(k_GameplayOnlyObjects, k_LegacyMenuCards), k_PersistentManagers)
                : Concat(k_MenuOnlyUI, k_PersistentManagers);

            foreach (string name in removals)
            {
                int removed = DestroyAllNamed(scene, name);
                if (removed > 0)
                {
                    io_Log.AppendLine($"[SPLIT] removed {removed}x {name}");
                }
            }

            int slept = 0;
            foreach (string name in k_AlwaysSleepingInScene)
            {
                foreach (GameObject found in FindAllNamed(scene, name))
                {
                    if (found.activeSelf)
                    {
                        found.SetActive(false);
                        slept++;
                    }
                }
            }

            io_Log.AppendLine($"[SPLIT] deactivated {slept} panel objects");
            NormalizePanelSizes(canvas.transform, io_Log);
            FixBackButtons(scene, io_Log);
            FixBackButtonPlacement(scene, io_Log);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            io_Log.AppendLine("[SPLIT] scene saved");
        }

        private static void RemoveSampleScene(StringBuilder io_Log)
        {
            List<EditorBuildSettingsScene> keep = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                if (entry.path == k_MenuScenePath || entry.path == k_GameplayScenePath)
                {
                    keep.Add(entry);
                }
                else
                {
                    io_Log.AppendLine($"[SPLIT] build settings: dropped {entry.path}");
                }
            }

            EditorBuildSettings.scenes = BuildOrder();

            if (File.Exists(k_SampleScenePath))
            {
                AssetDatabase.DeleteAsset(k_SampleScenePath);
                io_Log.AppendLine($"[SPLIT] deleted {k_SampleScenePath}");
            }
        }

        /// <summary>Bootstrap must be index 0: it instantiates persistent managers once.</summary>
        private static EditorBuildSettingsScene[] BuildOrder()
        {
            return new[]
            {
                new EditorBuildSettingsScene(k_BootstrapScenePath, true),
                new EditorBuildSettingsScene(k_MenuScenePath, true),
                new EditorBuildSettingsScene(k_GameplayScenePath, true),
            };
        }

        [MenuItem("Star Vanguard/Maintenance/Fix Scene Build Order")]
        public static void FixBuildOrder()
        {
            EditorBuildSettings.scenes = BuildOrder();
            Debug.Log("[SPLIT] build order -> " + string.Join(", ",
                System.Array.ConvertAll(EditorBuildSettings.scenes, s => s.path)));
        }

        private static string[] Concat(string[] i_A, string[] i_B)
        {
            string[] result = new string[i_A.Length + i_B.Length];
            i_A.CopyTo(result, 0);
            i_B.CopyTo(result, i_A.Length);
            return result;
        }

        private static GameObject FindRoot(Scene i_Scene, string i_Name)
        {
            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                if (root.name == i_Name) return root;
            }

            return null;
        }

        private static List<GameObject> FindAllNamed(Scene i_Scene, string i_Name)
        {
            List<GameObject> matches = new List<GameObject>();
            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name == i_Name)
                    {
                        matches.Add(candidate.gameObject);
                    }
                }
            }

            return matches;
        }

        private static int DestroyAllNamed(Scene i_Scene, string i_Name)
        {
            List<GameObject> matches = FindAllNamed(i_Scene, i_Name);
            foreach (GameObject match in matches)
            {
                Object.DestroyImmediate(match);
            }

            return matches.Count;
        }

        /// <summary>
        /// No panel may be larger than the 1920x1080 reference. SettingsPanel used to be
        /// 1520x1120, i.e. 40 px taller than the screen, which clipped its own title.
        /// </summary>
        private static void NormalizePanelSizes(Transform i_Canvas, StringBuilder io_Log)
        {
            const float maxWidth = 1920f;
            const float maxHeight = 1040f;

            RectTransform canvasRect = i_Canvas as RectTransform;
            float canvasWidth = canvasRect != null ? canvasRect.rect.width : maxWidth;
            float canvasHeight = canvasRect != null ? canvasRect.rect.height : 1080f;

            foreach (Transform child in i_Canvas)
            {
                if (!(child is RectTransform rect)) continue;
                if (!child.name.Contains("Panel")) continue;

                float height = rect.rect.height;
                float width = rect.rect.width;
                if (height <= maxHeight && width <= maxWidth) continue;

                // These panels stretch with the canvas, so sizeDelta is an OFFSET from the
                // parent size: height = canvasHeight + sizeDelta.y.
                bool stretchesY = Mathf.Abs(rect.anchorMax.y - rect.anchorMin.y) > 0.001f;
                bool stretchesX = Mathf.Abs(rect.anchorMax.x - rect.anchorMin.x) > 0.001f;

                Vector2 size = rect.sizeDelta;
                float targetHeight = Mathf.Min(height, maxHeight);
                float targetWidth = Mathf.Min(width, maxWidth);

                size.y = stretchesY ? targetHeight - canvasHeight : targetHeight;
                size.x = stretchesX ? targetWidth - canvasWidth : targetWidth;
                rect.sizeDelta = size;

                io_Log.AppendLine(
                    $"[SPLIT] resized {child.name} {width:F0}x{height:F0} -> {targetWidth:F0}x{targetHeight:F0}" +
                    $" (stretch x={stretchesX} y={stretchesY}, sizeDelta={size.x:F0},{size.y:F0})");
            }
        }

        /// <summary>
        /// The Settings panel's BACK button still pointed at ControlsPanelController with an
        /// empty target, so pressing it did nothing. Remove every dead persistent call and
        /// wire the button to the router-aware UIManager method instead.
        /// </summary>
        private static void FixBackButtons(Scene i_Scene, StringBuilder io_Log)
        {
            foreach (GameObject panel in FindAllNamed(i_Scene, "SettingsPanel"))
            {
                foreach (Button button in panel.GetComponentsInChildren<Button>(true))
                {
                    if (button.name != "BackButton") continue;

                    // SettingsUI already subscribes its own CloseSettings() ->
                    // UIManager.HideSettingsPanel() at runtime (it is the m_CloseButton of
                    // the panel). A persistent listener as well makes BACK run twice, which
                    // pops the router stack twice and lands on "no panel".
                    for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                    {
                        bool deadTarget = button.onClick.GetPersistentTarget(i) == null;
                        bool duplicateOfRuntimeListener =
                            button.onClick.GetPersistentMethodName(i) == nameof(UIManager.HideSettingsPanel);

                        if (deadTarget || duplicateOfRuntimeListener)
                        {
                            io_Log.AppendLine($"[SPLIT] removed stale listener on {panel.name}/{button.name}: " +
                                              $"{button.onClick.GetPersistentMethodName(i)}" +
                                              (deadTarget ? " (empty target)" : " (duplicate of SettingsUI.CloseSettings)"));
                            UnityEventTools.RemovePersistentListener(button.onClick, i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// SettingsPanel's BACK button sat at (-810, +685) from the panel centre while the
        /// panel is only 1520x1040, so it floated outside the panel (and above the screen)
        /// and could never be clicked. Move any such stray button back inside, bottom centre.
        /// </summary>
        private static void FixBackButtonPlacement(Scene i_Scene, StringBuilder io_Log)
        {
            foreach (GameObject panel in FindAllNamed(i_Scene, "SettingsPanel"))
            {
                RectTransform panelRect = panel.transform as RectTransform;
                if (panelRect == null) continue;

                float halfWidth = panelRect.rect.width * 0.5f;
                float halfHeight = panelRect.rect.height * 0.5f;

                foreach (Button button in panel.GetComponentsInChildren<Button>(true))
                {
                    if (button.name != "BackButton") continue;

                    RectTransform rect = button.GetComponent<RectTransform>();
                    if (rect == null) continue;

                    Vector2 position = rect.anchoredPosition;
                    bool outside = Mathf.Abs(position.x) + (rect.rect.width * 0.5f) > halfWidth
                                   || Mathf.Abs(position.y) + (rect.rect.height * 0.5f) > halfHeight;
                    if (!outside) continue;

                    io_Log.AppendLine($"[SPLIT] {panel.name}/{button.name} was outside the panel at " +
                                      $"({position.x:F0},{position.y:F0}) - moved to bottom centre");

                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    rect.sizeDelta = new Vector2(300f, 100f);
                    rect.anchoredPosition = new Vector2(0f, 40f);
                }
            }
        }

        private static void EnsureComponent<T>(GameObject i_Target, StringBuilder io_Log) where T : Component
        {
            if (i_Target.GetComponent<T>() == null)
            {
                i_Target.AddComponent<T>();
                io_Log.AppendLine($"[SPLIT] added {typeof(T).Name} to {i_Target.name}");
            }
        }
    }
}
