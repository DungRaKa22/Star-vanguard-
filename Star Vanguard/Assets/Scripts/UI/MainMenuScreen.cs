using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// The one and only main menu, built with uGUI/TMP from the shared <see cref="UITheme"/>.
    ///
    /// This replaces the old IMGUI menu in MainMenuUI, which drew a second, differently
    /// styled menu on top of the uGUI panels and was the main reason the menu scene looked
    /// like a pile of overlapping cards.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        private const string k_MenuSceneName = "MainMenuScene";
        private const string k_GameplaySceneName = "GameplayScene";
        private const string k_DifficultyPrefKey = "sv_difficulty";

        private static readonly string[] k_DifficultyNames = { "CADET", "VETERAN", "ACE" };

        private Canvas m_Canvas;
        private GameObject m_Root;
        private GameObject m_CoopPanel;
        private GameObject m_ControlsPanel;
        private TextMeshProUGUI m_BestScoreValue;
        private readonly List<Button> m_DifficultyButtons = new List<Button>(3);
        private readonly List<Image> m_DifficultyFills = new List<Image>(3);

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != k_MenuSceneName)
            {
                enabled = false;
                return;
            }

            Build();
        }

        private void Update()
        {
            // Esc closes the co-op sheet; nothing else in the menu listens for keys yet.
            if (Input.GetKeyDown(KeyCode.Escape) && UIPanelRouter.Instance != null)
            {
                UIPanelRouter router = UIPanelRouter.Instance;
                if (router.Current == UIPanelId.Coop || router.Current == UIPanelId.Settings || router.Current == UIPanelId.Controls)
                {
                    router.Back();
                }
            }
        }

        private void Build()
        {
            m_Canvas = FindAnyObjectByType<Canvas>();
            if (m_Canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                m_Canvas = canvasObject.GetComponent<Canvas>();
                m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            UITheme.ApplyCanvasScaler(m_Canvas);

            // A menu that was duplicated in the editor (Ctrl+D on MenuRoot) sits on top of the real
            // one and swallows clicks. Remove duplicate copies while keeping the primary menu intact.
            RemoveStaleMenus();

            UIPanelRouter router = UIPanelRouter.Instance != null
                ? UIPanelRouter.Instance
                : gameObject.AddComponent<UIPanelRouter>();

            Transform existingRoot = m_Canvas.transform.Find("MenuRoot");
            if (existingRoot != null)
            {
                m_Root = existingRoot.gameObject;
                BindHome(m_Root);
            }
            else
            {
                m_Root = BuildHome();
            }
            router.Register(UIPanelId.Home, m_Root);

            Transform existingCoop = m_Canvas.transform.Find("CoopPanel");
            if (existingCoop != null)
            {
                m_CoopPanel = existingCoop.gameObject;
                BindCoopPanel(m_CoopPanel);
            }
            else
            {
                m_CoopPanel = BuildCoopPanel();
            }
            router.Register(UIPanelId.Coop, m_CoopPanel);

            // Built from InputMap: the legacy scene panel was larger than the screen and
            // covered its own buttons with a raycast backdrop.
            Transform existingControls = m_Canvas.transform.Find("ControlsPanel");
            if (existingControls != null)
            {
                m_ControlsPanel = existingControls.gameObject;
                ControlsScreen.Bind(m_ControlsPanel);
            }
            else
            {
                m_ControlsPanel = ControlsScreen.Build(m_Canvas.transform);
            }
            router.Register(UIPanelId.Controls, m_ControlsPanel);

            Transform existingSettings = m_Canvas.transform.Find("SettingsPanel");
            if (existingSettings != null)
            {
                SettingsScreen.Bind(existingSettings.gameObject);
            }
            else
            {
                GameObject settingsObj = SettingsScreen.Build(m_Canvas.transform);
                settingsObj.SetActive(false);
            }

            if (GameManager.Instance == null || GameManager.Instance.GameState == eGameState.Menu)
            {
                router.Show(UIPanelId.Home);
            }

            DisableUnknownCanvasObjects();
        }

#if UNITY_EDITOR
        [ContextMenu("Bake Main Menu to Scene")]
        public void BakeHierarchy()
        {
            m_Canvas = GetComponentInParent<Canvas>();
            if (m_Canvas == null) m_Canvas = FindAnyObjectByType<Canvas>();
            if (m_Canvas == null)
            {
                Debug.LogError("MainMenuScreen: No Canvas found to bake menu!");
                return;
            }

            UITheme.ApplyCanvasScaler(m_Canvas);

            Transform existingRoot = m_Canvas.transform.Find("MenuRoot");
            if (existingRoot != null) DestroyImmediate(existingRoot.gameObject);

            Transform existingCoop = m_Canvas.transform.Find("CoopPanel");
            if (existingCoop != null) DestroyImmediate(existingCoop.gameObject);

            Transform existingControls = m_Canvas.transform.Find("ControlsPanel");
            if (existingControls != null) DestroyImmediate(existingControls.gameObject);

            Transform existingSettings = m_Canvas.transform.Find("SettingsPanel");
            if (existingSettings != null) DestroyImmediate(existingSettings.gameObject);

            m_Root = BuildHome();
            m_CoopPanel = BuildCoopPanel();
            m_ControlsPanel = ControlsScreen.Build(m_Canvas.transform);
            GameObject settingsPanel = SettingsScreen.Build(m_Canvas.transform);

            m_Root.SetActive(true);
            m_CoopPanel.SetActive(false);
            m_ControlsPanel.SetActive(false);
            settingsPanel.SetActive(false);

            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.EditorUtility.SetDirty(m_Canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);

            Debug.Log("MainMenuScreen: Successfully baked MenuRoot, CoopPanel, ControlsPanel, and SettingsPanel into Scene!");
        }
#endif

        // ------------------------------------------------------------ stale cleanup

        private static readonly string[] k_BuiltNames = { "MenuRoot", "CoopPanel", "ControlsPanel", "SettingsPanel" };

        /// <summary>
        /// Destroys duplicate leftover copies of the screens. Keeps the primary one intact so
        /// editor-baked visual layouts and customizations are preserved.
        /// </summary>
        private void RemoveStaleMenus()
        {
            Transform canvasRoot = m_Canvas.transform;
            foreach (string builtName in k_BuiltNames)
            {
                List<GameObject> matches = new List<GameObject>();
                foreach (Transform child in canvasRoot)
                {
                    if (child.name == builtName) matches.Add(child.gameObject);
                }

                // If multiple copies exist, keep matches[0] and destroy subsequent duplicates
                for (int i = 1; i < matches.Count; i++)
                {
                    Debug.LogWarning($"MainMenuScreen: removing duplicate '{builtName}' object.");
                    Destroy(matches[i]);
                }
            }
        }

        private static T FindChildComponent<T>(Transform i_Parent, string i_Name) where T : Component
        {
            if (i_Parent == null) return null;
            foreach (Transform child in i_Parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == i_Name)
                {
                    T comp = child.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }
            return null;
        }

        private void BindHome(GameObject i_Root)
        {
            m_DifficultyButtons.Clear();
            m_DifficultyFills.Clear();

            Button launchBtn = FindChildComponent<Button>(i_Root.transform, "LaunchButton");
            if (launchBtn != null)
            {
                launchBtn.onClick.RemoveAllListeners();
                launchBtn.onClick.AddListener(OnLaunchMission);
            }

            Button coopBtn = FindChildComponent<Button>(i_Root.transform, "CoopButton");
            if (coopBtn != null)
            {
                coopBtn.onClick.RemoveAllListeners();
                coopBtn.onClick.AddListener(() => OpenPanel(UIPanelId.Coop));
            }

            Button controlsBtn = FindChildComponent<Button>(i_Root.transform, "ControlsButton");
            if (controlsBtn != null)
            {
                controlsBtn.onClick.RemoveAllListeners();
                controlsBtn.onClick.AddListener(() => OpenPanel(UIPanelId.Controls));
            }

            Button settingsBtn = FindChildComponent<Button>(i_Root.transform, "SettingsButton");
            if (settingsBtn != null)
            {
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() => OpenPanel(UIPanelId.Settings));
            }

            Button exitBtn = FindChildComponent<Button>(i_Root.transform, "ExitButton");
            if (exitBtn != null)
            {
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(OnExitGame);
            }

            for (int i = 0; i < k_DifficultyNames.Length; i++)
            {
                int index = i;
                Button diffBtn = FindChildComponent<Button>(i_Root.transform, $"Difficulty_{k_DifficultyNames[i]}");
                if (diffBtn != null)
                {
                    diffBtn.onClick.RemoveAllListeners();
                    diffBtn.onClick.AddListener(() => SelectDifficulty(index));
                    m_DifficultyButtons.Add(diffBtn);

                    Image fill = diffBtn.GetComponent<Image>();
                    m_DifficultyFills.Add(fill);
                }
            }

            Transform bestCard = i_Root.transform.Find("BestScoreCard");
            if (bestCard != null)
            {
                m_BestScoreValue = FindChildComponent<TextMeshProUGUI>(bestCard, "BestValue");
            }
            else
            {
                m_BestScoreValue = FindChildComponent<TextMeshProUGUI>(i_Root.transform, "BestValue");
            }

            RefreshBestScore();
            RefreshDifficultyButtons();
        }

        private void BindCoopPanel(GameObject i_Root)
        {
            Button hostBtn = FindChildComponent<Button>(i_Root.transform, "HostButton");
            if (hostBtn != null)
            {
                hostBtn.onClick.RemoveAllListeners();
                hostBtn.onClick.AddListener(OnHostCoop);
            }

            Button joinBtn = FindChildComponent<Button>(i_Root.transform, "JoinButton");
            if (joinBtn != null)
            {
                joinBtn.onClick.RemoveAllListeners();
                joinBtn.onClick.AddListener(OnJoinCoop);
            }

            Button backBtn = FindChildComponent<Button>(i_Root.transform, "BackButton");
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(() => UIPanelRouter.Instance?.Back());
            }
        }

        /// <summary>
        /// A menu baked into an older version of the scene has different object names, so it
        /// cannot be matched by name. It still draws underneath the real menu and - because its
        /// stretched backdrop is a raycast target - eats every click. Nothing may stay visible on
        /// the menu canvas unless the menu or the router put it there.
        /// </summary>
        private void DisableUnknownCanvasObjects()
        {
            List<GameObject> leftovers = new List<GameObject>();
            foreach (Transform child in m_Canvas.transform)
            {
                if (!child.gameObject.activeSelf) continue;
                if (System.Array.IndexOf(k_BuiltNames, child.name) >= 0) continue;
                if (UIPanelRouter.IsKnownPanelName(child.name)) continue;

                leftovers.Add(child.gameObject);
            }

            if (leftovers.Count == 0) return;

            Debug.LogWarning($"MainMenuScreen: disabling {leftovers.Count} leftover UI object(s) on the menu canvas " +
                             $"({string.Join(", ", leftovers.ConvertAll(go => go.name))}). " +
                             "They come from an older version of the scene and cover the menu buttons.");

            foreach (GameObject candidate in leftovers)
            {
                candidate.SetActive(false);
            }
        }

        // ------------------------------------------------------------------- home

        private GameObject BuildHome()
        {
            RectTransform root = UITheme.CreateRect(m_Canvas.transform, "MenuRoot");
            UITheme.StretchToParent(root);

            // ---- left column: logo + actions ----
            RectTransform column = UITheme.CreateRect(root, "LeftColumn");
            column.anchorMin = new Vector2(0f, 0.5f);
            column.anchorMax = new Vector2(0f, 0.5f);
            column.pivot = new Vector2(0f, 0.5f);
            column.anchoredPosition = new Vector2(UITheme.SafeMargin, 24f);
            column.sizeDelta = new Vector2(520f, 800f);

            VerticalLayoutGroup layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = UITheme.Gap;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = UITheme.CreateText(
                column, "Title", "STAR VANGUARD", UITheme.Display, UITheme.TextPrimary,
                TextAlignmentOptions.Left, FontStyles.Bold);
            title.characterSpacing = 4f;
            Size(title.rectTransform, 520f, 92f);

            TextMeshProUGUI subtitle = UITheme.CreateText(
                column, "Subtitle", "-  A L I E N   O N S L A U G H T  -", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Left, FontStyles.Bold);
            subtitle.characterSpacing = 2f;
            Size(subtitle.rectTransform, 520f, 28f);

            Image divider = UITheme.CreateRect(column, "Divider").gameObject.AddComponent<Image>();
            divider.color = new Color(UITheme.Cyan.r, UITheme.Cyan.g, UITheme.Cyan.b, 0.65f);
            divider.raycastTarget = false;
            Size(divider.rectTransform, 320f, 2f);
            AddSpacer(column, 18f);

            UITheme.CreateButton(column, "LaunchButton", "LAUNCH MISSION", UITheme.ButtonWidth, UITheme.ButtonHeight,
                true, OnLaunchMission);
            UITheme.CreateButton(column, "CoopButton", "CO-OP MULTIPLAYER", UITheme.ButtonWidth, UITheme.ButtonHeight,
                false, () => OpenPanel(UIPanelId.Coop));
            UITheme.CreateButton(column, "ControlsButton", "CONTROLS & MANUAL", UITheme.ButtonWidth, UITheme.ButtonHeight,
                false, () => OpenPanel(UIPanelId.Controls));
            UITheme.CreateButton(column, "SettingsButton", "SETTINGS", UITheme.ButtonWidth, UITheme.ButtonHeight,
                false, () => OpenPanel(UIPanelId.Settings));
            UITheme.CreateButton(column, "ExitButton", "EXIT GAME", UITheme.ButtonWidth, UITheme.ButtonHeight,
                false, OnExitGame);

            AddSpacer(column, 24f);
            BuildDifficultyRow(column);

            // ---- right side: best score card ----
            Image card = UITheme.CreatePanelCard(root, "BestScoreCard", new Vector2(360f, 148f));
            RectTransform cardRect = card.rectTransform;
            cardRect.anchorMin = new Vector2(1f, 0f);
            cardRect.anchorMax = new Vector2(1f, 0f);
            cardRect.pivot = new Vector2(1f, 0f);
            cardRect.anchoredPosition = new Vector2(-UITheme.SafeMargin, 88f);

            TextMeshProUGUI bestLabel = UITheme.CreateText(
                cardRect, "BestLabel", "BEST SCORE", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Center, FontStyles.Bold);
            bestLabel.characterSpacing = 3f;
            bestLabel.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            bestLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            bestLabel.rectTransform.offsetMin = new Vector2(24f, 0f);
            bestLabel.rectTransform.offsetMax = new Vector2(-24f, -20f);

            m_BestScoreValue = UITheme.CreateText(
                cardRect, "BestValue", "000000", UITheme.H1, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            m_BestScoreValue.rectTransform.anchorMin = new Vector2(0f, 0f);
            m_BestScoreValue.rectTransform.anchorMax = new Vector2(1f, 0.6f);
            m_BestScoreValue.rectTransform.offsetMin = new Vector2(24f, 18f);
            m_BestScoreValue.rectTransform.offsetMax = new Vector2(-24f, 0f);

            // ---- footer: the key legend lives in exactly one place ----
            TextMeshProUGUI footer = UITheme.CreateText(
                root, "Footer",
                "WASD / ARROWS / MOUSE   ·   SPACE FIRE   ·   SHIFT BOMB   ·   ESC PAUSE",
                UITheme.Caption, UITheme.TextMuted, TextAlignmentOptions.Left);
            footer.rectTransform.anchorMin = new Vector2(0f, 0f);
            footer.rectTransform.anchorMax = new Vector2(0f, 0f);
            footer.rectTransform.pivot = new Vector2(0f, 0f);
            footer.rectTransform.anchoredPosition = new Vector2(UITheme.SafeMargin, 32f);
            footer.rectTransform.sizeDelta = new Vector2(900f, 24f);

            RefreshBestScore();
            RefreshDifficultyButtons();
            return root.gameObject;
        }

        private void BuildDifficultyRow(Transform i_Parent)
        {
            TextMeshProUGUI label = UITheme.CreateText(
                i_Parent, "DifficultyLabel", "DIFFICULTY", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Left, FontStyles.Bold);
            label.characterSpacing = 3f;
            Size(label.rectTransform, 520f, 22f);

            RectTransform row = UITheme.CreateRect(i_Parent, "DifficultyRow");
            Size(row, 520f, UITheme.ButtonHeightSmall);

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < k_DifficultyNames.Length; i++)
            {
                int index = i;
                Button button = UITheme.CreateButton(
                    row, $"Difficulty_{k_DifficultyNames[i]}", k_DifficultyNames[i], 160f,
                    UITheme.ButtonHeightSmall, false, () => SelectDifficulty(index));

                m_DifficultyButtons.Add(button);
                LayoutElement element = button.GetComponent<LayoutElement>();
                if (element != null) element.preferredWidth = 160f;

                Image fill = button.GetComponent<Image>();
                m_DifficultyFills.Add(fill);
            }
        }

        private void SelectDifficulty(int i_Index)
        {
            AudioManager.Instance?.Play(eSFXId.UIClick);
            int tier = i_Index + 1;
            GameManager.Instance?.SetSelectedDifficulty(tier);
            PlayerPrefs.SetInt(k_DifficultyPrefKey, tier);
            PlayerPrefs.Save();
            RefreshDifficultyButtons();
        }

        private void RefreshDifficultyButtons()
        {
            int selected = GameManager.Instance != null ? GameManager.Instance.SelectedDifficulty : 1;

            for (int i = 0; i < m_DifficultyFills.Count; i++)
            {
                if (m_DifficultyFills[i] == null) continue;

                bool isSelected = (i + 1) == selected;
                m_DifficultyFills[i].color = isSelected ? UITheme.ButtonFillPrimary : UITheme.ButtonFill;

                TextMeshProUGUI label = m_DifficultyButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.color = isSelected ? UITheme.Cyan : UITheme.TextMuted;
                }
            }
        }

        private void RefreshBestScore()
        {
            if (m_BestScoreValue == null) return;

            int best = LeaderboardManager.Instance != null ? LeaderboardManager.Instance.GetHighestScore() : 0;
            m_BestScoreValue.SetText("{0:000000}", best);
        }

        // ------------------------------------------------------------------- coop

        private GameObject BuildCoopPanel()
        {
            RectTransform root = UITheme.CreateRect(m_Canvas.transform, "CoopPanel");
            UITheme.StretchToParent(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0.02f, 0.06f, 0.82f);

            Image card = UITheme.CreatePanelCard(root, "CoopCard", new Vector2(UITheme.FormPanelWidth, 460f));
            card.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            card.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            card.rectTransform.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = UITheme.CreateText(
                card.rectTransform, "Title", "CO-OP MULTIPLAYER", UITheme.H1, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            title.characterSpacing = 3f;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -UITheme.PanelPadding);
            title.rectTransform.sizeDelta = new Vector2(-80f, 60f);

            TextMeshProUGUI info = UITheme.CreateText(
                card.rectTransform, "Info", "LAN PROTOCOL  ·  PORT 7777  ·  UP TO 4 WINGMEN",
                UITheme.Caption, UITheme.TextMuted, TextAlignmentOptions.Center);
            info.rectTransform.anchorMin = new Vector2(0f, 1f);
            info.rectTransform.anchorMax = new Vector2(1f, 1f);
            info.rectTransform.pivot = new Vector2(0.5f, 1f);
            info.rectTransform.anchoredPosition = new Vector2(0f, -110f);
            info.rectTransform.sizeDelta = new Vector2(-80f, 26f);

            RectTransform buttons = UITheme.CreateRect(card.rectTransform, "Buttons");
            buttons.anchorMin = new Vector2(0.5f, 0.5f);
            buttons.anchorMax = new Vector2(0.5f, 0.5f);
            buttons.pivot = new Vector2(0.5f, 0.5f);
            buttons.anchoredPosition = new Vector2(0f, -24f);
            buttons.sizeDelta = new Vector2(UITheme.ButtonWidth, 220f);

            VerticalLayoutGroup layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = UITheme.Gap;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            UITheme.CreateButton(buttons, "HostButton", "HOST SESSION", UITheme.ButtonWidth, UITheme.ButtonHeight,
                true, OnHostCoop);
            UITheme.CreateButton(buttons, "JoinButton", "JOIN WINGMAN", UITheme.ButtonWidth, UITheme.ButtonHeight,
                false, OnJoinCoop);
            UITheme.CreateButton(buttons, "BackButton", "BACK", UITheme.ButtonWidth, UITheme.ButtonHeightSmall,
                false, () => UIPanelRouter.Instance?.Back());

            return root.gameObject;
        }

        // ----------------------------------------------------------------- actions

        private void OpenPanel(UIPanelId i_Id)
        {
            AudioManager.Instance?.Play(eSFXId.UIClick);
            UIPanelRouter router = UIPanelRouter.Instance;
            if (router == null) return;

            if (router.Has(i_Id))
            {
                router.Push(i_Id);
            }
            else
            {
                Debug.LogWarning($"MainMenuScreen: panel {i_Id} is not present in this scene.");
            }
        }

        private void OnLaunchMission()
        {
            AudioManager.Instance?.Play(eSFXId.UIClick);
            Time.timeScale = 1f;
            SceneManager.LoadScene(k_GameplaySceneName);
        }

        private void OnHostCoop()
        {
            AudioManager.Instance?.Play(eSFXId.UIClick);
            NetworkLobbyUI lobby = FindAnyObjectByType<NetworkLobbyUI>();
            if (lobby != null)
            {
                lobby.StartHostLAN();
            }
            else
            {
                OnLaunchMission();
            }
        }

        private void OnJoinCoop()
        {
            AudioManager.Instance?.Play(eSFXId.UIClick);
            NetworkLobbyUI lobby = FindAnyObjectByType<NetworkLobbyUI>();
            if (lobby != null)
            {
                lobby.JoinClientLAN();
            }
            else
            {
                OnLaunchMission();
            }
        }

        private void OnExitGame()
        {
            AudioManager.Instance?.Play(eSFXId.UIClick);
            GameManager.Instance?.QuitApplication();
        }

        // ----------------------------------------------------------------- helpers

        private static void Size(RectTransform i_Rect, float i_Width, float i_Height)
        {
            i_Rect.sizeDelta = new Vector2(i_Width, i_Height);
            LayoutElement element = i_Rect.GetComponent<LayoutElement>();
            if (element == null) element = i_Rect.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = i_Width;
            element.preferredHeight = i_Height;
        }

        private static void AddSpacer(Transform i_Parent, float i_Height)
        {
            RectTransform spacer = UITheme.CreateRect(i_Parent, "Spacer");
            spacer.sizeDelta = new Vector2(1f, i_Height);
            LayoutElement element = spacer.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = i_Height;
            element.preferredWidth = 1f;
        }
    }
}
