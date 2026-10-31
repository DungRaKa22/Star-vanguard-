using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Builds the LAN Co-op connection panel adhering to Star Vanguard UITheme (ADR-002).
    /// Pure uGUI presentation layer, decoupled from networking transport.
    /// </summary>
    public static class CoopScreen
    {
        public const float CardWidth = 760f;
        public const float CardHeight = 560f;
        public const string RootName = "CoopModalPanel";

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
                cardRect, "Title", "CHIẾN DỊCH CO-OP (LAN)", UITheme.H1, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            title.characterSpacing = 3f;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -32f);
            title.rectTransform.sizeDelta = new Vector2(-60f, 50f);

            // Subtitle
            TextMeshProUGUI sub = UITheme.CreateText(
                cardRect, "Subtitle", "KẾT NỐI MẠNG NỘI BỘ 2 NGƯỜI CHƠI (UDP PORT 7777)", UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Center, FontStyles.Normal);
            sub.rectTransform.anchorMin = new Vector2(0f, 1f);
            sub.rectTransform.anchorMax = new Vector2(1f, 1f);
            sub.rectTransform.pivot = new Vector2(0.5f, 1f);
            sub.rectTransform.anchoredPosition = new Vector2(0f, -80f);
            sub.rectTransform.sizeDelta = new Vector2(-60f, 30f);

            // Status message box
            Image statusBox = UITheme.CreateRect(cardRect, "StatusBox").gameObject.AddComponent<Image>();
            statusBox.color = new Color(0.02f, 0.05f, 0.12f, 0.8f);
            statusBox.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            statusBox.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            statusBox.rectTransform.pivot = new Vector2(0.5f, 1f);
            statusBox.rectTransform.anchoredPosition = new Vector2(0f, -125f);
            statusBox.rectTransform.sizeDelta = new Vector2(620f, 48f);

            TextMeshProUGUI statusTxt = UITheme.CreateText(
                statusBox.rectTransform, "Txt_Status", "Sẵn sàng tạo phòng Host hoặc kết nối...", UITheme.Body, UITheme.Accent,
                TextAlignmentOptions.Center, FontStyles.Normal);
            UITheme.StretchToParent(statusTxt.rectTransform);

            // Host Button
            Button hostBtn = UITheme.CreateButton(
                cardRect, "Btn_Host", "TẠO PHÒNG (HOST)", 420f, UITheme.ButtonHeight, true, null);
            RectTransform hostRect = hostBtn.GetComponent<RectTransform>();
            hostRect.anchorMin = new Vector2(0.5f, 0.5f);
            hostRect.anchorMax = new Vector2(0.5f, 0.5f);
            hostRect.pivot = new Vector2(0.5f, 0.5f);
            hostRect.anchoredPosition = new Vector2(0f, 30f);

            // IP Address Input Field Container
            RectTransform ipContainer = UITheme.CreateRect(cardRect, "IpContainer");
            ipContainer.anchorMin = new Vector2(0.5f, 0.5f);
            ipContainer.anchorMax = new Vector2(0.5f, 0.5f);
            ipContainer.pivot = new Vector2(0.5f, 0.5f);
            ipContainer.anchoredPosition = new Vector2(0f, -50f);
            ipContainer.sizeDelta = new Vector2(420f, 44f);

            Image ipBg = ipContainer.gameObject.AddComponent<Image>();
            ipBg.color = new Color(0.04f, 0.08f, 0.15f, 0.9f);

            TMP_InputField inputField = ipContainer.gameObject.AddComponent<TMP_InputField>();
            inputField.name = "Input_IPAddress";

            TextMeshProUGUI inputText = UITheme.CreateText(
                ipContainer, "Text", "127.0.0.1", UITheme.Body, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Normal);
            UITheme.StretchToParent(inputText.rectTransform);
            inputField.textComponent = inputText;
            inputField.text = "127.0.0.1";

            // Join Button
            Button joinBtn = UITheme.CreateButton(
                cardRect, "Btn_Join", "THAM GIA (JOIN IP)", 420f, UITheme.ButtonHeightSmall, false, null);
            RectTransform joinRect = joinBtn.GetComponent<RectTransform>();
            joinRect.anchorMin = new Vector2(0.5f, 0.5f);
            joinRect.anchorMax = new Vector2(0.5f, 0.5f);
            joinRect.pivot = new Vector2(0.5f, 0.5f);
            joinRect.anchoredPosition = new Vector2(0f, -120f);

            // Back Button
            Button backBtn = UITheme.CreateButton(
                cardRect, "Btn_Back", "QUAY LẠI (BACK)", 260f, UITheme.ButtonHeightSmall, false,
                () => UIPanelRouter.Instance?.Back());
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 30f);

            return root.gameObject;
        }
    }
}
