using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using StarVanguard.UI;

namespace StarVanguard.Editor
{
    /// <summary>
    /// Automated builder for Star Vanguard UI Prefabs based on Google Stitch MCP design specifications.
    /// Converts Stitch dark sci-fi arcade space shooter design tokens to Unity 6 uGUI Prefabs:
    /// 1. MainMenuScreen.prefab (T-401)
    /// 2. OverheatGaugeUI.prefab (T-201)
    /// </summary>
    public static class StarVanguardUIPrefabBuilder
    {
        private const string k_OutputDir = "Assets/Prefabs/UI_Prefabs";
        private const string k_MainMenuPrefabPath = "Assets/Prefabs/UI_Prefabs/MainMenuScreen.prefab";
        private const string k_OverheatPrefabPath = "Assets/Prefabs/UI_Prefabs/OverheatGaugeUI.prefab";

        // Stitch Design Tokens
        private static readonly Color k_ColorVoid = new Color(0.043f, 0.055f, 0.090f, 1f);      // #0B0E17
        private static readonly Color k_ColorSurface = new Color(0.063f, 0.075f, 0.110f, 1f);   // #10131C
        private static readonly Color k_ColorContainer = new Color(0.094f, 0.125f, 0.196f, 0.9f);// #182032
        private static readonly Color k_ColorBorder = new Color(0.180f, 0.239f, 0.333f, 0.8f);  // #2E3D55
        private static readonly Color k_ColorCyan = new Color(0.0f, 0.941f, 1.0f, 1f);          // #00F0FF (Neon Cyan)
        private static readonly Color k_ColorOrange = new Color(1.0f, 0.420f, 0.0f, 1f);        // #FF6B00 (Plasma Orange)
        private static readonly Color k_ColorRed = new Color(1.0f, 0.165f, 0.165f, 1f);         // #FF2A2A (Alert Red)
        private static readonly Color k_ColorTextLight = new Color(0.878f, 0.886f, 0.937f, 1f); // #E0E2EF
        private static readonly Color k_ColorTextSlate = new Color(0.494f, 0.576f, 0.702f, 1f); // #7E93B3

        [MenuItem("Tools/Star Vanguard/Build UI Prefabs from Stitch Spec")]
        public static void BuildAllPrefabs()
        {
            if (!Directory.Exists(k_OutputDir))
            {
                Directory.CreateDirectory(k_OutputDir);
            }

            Debug.Log("[StarVanguardUIPrefabBuilder] Building MainMenuScreen.prefab...");
            BuildMainMenuPrefab();

            Debug.Log("[StarVanguardUIPrefabBuilder] Building OverheatGaugeUI.prefab...");
            BuildOverheatGaugePrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[StarVanguardUIPrefabBuilder] All UI Prefabs successfully built and saved!");
        }

