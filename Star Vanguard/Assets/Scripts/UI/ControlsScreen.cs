using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Builds the CONTROLS &amp; MANUAL panel from <see cref="InputMap"/>.
    ///
    /// The legacy scene panel was 1920x1440 (taller than the 1080 screen) and contained a
    /// 1680x820 raycast-blocking backdrop drawn on top of its own Back button, so once it
    /// opened nothing could be clicked. This version is a single 980x720 card with no
    /// invisible blocker between the pointer and the buttons.
    /// </summary>
    public static class ControlsScreen
    {
        public const float CardWidth = 980f;
        public const float CardHeight = 720f;
        public const string RootName = "ControlsPanel";

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

            TextMeshProUGUI title = UITheme.CreateText(
                cardRect, "Title", "CONTROLS & MANUAL", UITheme.H1, UITheme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            title.characterSpacing = 3f;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -32f);
            title.rectTransform.sizeDelta = new Vector2(-80f, 56f);

            // Column headers
            const float columnTop = -110f;
            AddHeader(cardRect, "ACTION_HEADER", "ACTION", 0f, columnTop);
            AddHeader(cardRect, "KEYBOARD_HEADER", "KEYBOARD", 300f, columnTop);
            AddHeader(cardRect, "MOUSE_HEADER", "MOUSE", 660f, columnTop);

            float rowY = columnTop - 46f;
            foreach (InputMap.Row row in InputMap.Rows)
            {
                AddRow(cardRect, row, rowY);
                rowY -= 52f;
            }

            Button back = UITheme.CreateButton(
                cardRect, "BackButton", "BACK", 340f, UITheme.ButtonHeightSmall, false,
                () => UIPanelRouter.Instance?.Back());
            RectTransform backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 32f);

            TextMeshProUGUI hint = UITheme.CreateText(
                cardRect, "Hint", "Hold FIRE for autofire  ·  ESC closes this panel",
                UITheme.Caption, UITheme.TextMuted, TextAlignmentOptions.Center);
            hint.rectTransform.anchorMin = new Vector2(0f, 0f);
            hint.rectTransform.anchorMax = new Vector2(1f, 0f);
            hint.rectTransform.pivot = new Vector2(0.5f, 0f);
            hint.rectTransform.anchoredPosition = new Vector2(0f, 108f);
            hint.rectTransform.sizeDelta = new Vector2(-80f, 24f);

            return root.gameObject;
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

        // Rows are placed from the top-left corner of the card, one 8 px grid step apart.
        private static void AddHeader(RectTransform i_Card, string i_Name, string i_Label, float i_X, float i_Y)
        {
            TextMeshProUGUI header = UITheme.CreateText(
                i_Card, i_Name, i_Label, UITheme.Caption, UITheme.CyanDim,
                TextAlignmentOptions.Left, FontStyles.Bold);
            header.characterSpacing = 3f;
            header.rectTransform.anchorMin = new Vector2(0f, 1f);
            header.rectTransform.anchorMax = new Vector2(0f, 1f);
            header.rectTransform.pivot = new Vector2(0f, 1f);
            header.rectTransform.anchoredPosition = new Vector2(48f + i_X, i_Y);
            header.rectTransform.sizeDelta = new Vector2(260f, 24f);
        }

        private static void AddRow(RectTransform i_Card, InputMap.Row i_Row, float i_Y)
        {
            AddCell(i_Card, $"{i_Row.Action}_Action", i_Row.Action, 48f, i_Y, UITheme.TextMuted, 300f);
            AddCell(i_Card, $"{i_Row.Action}_Keyboard", i_Row.Keyboard, 348f, i_Y, UITheme.TextPrimary, 340f);
            AddCell(i_Card, $"{i_Row.Action}_Mouse", i_Row.Mouse, 708f, i_Y, UITheme.TextMuted, 240f);
        }

        private static void AddCell(RectTransform i_Parent, string i_Name, string i_Text,
            float i_X, float i_Y, Color i_Color, float i_Width)
        {
            TextMeshProUGUI cell = UITheme.CreateText(
                i_Parent, i_Name, i_Text, UITheme.Body, i_Color, TextAlignmentOptions.Left);
            cell.rectTransform.anchorMin = new Vector2(0f, 1f);
            cell.rectTransform.anchorMax = new Vector2(0f, 1f);
            cell.rectTransform.pivot = new Vector2(0f, 1f);
            cell.rectTransform.anchoredPosition = new Vector2(i_X, i_Y);
            cell.rectTransform.sizeDelta = new Vector2(i_Width, 32f);
        }
    }
}
