using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarVanguard.UI
{
    /// <summary>Every screen the player can be looking at. Exactly one is visible at a time.</summary>
    public enum UIPanelId
    {
        None = 0,
        Home = 1,
        Settings = 2,
        Controls = 3,
        Leaderboard = 4,
        Coop = 5,
        Pause = 6,
        GameOver = 7,
        NewHighScore = 8,
    }

    /// <summary>
    /// The only thing allowed to switch panels on and off.
    ///
    /// Before this router every script called SetActive on whichever panel it knew
    /// about, so two panels could be open at once and the "back" button had to guess
    /// where to return. Now: one visible panel, an explicit back-stack, and a short
    /// fade so panels never pop in on top of each other.
    /// </summary>
    public class UIPanelRouter : MonoBehaviour
    {
        public static UIPanelRouter Instance { get; private set; }
        public static event System.Action<UIPanelId, GameObject> OnPanelRegistered;

        // Scene objects are discovered by name so the router keeps working even if a
        // designer renames nothing but moves panels around the hierarchy.
        private static readonly (UIPanelId Id, string ObjectName)[] k_ScenePanels =
        {
            (UIPanelId.Settings, "SettingsPanel"),
            (UIPanelId.Controls, "ControlsPanel"),
            (UIPanelId.Coop, "CoopModalPanel"),
            (UIPanelId.Leaderboard, "LeaderboardPanel"),
            (UIPanelId.Pause, "PausePanel"),
            (UIPanelId.GameOver, "GameOverPanel"),
            (UIPanelId.NewHighScore, "NewHighScorePanel"),
        };

        private const float k_FadeDuration = 0.12f;

        // Temporary switch used by the headless smoke test to trace navigation calls.
        public static bool s_DebugTrace;

        private readonly Dictionary<UIPanelId, GameObject> m_Panels = new Dictionary<UIPanelId, GameObject>();
        private readonly Dictionary<UIPanelId, CanvasGroup> m_Groups = new Dictionary<UIPanelId, CanvasGroup>();
        private readonly Dictionary<UIPanelId, Coroutine> m_Fades = new Dictionary<UIPanelId, Coroutine>();
        private readonly List<UIPanelId> m_Stack = new List<UIPanelId>();

        public UIPanelId Current => m_Stack.Count > 0 ? m_Stack[m_Stack.Count - 1] : UIPanelId.None;
        public int StackDepth => m_Stack.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DiscoverPanels();
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            UIManager.OnShowSettingsRequested += HandleShowSettings;
            UIManager.OnHideSettingsRequested += HandleHideSettings;
            UIManager.OnShowControlsRequested += HandleShowControls;
            UIManager.OnHideControlsRequested += HandleHideControls;
            UIManager.OnShowAudioSettingsRequested += HandleShowAudioSettings;
            UIManager.OnHideSubPanelsRequested += HandleHideSubPanels;
            UIManager.OnHideAllPanelsRequested += HandleHideAll;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            UIManager.OnShowSettingsRequested -= HandleShowSettings;
            UIManager.OnHideSettingsRequested -= HandleHideSettings;
            UIManager.OnShowControlsRequested -= HandleShowControls;
            UIManager.OnHideControlsRequested -= HandleHideControls;
            UIManager.OnShowAudioSettingsRequested -= HandleShowAudioSettings;
            UIManager.OnHideSubPanelsRequested -= HandleHideSubPanels;
            UIManager.OnHideAllPanelsRequested -= HandleHideAll;
        }

        private void Start()
        {
            // Sync with current game state on startup if no panel is currently active
            if (GameManager.Instance != null && m_Stack.Count == 0)
            {
                HandleGameStateChanged(GameManager.Instance.GameState);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void DiscoverPanels()
        {
            EnsureModernGameplayPanels();

            foreach ((UIPanelId id, string objectName) in k_ScenePanels)
            {
                GameObject panel = FindInScene(objectName);
                if (panel != null)
                {
                    Register(id, panel);
                }
            }
        }

        private void EnsureModernGameplayPanels()
        {
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // In MainMenuScene, MainMenuScreen builds and manages its own panels.
            if (UnityEngine.Object.FindAnyObjectByType<MainMenuScreen>() != null) return;

            // 1. Ensure modern PausePanel
            GameObject existingPause = FindInScene("PausePanel");
            bool pauseNeedsRebuild = existingPause == null || existingPause.transform.Find("Card") == null;
            if (pauseNeedsRebuild)
            {
                if (existingPause != null)
                {
                    DestroyImmediate(existingPause);
                }
                GameObject pauseObj = PauseScreen.Build(canvas.transform);
                pauseObj.SetActive(false);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.SetPausePanel(pauseObj);
                }
            }

            // 2. Ensure modern SettingsPanel
            GameObject existingSettings = FindInScene("SettingsPanel");
            bool settingsNeedsRebuild = existingSettings == null || existingSettings.transform.Find("Card") == null;
            if (settingsNeedsRebuild)
            {
                if (existingSettings != null)
                {
                    DestroyImmediate(existingSettings);
                }
                GameObject settingsObj = SettingsScreen.Build(canvas.transform);
                settingsObj.SetActive(false);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.SetSettingsPanel(settingsObj);
                }
            }

            // 3. Ensure GameOverPanel buttons are active if present
            GameObject existingGameOver = FindInScene("GameOverPanel");
            if (existingGameOver != null)
            {
                Transform btnContainer = existingGameOver.transform.Find("GameOverPanelButtons");
                if (btnContainer != null && !btnContainer.gameObject.activeSelf)
                {
                    btnContainer.gameObject.SetActive(true);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenuScene")
            {
                if (Current == UIPanelId.Settings || Current == UIPanelId.Controls)
                {
                    Back();
                }
            }
        }

        private GameObject FindInScene(string i_Name)
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Transform[] all = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform candidate in all)
                {
                    if (candidate.name == i_Name)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        /// <summary>Adds a panel (scene object or built at runtime) and hides it.</summary>
        public void Register(UIPanelId i_Id, GameObject i_Panel)
        {
            if (i_Panel == null) return;

            m_Panels[i_Id] = i_Panel;

            CanvasGroup group = i_Panel.GetComponent<CanvasGroup>();
            if (group == null) group = i_Panel.AddComponent<CanvasGroup>();
            m_Groups[i_Id] = group;

            SetPanelVisible(i_Id, false, true);

            OnPanelRegistered?.Invoke(i_Id, i_Panel);
        }

        public bool Has(UIPanelId i_Id) => m_Panels.TryGetValue(i_Id, out GameObject panel) && panel != null;

        /// <summary>True when the router owns a scene panel with this object name.</summary>
        public static bool IsKnownPanelName(string i_Name)
        {
            foreach ((UIPanelId id, string objectName) in k_ScenePanels)
            {
                if (objectName == i_Name) return true;
            }

            return false;
        }

        /// <summary>Shows one panel and forgets the navigation history (state-driven screens).</summary>
        public void Show(UIPanelId i_Id)
        {
            m_Stack.Clear();
            if (i_Id != UIPanelId.None)
            {
                m_Stack.Add(i_Id);
            }

            ApplyVisibility(i_Id);
        }

        /// <summary>Opens a panel on top of the current one; Back returns to it.</summary>
        public void Push(UIPanelId i_Id)
        {
            if (i_Id == UIPanelId.None) return;

            if (m_Stack.Count > 0 && m_Stack[m_Stack.Count - 1] == i_Id) return;

            m_Stack.Add(i_Id);
            ApplyVisibility(i_Id);
        }

        /// <summary>Closes the top panel and returns to the previous one.</summary>
        public void Back()
        {
            if (m_Stack.Count == 0) return;

            if (s_DebugTrace)
            {
                Debug.LogWarning($"[UI-ROUTER] Back() depth={m_Stack.Count}\n{new System.Diagnostics.StackTrace(true)}");
            }

            m_Stack.RemoveAt(m_Stack.Count - 1);
            UIPanelId next = m_Stack.Count > 0 ? m_Stack[m_Stack.Count - 1] : UIPanelId.None;
            ApplyVisibility(next);
        }

        public void HideAll()
        {
            if (s_DebugTrace)
            {
                Debug.LogWarning($"[UI-ROUTER] HideAll() depth={m_Stack.Count}\n{new System.Diagnostics.StackTrace(true)}");
            }

            m_Stack.Clear();
            ApplyVisibility(UIPanelId.None);
        }

        #region Navigation Event Handlers
        private void HandleGameStateChanged(eGameState i_State)
        {
            switch (i_State)
            {
                case eGameState.Menu:
                    Show(UIPanelId.Home);
                    break;
                case eGameState.Paused:
                    Show(UIPanelId.Pause);
                    break;
                case eGameState.GameOver:
                    Show(UIPanelId.GameOver);
                    break;
                case eGameState.Playing:
                    HideAll();
                    break;
            }
        }

        private void HandleShowSettings() => Push(UIPanelId.Settings);

        private void HandleHideSettings()
        {
            if (Current == UIPanelId.Settings)
            {
                Back();
            }
        }

        private void HandleShowControls() => Push(UIPanelId.Controls);

        private void HandleHideControls()
        {
            if (Current == UIPanelId.Controls)
            {
                Back();
            }
        }

        private void HandleShowAudioSettings() => Push(UIPanelId.Settings);

        private void HandleHideSubPanels()
        {
            if (Current == UIPanelId.Settings || Current == UIPanelId.Controls)
            {
                Back();
            }
        }

        private void HandleHideAll() => HideAll();
        #endregion

        private void ApplyVisibility(UIPanelId i_Visible)
        {
            foreach (UIPanelId id in m_Panels.Keys)
            {
                SetPanelVisible(id, id == i_Visible, false);
            }
        }

        private void SetPanelVisible(UIPanelId i_Id, bool i_Visible, bool i_Instant)
        {
            if (!m_Panels.TryGetValue(i_Id, out GameObject panel) || panel == null) return;
            if (!m_Groups.TryGetValue(i_Id, out CanvasGroup group) || group == null)
            {
                group = panel.AddComponent<CanvasGroup>();
                m_Groups[i_Id] = group;
            }

            if (m_Fades.TryGetValue(i_Id, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            m_Fades[i_Id] = null;

            if (i_Visible)
            {
                panel.SetActive(true);
                group.blocksRaycasts = true;
                group.interactable = true;

                if (i_Instant || !isActiveAndEnabled)
                {
                    group.alpha = 1f;
                }
                else
                {
                    group.alpha = 0f;
                    m_Fades[i_Id] = StartCoroutine(FadeTo(group, 1f));
                }
            }
            else
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                group.alpha = 0f;
                if (panel.activeSelf)
                {
                    panel.SetActive(false);
                }
            }
        }

        private IEnumerator FadeTo(CanvasGroup i_Group, float i_Target)
        {
            float start = i_Group.alpha;
            float elapsed = 0f;
            while (elapsed < k_FadeDuration)
            {
                // Unscaled: panels must still fade while the game is paused.
                elapsed += Time.unscaledDeltaTime;
                i_Group.alpha = Mathf.Lerp(start, i_Target, elapsed / k_FadeDuration);
                yield return null;
            }

            i_Group.alpha = i_Target;
        }
    }
}