        public static void BuildMainMenuPrefab()
        {
            // 1. Root Canvas
            GameObject root = new GameObject("MainMenuScreen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<MainMenuScreen>();
            root.AddComponent<UIPanelRouter>();

            // 2. Background Panel (#0B0E17)
            GameObject bgObj = CreateRectChild(root, "BackgroundPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = k_ColorVoid;

            // 3. Cockpit Frame / Ambient Border
            GameObject frameObj = CreateRectChild(bgObj, "CockpitFrame", Vector2.zero, Vector2.one, new Vector2(24, 24), new Vector2(-24, -24));
            Image frameImg = frameObj.AddComponent<Image>();
            frameImg.color = new Color(k_ColorBorder.r, k_ColorBorder.g, k_ColorBorder.b, 0.25f);

            // 4. MenuRoot (Main home panel)
            GameObject menuRoot = CreateRectChild(root, "MenuRoot", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Header Container (Top-Center)
            GameObject headerObj = CreateRectChild(menuRoot, "HeaderContainer", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -110), new Vector2(1100, 180));
            VerticalLayoutGroup headerLayout = headerObj.AddComponent<VerticalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.spacing = 6;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = false;

            // Title: STAR VANGUARD (#00F0FF)
            GameObject titleObj = CreateTextChild(headerObj, "TitleText", "STAR VANGUARD", 58, FontStyles.Bold, k_ColorCyan);
            // Subtitle: CHIẾN CƠ KHÔNG GIAN (#FF6B00)
            GameObject subtitleObj = CreateTextChild(headerObj, "SubtitleText", "CHIẾN CƠ KHÔNG GIAN", 24, FontStyles.Bold, k_ColorOrange);
            // Telemetry Badge
            GameObject badgeObj = CreateTextChild(headerObj, "TelemetryBadge", "[ v1.0.4 ARCADE PROTOCOL // SYSTEM ONLINE ]", 13, FontStyles.Normal, k_ColorTextSlate);

            // Vertical Buttons Container (Center)
            GameObject buttonsContainer = CreateRectChild(menuRoot, "ButtonsContainer", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(460, 360));
            VerticalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<VerticalLayoutGroup>();
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.spacing = 16;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = false;

            // 4 Core Vertical Buttons (Stitch Spec)
            CreateSciFiButton(buttonsContainer, "LaunchButton", "BẮT ĐẦU CHIẾN DỊCH", 64, k_ColorCyan, true);
            CreateSciFiButton(buttonsContainer, "LeaderboardButton", "BẢNG XẾP HẠNG", 56, k_ColorTextLight, false);
            CreateSciFiButton(buttonsContainer, "ControlsButton", "HƯỚNG DẪN CHIẾN ĐẤU", 56, k_ColorTextLight, false);
            CreateSciFiButton(buttonsContainer, "SettingsButton", "CÀI ĐẶT HỆ THỐNG", 56, k_ColorTextLight, false);
            CreateSciFiButton(buttonsContainer, "ExitButton", "THOÁT", 56, k_ColorOrange, false);

            // Difficulty Row (Bottom-Center)
            GameObject diffRow = CreateRectChild(menuRoot, "DifficultyRow", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(460, 48));
            HorizontalLayoutGroup diffLayout = diffRow.AddComponent<HorizontalLayoutGroup>();
            diffLayout.childAlignment = TextAnchor.MiddleCenter;
            diffLayout.spacing = 12;
            diffLayout.childControlWidth = true;
            diffLayout.childControlHeight = true;

            string[] diffNames = { "CADET", "VETERAN", "ACE" };
            foreach (string diff in diffNames)
            {
                CreateSciFiButton(diffRow, $"Difficulty_{diff}", diff, 42, k_ColorTextSlate, false);
            }

            // Best Score Card (Bottom-Right)
            GameObject scoreCard = CreateRectChild(menuRoot, "BestScoreCard", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-220, 110), new Vector2(360, 130));
            Image cardBg = scoreCard.AddComponent<Image>();
            cardBg.color = k_ColorContainer;

            VerticalLayoutGroup cardLayout = scoreCard.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(20, 20, 16, 16);
            cardLayout.spacing = 4;
            cardLayout.childAlignment = TextAnchor.UpperLeft;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;

            CreateTextChild(scoreCard, "CardLabel", "THÀNH TÍCH CAO NHẤT", 12, FontStyles.Bold, k_ColorTextSlate);
            CreateTextChild(scoreCard, "BestValue", "284,950", 32, FontStyles.Bold, k_ColorCyan);
            CreateTextChild(scoreCard, "PilotStatus", "PILOT: SQUADRON ACE [READY]", 11, FontStyles.Normal, k_ColorOrange);

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(root, k_MainMenuPrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[StarVanguardUIPrefabBuilder] Saved: {k_MainMenuPrefabPath}");
        }

        public static void BuildOverheatGaugePrefab()
        {
            // Root HUD Canvas
            GameObject root = new GameObject("OverheatGaugeUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 1. Top-Left Vitals (Score & Lives Overlay from Stitch spec)
            GameObject vitalsObj = CreateRectChild(root, "TopLeftVitals", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(230, -80), new Vector2(400, 110));
            Image vitalsBg = vitalsObj.AddComponent<Image>();
            vitalsBg.color = new Color(k_ColorSurface.r, k_ColorSurface.g, k_ColorSurface.b, 0.85f);

            VerticalLayoutGroup vitalsLayout = vitalsObj.AddComponent<VerticalLayoutGroup>();
            vitalsLayout.padding = new RectOffset(20, 20, 12, 12);
            vitalsLayout.spacing = 4;
            vitalsLayout.childAlignment = TextAnchor.UpperLeft;
            vitalsLayout.childControlWidth = true;
            vitalsLayout.childControlHeight = false;

            CreateTextChild(vitalsObj, "ScoreLabel", "ĐIỂM CHIẾN DỊCH: 284,950", 18, FontStyles.Bold, k_ColorCyan);
            CreateTextChild(vitalsObj, "ComboLabel", "HỆ SỐ COMBO: x8.4  |  WAVE 04/12", 13, FontStyles.Normal, k_ColorOrange);
            CreateTextChild(vitalsObj, "LivesLabel", "CHIẾN CƠ DỰ BỊ: 3x [ACTIVE]", 12, FontStyles.Bold, k_ColorTextLight);

            // 2. Bottom-Left Overheat HUD (OverheatGaugeUI container)
            GameObject gaugeRoot = CreateRectChild(root, "OverheatGaugeContainer", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(240, 95), new Vector2(440, 130));
            Image gaugeBg = gaugeRoot.AddComponent<Image>();
            gaugeBg.color = new Color(k_ColorSurface.r, k_ColorSurface.g, k_ColorSurface.b, 0.90f);

            OverheatGaugeUI gaugeComp = gaugeRoot.AddComponent<OverheatGaugeUI>();

            // Layout for Overheat Container
            VerticalLayoutGroup gaugeLayout = gaugeRoot.AddComponent<VerticalLayoutGroup>();
            gaugeLayout.padding = new RectOffset(20, 20, 14, 14);
            gaugeLayout.spacing = 8;
            gaugeLayout.childAlignment = TextAnchor.UpperLeft;
            gaugeLayout.childControlWidth = true;
            gaugeLayout.childControlHeight = false;

            // Header Row: Label + Status Text
            GameObject headerRow = CreateRectChild(gaugeRoot, "HeaderRow", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, 24));
            HorizontalLayoutGroup headerHLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerHLayout.childAlignment = TextAnchor.MiddleLeft;
            headerHLayout.childControlWidth = true;
            headerHLayout.childControlHeight = true;

            GameObject labelTextObj = CreateTextChild(headerRow, "LabelText", "NHIỆT ĐỘ VŨ KHÍ", 14, FontStyles.Bold, k_ColorCyan);
            GameObject statusTextObj = CreateTextChild(headerRow, "StatusText", "OPTIMAL (0%)", 13, FontStyles.Bold, k_ColorOrange);
            TextMeshProUGUI statusTMP = statusTextObj.GetComponent<TextMeshProUGUI>();
            statusTMP.alignment = TextAlignmentOptions.Right;

            // Slider Component
            GameObject sliderObj = CreateRectChild(gaugeRoot, "HeatSlider", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(400, 28));
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;

            // Slider Background
            GameObject sliderBgObj = CreateRectChild(sliderObj, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image sliderBg = sliderBgObj.AddComponent<Image>();
            sliderBg.color = new Color(0.08f, 0.10f, 0.15f, 1f);

            // Fill Area & Fill Image
            GameObject fillArea = CreateRectChild(sliderObj, "Fill Area", Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
            GameObject fillObj = CreateRectChild(fillArea, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = k_ColorCyan;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            slider.targetGraphic = fillImage;
            slider.fillRect = fillObj.GetComponent<RectTransform>();

            // Footer / Cooling prompt
            CreateTextChild(gaugeRoot, "VentingPrompt", "[SPACE] LÀM NGUỘI THỦ CÔNG  |  BURST DISCHARGE", 10, FontStyles.Normal, k_ColorTextSlate);

            // Wire Serialized Properties to OverheatGaugeUI
            SerializedObject so = new SerializedObject(gaugeComp);
            SerializedProperty sliderProp = so.FindProperty("m_HeatSlider");
            if (sliderProp != null) sliderProp.objectReferenceValue = slider;

            SerializedProperty fillProp = so.FindProperty("m_HeatFillImage");
            if (fillProp != null) fillProp.objectReferenceValue = fillImage;

            SerializedProperty textProp = so.FindProperty("m_HeatStatusText");
            if (textProp != null) textProp.objectReferenceValue = statusTMP;

            SerializedProperty procProp = so.FindProperty("m_EnableProceduralHUD");
            if (procProp != null) procProp.boolValue = false; // Disable OnGUI IMGUI HUD!

            so.ApplyModifiedPropertiesWithoutUndo();

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(root, k_OverheatPrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[StarVanguardUIPrefabBuilder] Saved: {k_OverheatPrefabPath}");
        }

        // ------------------------------------------------------------- Helpers
        private static GameObject CreateRectChild(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent.transform, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return obj;
        }

        private static GameObject CreateTextChild(GameObject parent, string name, string text, float fontSize, FontStyles style, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent.transform, false);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = false;
            return obj;
        }

        private static GameObject CreateSciFiButton(GameObject parent, string name, string label, float height, Color accentColor, bool isPrimary)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnObj.transform.SetParent(parent.transform, false);

            LayoutElement le = btnObj.GetComponent<LayoutElement>();
            le.preferredHeight = height;

            Image img = btnObj.GetComponent<Image>();
            img.color = isPrimary ? new Color(0.09f, 0.16f, 0.28f, 0.95f) : k_ColorContainer;

            Button btn = btnObj.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.0f, 0.941f, 1.0f, 0.8f);
            cb.pressedColor = new Color(1.0f, 0.420f, 0.0f, 0.9f);
            btn.colors = cb;

            // Child Text
            GameObject txtObj = CreateTextChild(btnObj, "Text", label, 18, isPrimary ? FontStyles.Bold : FontStyles.Normal, accentColor);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;

            return btnObj;
        }
    }
}
