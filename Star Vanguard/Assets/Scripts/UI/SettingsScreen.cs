using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Builds the modern SETTINGS panel using <see cref="UITheme"/> and binds to <see cref="SettingsUI"/>.
    /// Replaces the legacy broken container that was missing volume sliders and toggles.
    /// </summary>
    public static class SettingsScreen
    {
        public const float CardWidth = 980f;
        public const float CardHeight = 720f;
        public const string RootName = "SettingsPanel";

        public static GameObject Build(Transform i_Canvas)
        {
            RectTransform root = UITheme.CreateRect(i_Canvas, RootName);
            UITheme.StretchToParent(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0.02f, 0.06f, 0.82f);

            Image card = UITheme.CreatePanelCard(root, "Card", new Vector2(CardWidth, CardHeight));
            RectTransform cardRect = card.rectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            // Title
            TextMeshProUGUI title = UITheme.CreateText(
                cardRect, "Title", "CÀI ĐẶT HỆ THỐNG / SETTINGS", UITheme.H1, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            title.characterSpacing = 3f;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -28f);
            title.rectTransform.sizeDelta = new Vector2(-80f, 50f);

            // Left column: Audio
            float colY = -90f;
            TextMeshProUGUI audioHeader = UITheme.CreateText(
                cardRect, "AudioHeader", "ÂM THANH (AUDIO)", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Left, FontStyles.Bold);
            audioHeader.characterSpacing = 2f;
            audioHeader.rectTransform.anchorMin = new Vector2(0f, 1f);
            audioHeader.rectTransform.anchorMax = new Vector2(0f, 1f);
            audioHeader.rectTransform.pivot = new Vector2(0f, 1f);
            audioHeader.rectTransform.anchoredPosition = new Vector2(60f, colY);
            audioHeader.rectTransform.sizeDelta = new Vector2(400f, 24f);

            // Master Volume
            float itemY = colY - 36f;
            (Slider masterSlider, TextMeshProUGUI masterPct) = AddVolumeRow(cardRect, "MasterVolume", "Âm Lượng Tổng (Master)", 60f, itemY, 1f);

            // Music Volume
            itemY -= 76f;
            (Slider musicSlider, TextMeshProUGUI musicPct) = AddVolumeRow(cardRect, "MusicVolume", "Nhạc Nền (Music)", 60f, itemY, 0.8f);

            // SFX Volume
            itemY -= 76f;
            (Slider sfxSlider, TextMeshProUGUI sfxPct) = AddVolumeRow(cardRect, "SFXVolume", "Hiệu Ứng (SFX)", 60f, itemY, 1f);

            // Right column: Gameplay / Controls
            TextMeshProUGUI controlsHeader = UITheme.CreateText(
                cardRect, "ControlsHeader", "ĐIỀU KHIỂN (CONTROLS)", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Left, FontStyles.Bold);
            controlsHeader.characterSpacing = 2f;
            controlsHeader.rectTransform.anchorMin = new Vector2(0f, 1f);
            controlsHeader.rectTransform.anchorMax = new Vector2(0f, 1f);
            controlsHeader.rectTransform.pivot = new Vector2(0f, 1f);
            controlsHeader.rectTransform.anchoredPosition = new Vector2(520f, colY);
            controlsHeader.rectTransform.sizeDelta = new Vector2(400f, 24f);

            float toggleY = colY - 44f;
            Toggle mouseToggle = AddToggleRow(cardRect, "MouseControlsToggle", "Điều Khiển Chuột (Mouse)", 520f, toggleY, false);

            toggleY -= 68f;
            Toggle mouseSmoothToggle = AddToggleRow(cardRect, "MouseSmoothingToggle", "Mượt Chuột (Smoothing)", 520f, toggleY, true);

            toggleY -= 68f;
            Toggle kbSmoothToggle = AddToggleRow(cardRect, "KeyboardSmoothingToggle", "Mượt Phím (Smoothing)", 520f, toggleY, true);

            // Bottom action buttons
            Button resetBtn = UITheme.CreateButton(
                cardRect, "ResetToDefaultsButton", "ĐẶT LẠI (RESET)", 260f, UITheme.ButtonHeightSmall, false, null);
            RectTransform resetRect = resetBtn.GetComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.5f, 0f);
            resetRect.anchorMax = new Vector2(0.5f, 0f);
            resetRect.pivot = new Vector2(0.5f, 0f);
            resetRect.anchoredPosition = new Vector2(-150f, 32f);

            Button backBtn = UITheme.CreateButton(
                cardRect, "BackButton", "QUAY LẠI (BACK)", 260f, UITheme.ButtonHeightSmall, true,
                () => UIPanelRouter.Instance?.Back());
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(150f, 32f);

            // Component SettingsUI
            SettingsUI settingsUI = root.gameObject.GetComponent<SettingsUI>();
            if (settingsUI == null) settingsUI = root.gameObject.AddComponent<SettingsUI>();

            settingsUI.Configure(
                mouseToggle, mouseSmoothToggle, kbSmoothToggle,
                masterSlider, masterPct,
                sfxSlider, sfxPct,
                musicSlider, musicPct,
                resetBtn, backBtn);

            return root.gameObject;
        }

        private static (Slider, TextMeshProUGUI) AddVolumeRow(
            RectTransform i_Card, string i_Name, string i_Label, float i_X, float i_Y, float i_Initial)
        {
            TextMeshProUGUI label = UITheme.CreateText(
                i_Card, $"{i_Name}_Label", i_Label, UITheme.Body, UITheme.TextPrimary, TextAlignmentOptions.Left);
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(0f, 1f);
            label.rectTransform.pivot = new Vector2(0f, 1f);
            label.rectTransform.anchoredPosition = new Vector2(i_X, i_Y);
            label.rectTransform.sizeDelta = new Vector2(280f, 26f);

            TextMeshProUGUI pct = UITheme.CreateText(
                i_Card, $"{i_Name}_Pct", $"{Mathf.RoundToInt(i_Initial * 100f)}%", UITheme.Body, UITheme.Cyan, TextAlignmentOptions.Right);
            pct.rectTransform.anchorMin = new Vector2(0f, 1f);
            pct.rectTransform.anchorMax = new Vector2(0f, 1f);
            pct.rectTransform.pivot = new Vector2(0f, 1f);
            pct.rectTransform.anchoredPosition = new Vector2(i_X + 280f, i_Y);
            pct.rectTransform.sizeDelta = new Vector2(100f, 26f);

            Slider slider = UITheme.CreateSlider(i_Card, $"{i_Name}_Slider", 380f, 24f, 0f, 1f, i_Initial);
            RectTransform sRect = slider.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 1f);
            sRect.anchorMax = new Vector2(0f, 1f);
            sRect.pivot = new Vector2(0f, 1f);
            sRect.anchoredPosition = new Vector2(i_X, i_Y - 30f);

            return (slider, pct);
        }

        private static Toggle AddToggleRow(
            RectTransform i_Card, string i_Name, string i_Label, float i_X, float i_Y, bool i_Initial)
        {
            Toggle toggle = UITheme.CreateToggle(i_Card, i_Name, i_Label, 380f, 44f, i_Initial);
            RectTransform tRect = toggle.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(0f, 1f);
            tRect.pivot = new Vector2(0f, 1f);
            tRect.anchoredPosition = new Vector2(i_X, i_Y);
            return toggle;
        }

        public static void Bind(GameObject i_Root)
        {
            if (i_Root == null) return;
            Button[] buttons = i_Root.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.name == "BackButton")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => UIPanelRouter.Instance?.Back());
                    break;
                }
            }
        }
    }
}
