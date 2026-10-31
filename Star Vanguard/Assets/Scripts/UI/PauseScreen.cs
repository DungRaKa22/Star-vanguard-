using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Modern sci-fi in-game pause screen adhering to Star Vanguard UITheme.
    /// Synchronizes design language with MainMenu, Controls, and Settings panels.
    /// Provides Resume, Restart, Settings, Save Game, and Return to Menu actions.
    /// </summary>
    public static class PauseScreen
    {
        public const float CardWidth = 560f;
        public const float CardHeight = 640f;
        public const string RootName = "PausePanel";

        public static GameObject Build(Transform i_Canvas)
        {
            RectTransform root = UITheme.CreateRect(i_Canvas, RootName);
            UITheme.StretchToParent(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0.02f, 0.06f, 0.85f);

            Image card = UITheme.CreatePanelCard(root, "Card", new Vector2(CardWidth, CardHeight));
            RectTransform cardRect = card.rectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            // Title
            TextMeshProUGUI title = UITheme.CreateText(
                cardRect, "Title", "TẠM DỪNG / PAUSED", UITheme.H1, UITheme.Cyan,
                TextAlignmentOptions.Center, FontStyles.Bold);
            title.characterSpacing = 3f;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -28f);
            title.rectTransform.sizeDelta = new Vector2(-60f, 48f);

            // Subtitle
            TextMeshProUGUI subtitle = UITheme.CreateText(
                cardRect, "Subtitle", "- HỆ THỐNG PHÒNG THỦ KHÔNG GIAN -", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Center, FontStyles.Normal);
            subtitle.characterSpacing = 2f;
            subtitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, -74f);
            subtitle.rectTransform.sizeDelta = new Vector2(-60f, 24f);

            // Divider
            Image divider = UITheme.CreateRect(cardRect, "Divider").gameObject.AddComponent<Image>();
            divider.color = new Color(UITheme.Cyan.r, UITheme.Cyan.g, UITheme.Cyan.b, 0.6f);
            divider.raycastTarget = false;
            divider.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            divider.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            divider.rectTransform.pivot = new Vector2(0.5f, 1f);
            divider.rectTransform.anchoredPosition = new Vector2(0f, -106f);
            divider.rectTransform.sizeDelta = new Vector2(380f, 2f);

            // Status message toast/feedback
            TextMeshProUGUI statusText = UITheme.CreateText(
                cardRect, "Txt_Status", "", UITheme.Caption, UITheme.Accent,
                TextAlignmentOptions.Center, FontStyles.Normal);
            statusText.rectTransform.anchorMin = new Vector2(0f, 1f);
            statusText.rectTransform.anchorMax = new Vector2(1f, 1f);
            statusText.rectTransform.pivot = new Vector2(0.5f, 1f);
            statusText.rectTransform.anchoredPosition = new Vector2(0f, -116f);
            statusText.rectTransform.sizeDelta = new Vector2(-60f, 24f);

            // Button list container
            RectTransform col = UITheme.CreateRect(cardRect, "ButtonColumn");
            col.anchorMin = new Vector2(0.5f, 0f);
            col.anchorMax = new Vector2(0.5f, 0f);
            col.pivot = new Vector2(0.5f, 0f);
            col.anchoredPosition = new Vector2(0f, 28f);
            col.sizeDelta = new Vector2(UITheme.ButtonWidth, 470f);

            VerticalLayoutGroup layout = col.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // 1. Resume Button (Primary Cyan highlight)
            UITheme.CreateButton(col, "Btn_Resume", "TIẾP TỤC (RESUME)", UITheme.ButtonWidth, UITheme.ButtonHeightSmall,
                true, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    GameManager.Instance?.ResumeGame();
                });

            // 2. Restart Button
            UITheme.CreateButton(col, "Btn_Restart", "CHƠI LẠI (RESTART)", UITheme.ButtonWidth, UITheme.ButtonHeightSmall,
                false, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    GameManager.Instance?.RestartGame();
                });

            // 3. Settings Button
            UITheme.CreateButton(col, "Btn_Settings", "CÀI ĐẶT HỆ THỐNG", UITheme.ButtonWidth, UITheme.ButtonHeightSmall,
                false, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    UIPanelRouter.Instance?.Push(UIPanelId.Settings);
                });

            // 4. Save Game Button
            UITheme.CreateButton(col, "Btn_Save", "LƯU TIẾN TRÌNH (SAVE)", UITheme.ButtonWidth, UITheme.ButtonHeightSmall,
                false, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    SaveGameProgress(statusText);
                });

            // 5. Quit to Main Menu Button
            UITheme.CreateButton(col, "Btn_Quit", "VỀ MENU CHÍNH (QUIT)", UITheme.ButtonWidth, UITheme.ButtonHeightSmall,
                false, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    GameManager.Instance?.ReturnToMenu();
                });

            // Compatibility: attach PauseController so legacy code doesn't fail
            if (root.GetComponent<PauseController>() == null)
            {
                root.gameObject.AddComponent<PauseController>();
            }

            return root.gameObject;
        }

        private static void SaveGameProgress(TextMeshProUGUI i_Status)
        {
            try
            {
                SettingsManager.Instance?.SaveSettings();
                PlayerPrefs.Save();
                if (i_Status != null)
                {
                    i_Status.text = "✓ ĐÃ LƯU TIẾN TRÌNH CHIẾN DỊCH THÀNH CÔNG!";
                    i_Status.color = UITheme.Cyan;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PauseScreen] Error saving progress: {ex.Message}");
                if (i_Status != null)
                {
                    i_Status.text = "LƯU TIẾN TRÌNH THẤT BẠI!";
                    i_Status.color = UITheme.Danger;
                }
            }
        }
    }
}
