using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Modern sci-fi in-game Game Over debrief terminal adhering to Star Vanguard UITheme.
    /// Provides Final Score display, High Score record badge, Player Name input,
    /// Top Pilots Leaderboard, and Restart, Settings, and Return to Menu actions.
    /// </summary>
    public static class GameOverScreen
    {
        public const float CardWidth = 860f;
        public const float CardHeight = 780f;
        public const string RootName = "GameOverPanel";

        public static GameObject Build(Transform i_Canvas)
        {
            RectTransform root = UITheme.CreateRect(i_Canvas, RootName);
            UITheme.StretchToParent(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.02f, 0.05f, 0.92f);

            Image card = UITheme.CreatePanelCard(root, "Card", new Vector2(CardWidth, CardHeight));
            RectTransform cardRect = card.rectTransform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            // 1. Title
            TextMeshProUGUI title = UITheme.CreateText(
                cardRect, "Title", "CHIẾN DỊCH KẾT THÚC", UITheme.H1, UITheme.Danger,
                TextAlignmentOptions.Center, FontStyles.Bold);
            title.characterSpacing = 3f;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            title.rectTransform.sizeDelta = new Vector2(-60f, 46f);

            // Subtitle
            TextMeshProUGUI subtitle = UITheme.CreateText(
                cardRect, "Subtitle", "- CHIẾN CƠ ĐÃ BỊ TIÊU DIỆT TRONG KHÔNG GIAN -", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Center, FontStyles.Normal);
            subtitle.characterSpacing = 2f;
            subtitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, -68f);
            subtitle.rectTransform.sizeDelta = new Vector2(-60f, 24f);

            // Divider
            Image divider = UITheme.CreateRect(cardRect, "Divider").gameObject.AddComponent<Image>();
            divider.color = new Color(UITheme.Danger.r, UITheme.Danger.g, UITheme.Danger.b, 0.65f);
            divider.raycastTarget = false;
            divider.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            divider.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            divider.rectTransform.pivot = new Vector2(0.5f, 1f);
            divider.rectTransform.anchoredPosition = new Vector2(0f, -98f);
            divider.rectTransform.sizeDelta = new Vector2(620f, 2f);

            // 2. Final Score Summary Box
            Image scoreBox = UITheme.CreatePanelCard(cardRect, "ScoreBox", new Vector2(740f, 120f));
            RectTransform sbRect = scoreBox.rectTransform;
            sbRect.anchorMin = new Vector2(0.5f, 1f);
            sbRect.anchorMax = new Vector2(0.5f, 1f);
            sbRect.pivot = new Vector2(0.5f, 1f);
            sbRect.anchoredPosition = new Vector2(0f, -112f);

            TextMeshProUGUI scoreLabel = UITheme.CreateText(
                sbRect, "ScoreLabel", "ĐIỂM SỐ CHIẾN DỊCH (FINAL SCORE)", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Center, FontStyles.Bold);
            scoreLabel.characterSpacing = 2f;
            scoreLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            scoreLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            scoreLabel.rectTransform.pivot = new Vector2(0.5f, 1f);
            scoreLabel.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            scoreLabel.rectTransform.sizeDelta = new Vector2(-40f, 22f);

            TextMeshProUGUI finalScoreText = UITheme.CreateText(
                sbRect, "FinalScoreText", "0", 52, UITheme.Cyan,
                TextAlignmentOptions.Center, FontStyles.Bold);
            finalScoreText.characterSpacing = 2f;
            finalScoreText.rectTransform.anchorMin = new Vector2(0f, 1f);
            finalScoreText.rectTransform.anchorMax = new Vector2(1f, 1f);
            finalScoreText.rectTransform.pivot = new Vector2(0.5f, 1f);
            finalScoreText.rectTransform.anchoredPosition = new Vector2(0f, -38f);
            finalScoreText.rectTransform.sizeDelta = new Vector2(-40f, 54f);

            int highest = LeaderboardManager.Instance != null ? LeaderboardManager.Instance.GetHighestScore() : 0;
            TextMeshProUGUI bestScoreText = UITheme.CreateText(
                sbRect, "BestScoreText", $"KỶ LỤC HIỆN TẠI: {highest:N0}", UITheme.Caption, UITheme.Accent,
                TextAlignmentOptions.Center, FontStyles.Normal);
            bestScoreText.rectTransform.anchorMin = new Vector2(0f, 0f);
            bestScoreText.rectTransform.anchorMax = new Vector2(1f, 0f);
            bestScoreText.rectTransform.pivot = new Vector2(0.5f, 0f);
            bestScoreText.rectTransform.anchoredPosition = new Vector2(0f, 8f);
            bestScoreText.rectTransform.sizeDelta = new Vector2(-40f, 22f);

            // 3. Middle Content Container (Height: 380f)
            RectTransform contentArea = UITheme.CreateRect(cardRect, "ContentArea");
            contentArea.anchorMin = new Vector2(0.5f, 1f);
            contentArea.anchorMax = new Vector2(0.5f, 1f);
            contentArea.pivot = new Vector2(0.5f, 1f);
            contentArea.anchoredPosition = new Vector2(0f, -242f);
            contentArea.sizeDelta = new Vector2(740f, 380f);

            // --- 3A. New High Score Subpanel ---
            RectTransform newHighPanel = UITheme.CreateRect(contentArea, "NewHighScorePanel");
            UITheme.StretchToParent(newHighPanel);

            TextMeshProUGUI newHighMsg = UITheme.CreateText(
                newHighPanel, "NewHighScoreMessage", "★ THIẾT LẬP KỶ LỤC MỚI! (NEW HIGH SCORE) ★", UITheme.H2, UITheme.Accent,
                TextAlignmentOptions.Center, FontStyles.Bold);
            newHighMsg.characterSpacing = 2f;
            newHighMsg.rectTransform.anchorMin = new Vector2(0f, 1f);
            newHighMsg.rectTransform.anchorMax = new Vector2(1f, 1f);
            newHighMsg.rectTransform.pivot = new Vector2(0.5f, 1f);
            newHighMsg.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            newHighMsg.rectTransform.sizeDelta = new Vector2(-40f, 40f);

            TextMeshProUGUI promptText = UITheme.CreateText(
                newHighPanel, "PromptText", "Chúc mừng phi công! Nhập tên của bạn để lưu danh vào bảng vàng:", UITheme.Body, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Normal);
            promptText.rectTransform.anchorMin = new Vector2(0f, 1f);
            promptText.rectTransform.anchorMax = new Vector2(1f, 1f);
            promptText.rectTransform.pivot = new Vector2(0.5f, 1f);
            promptText.rectTransform.anchoredPosition = new Vector2(0f, -70f);
            promptText.rectTransform.sizeDelta = new Vector2(-40f, 30f);

            // Pilot Name Input Container
            RectTransform inputContainer = UITheme.CreateRect(newHighPanel, "PlayerNameInputContainer");
            inputContainer.anchorMin = new Vector2(0.5f, 1f);
            inputContainer.anchorMax = new Vector2(0.5f, 1f);
            inputContainer.pivot = new Vector2(0.5f, 1f);
            inputContainer.anchoredPosition = new Vector2(0f, -120f);
            inputContainer.sizeDelta = new Vector2(460f, 50f);

            Image inputBg = inputContainer.gameObject.AddComponent<Image>();
            inputBg.sprite = UITheme.Rounded;
            inputBg.type = Image.Type.Sliced;
            inputBg.color = new Color(0.04f, 0.10f, 0.20f, 0.95f);

            TMP_InputField inputField = inputContainer.gameObject.AddComponent<TMP_InputField>();
            inputField.name = "PlayerNameInput";

            TextMeshProUGUI inputText = UITheme.CreateText(
                inputContainer, "Text", "", UITheme.Body, UITheme.Cyan,
                TextAlignmentOptions.Center, FontStyles.Bold);
            UITheme.StretchToParent(inputText.rectTransform);
            inputField.textComponent = inputText;

            TextMeshProUGUI placeholder = UITheme.CreateText(
                inputContainer, "Placeholder", "NHẬP TÊN PHI CÔNG...", UITheme.Body, UITheme.TextMuted,
                TextAlignmentOptions.Center, FontStyles.Normal);
            UITheme.StretchToParent(placeholder.rectTransform);
            inputField.placeholder = placeholder;

            string savedName = LeaderboardManager.Instance != null ? LeaderboardManager.Instance.GetPlayerName() : "";
            if (!string.IsNullOrEmpty(savedName))
            {
                inputField.text = savedName;
            }

            Button submitBtn = UITheme.CreateButton(
                newHighPanel, "SubmitButton", "LƯU KỶ LỤC (SUBMIT)", 340f, UITheme.ButtonHeightSmall,
                true, null);
            RectTransform submitRect = submitBtn.GetComponent<RectTransform>();
            submitRect.anchorMin = new Vector2(0.5f, 1f);
            submitRect.anchorMax = new Vector2(0.5f, 1f);
            submitRect.pivot = new Vector2(0.5f, 1f);
            submitRect.anchoredPosition = new Vector2(0f, -200f);

            newHighPanel.gameObject.SetActive(false);

            // --- 3B. Leaderboard Top Pilots Subpanel ---
            RectTransform lbContent = UITheme.CreateRect(contentArea, "LeaderboardContent");
            UITheme.StretchToParent(lbContent);

            TextMeshProUGUI lbHeader = UITheme.CreateText(
                lbContent, "LeaderboardHeader", "BẢNG VINH DANH PHI CÔNG XUẤT SẮC (TOP 5)", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Center, FontStyles.Bold);
            lbHeader.characterSpacing = 2f;
            lbHeader.rectTransform.anchorMin = new Vector2(0f, 1f);
            lbHeader.rectTransform.anchorMax = new Vector2(1f, 1f);
            lbHeader.rectTransform.pivot = new Vector2(0.5f, 1f);
            lbHeader.rectTransform.anchoredPosition = new Vector2(0f, -10f);
            lbHeader.rectTransform.sizeDelta = new Vector2(-40f, 24f);

            RectTransform lbContainer = UITheme.CreateRect(lbContent, "LeaderboardContainer");
            lbContainer.anchorMin = new Vector2(0.5f, 1f);
            lbContainer.anchorMax = new Vector2(0.5f, 1f);
            lbContainer.pivot = new Vector2(0.5f, 1f);
            lbContainer.anchoredPosition = new Vector2(0f, -44f);
            lbContainer.sizeDelta = new Vector2(700f, 300f);

            VerticalLayoutGroup lbLayout = lbContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            lbLayout.spacing = 8f;
            lbLayout.childAlignment = TextAnchor.UpperCenter;
            lbLayout.childControlWidth = true;
            lbLayout.childControlHeight = false;
            lbLayout.childForceExpandWidth = true;
            lbLayout.childForceExpandHeight = false;

            TextMeshProUGUI noScoresText = UITheme.CreateText(
                lbContent, "NoScoresText", "Chưa có dữ liệu kỷ lục nào được ghi nhận.", UITheme.Body, UITheme.TextMuted,
                TextAlignmentOptions.Center, FontStyles.Normal);
            noScoresText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            noScoresText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            noScoresText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            noScoresText.rectTransform.anchoredPosition = new Vector2(0f, -60f);
            noScoresText.rectTransform.sizeDelta = new Vector2(500f, 40f);
            noScoresText.gameObject.SetActive(false);

            // 4. Action Button Row at Bottom
            RectTransform actionRow = UITheme.CreateRect(cardRect, "ActionRow");
            actionRow.anchorMin = new Vector2(0.5f, 0f);
            actionRow.anchorMax = new Vector2(0.5f, 0f);
            actionRow.pivot = new Vector2(0.5f, 0f);
            actionRow.anchoredPosition = new Vector2(0f, 32f);
            actionRow.sizeDelta = new Vector2(740f, UITheme.ButtonHeightSmall);

            HorizontalLayoutGroup rowLayout = actionRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 20f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            Button restartBtn = UITheme.CreateButton(
                actionRow, "RestartButton", "CHƠI LẠI (RESTART)", 230f, UITheme.ButtonHeightSmall,
                true, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    GameManager.Instance?.RestartGame();
                });

            Button settingsBtn = UITheme.CreateButton(
                actionRow, "SettingsButton", "CÀI ĐẶT (SETTINGS)", 220f, UITheme.ButtonHeightSmall,
                false, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    UIPanelRouter.Instance?.Push(UIPanelId.Settings);
                });

            Button quitBtn = UITheme.CreateButton(
                actionRow, "QuitButton", "VỀ MENU (QUIT)", 220f, UITheme.ButtonHeightSmall,
                false, () =>
                {
                    AudioManager.Instance?.Play(eSFXId.UIClick);
                    GameManager.Instance?.ReturnToMenu();
                });

            // 5. Attach & Configure Controller Components
            GameOverController ctrl = root.GetComponent<GameOverController>();
            if (ctrl == null) ctrl = root.gameObject.AddComponent<GameOverController>();

            LeaderboardUI lbUI = root.GetComponent<LeaderboardUI>();
            if (lbUI == null) lbUI = root.gameObject.AddComponent<LeaderboardUI>();

            GameObject entryPrefab = null;
#if UNITY_EDITOR
            entryPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI_Prefabs/LeaderboardEntry_Prefab.prefab");
#endif
            lbUI.Configure(
                lbContent.gameObject,
                lbContainer,
                entryPrefab,
                noScoresText,
                newHighPanel.gameObject,
                inputField,
                submitBtn,
                finalScoreText);

            ctrl.Configure(finalScoreText, lbUI, restartBtn, quitBtn, 0.5f);

            return root.gameObject;
        }
    }
}
