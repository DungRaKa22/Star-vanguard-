using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Single source of truth for every UI measurement in Star Vanguard.
    ///
    /// Before this existed the project mixed font sizes 24/30/36/50/60/75 and buttons
    /// of arbitrary sizes, so no two panels lined up. Everything now derives from one
    /// 1920x1080 reference and an 8 px grid.
    /// </summary>
    public static class UITheme
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        // ---- typography ramp (px at 1920x1080) ----
        public const int Display = 72;   // game title, GAME OVER
        public const int H1 = 48;        // panel titles
        public const int H2 = 32;        // section titles, wave name
        public const int Body = 24;      // buttons, content
        public const int Caption = 18;   // hints, footer
        public const int Hud = 36;       // score, lives, bombs

        // ---- metrics (8 px grid) ----
        public const float ButtonWidth = 440f;
        public const float ButtonHeight = 76f;
        public const float ButtonHeightSmall = 56f;
        public const float Gap = 16f;
        public const float PanelPadding = 40f;
        public const float SafeMargin = 64f;
        public const float FormPanelWidth = 720f;
        public const float WidePanelWidth = 1000f;

        // ---- palette ----
        public static readonly Color Cyan = new Color(0.0f, 0.95f, 1.0f);
        public static readonly Color CyanDim = new Color(0.35f, 0.75f, 0.9f);
        public static readonly Color TextPrimary = new Color(0.90f, 0.97f, 1.0f);
        public static readonly Color TextMuted = new Color(0.55f, 0.72f, 0.85f);
        public static readonly Color PanelFill = new Color(0.02f, 0.07f, 0.14f, 0.92f);
        public static readonly Color ButtonFill = new Color(0.04f, 0.13f, 0.26f, 0.92f);
        public static readonly Color ButtonFillPrimary = new Color(0.0f, 0.30f, 0.48f, 0.96f);
        public static readonly Color Danger = new Color(1.0f, 0.28f, 0.32f);
        public static readonly Color Accent = new Color(1.0f, 0.82f, 0.2f);
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

        private static Sprite s_Rounded;
        private static Sprite s_RoundedOutline;

        /// <summary>9-sliced rounded rectangle, ready for Image.Type.Sliced.</summary>
        public static Sprite Rounded
        {
            get
            {
                if (s_Rounded == null)
                {
#if UNITY_EDITOR
                    s_Rounded = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_Rounded.png");
#endif
                    if (s_Rounded == null)
                    {
                        s_Rounded = CreateRoundedSprite(false);
                    }
                }
                return s_Rounded;
            }
        }

        /// <summary>Neon outline that matches <see cref="Rounded"/> exactly.</summary>
        public static Sprite RoundedOutline
        {
            get
            {
                if (s_RoundedOutline == null)
                {
#if UNITY_EDITOR
                    s_RoundedOutline = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/UI_RoundedOutline.png");
#endif
                    if (s_RoundedOutline == null)
                    {
                        s_RoundedOutline = CreateRoundedSprite(true);
                    }
                }
                return s_RoundedOutline;
            }
        }

        private const int k_TextureSize = 64;
        private const int k_CornerRadius = 14;

        private static Sprite CreateRoundedSprite(bool i_Outline)
        {
            Texture2D texture = new Texture2D(k_TextureSize, k_TextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = i_Outline ? "UI_RoundedOutline" : "UI_Rounded"
            };

            Color32[] pixels = new Color32[k_TextureSize * k_TextureSize];
            float radius = k_CornerRadius;

            for (int y = 0; y < k_TextureSize; y++)
            {
                for (int x = 0; x < k_TextureSize; x++)
                {
                    // Distance to the corner centre => anti-aliased rounded corner.
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, k_TextureSize - radius);
                    float cy = Mathf.Clamp(py, radius, k_TextureSize - radius);
                    float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));

                    float inside = Mathf.Clamp01(radius - distance + 0.5f);
                    float alpha = i_Outline
                        ? Mathf.Clamp01(inside - Mathf.Clamp01(radius - distance - 2f))
                        : inside;

                    pixels[(y * k_TextureSize) + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha) * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, k_TextureSize, k_TextureSize),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
        }

        // ------------------------------------------------------------------ factories

        public static RectTransform CreateRect(Transform i_Parent, string i_Name)
        {
            GameObject go = new GameObject(i_Name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(i_Parent, false);
            return rect;
        }

        public static TextMeshProUGUI CreateText(
            Transform i_Parent,
            string i_Name,
            string i_Content,
            int i_Size,
            Color i_Color,
            TextAlignmentOptions i_Alignment = TextAlignmentOptions.Left,
            FontStyles i_Style = FontStyles.Normal)
        {
            RectTransform rect = CreateRect(i_Parent, i_Name);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (text.font == null && TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }

            text.text = i_Content;
            text.fontSize = i_Size;
            text.color = i_Color;
            text.alignment = i_Alignment;
            text.fontStyle = i_Style;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        public static Button CreateButton(
            Transform i_Parent,
            string i_Name,
            string i_Label,
            float i_Width,
            float i_Height,
            bool i_Primary,
            UnityAction i_OnClick)
        {
            RectTransform rect = CreateRect(i_Parent, i_Name);
            rect.sizeDelta = new Vector2(i_Width, i_Height);

            Image background = rect.gameObject.AddComponent<Image>();
            background.sprite = Rounded;
            background.type = Image.Type.Sliced;
            background.color = i_Primary ? ButtonFillPrimary : ButtonFill;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.45f, 1.45f, 1.45f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            if (i_Primary)
            {
                Image outline = CreateRect(rect, "Outline").gameObject.AddComponent<Image>();
                outline.sprite = RoundedOutline;
                outline.type = Image.Type.Sliced;
                outline.color = Cyan;
                outline.raycastTarget = false;
                StretchToParent(outline.rectTransform);
            }

            TextMeshProUGUI label = CreateText(
                rect, "Label", i_Label, Body, i_Primary ? TextPrimary : CyanDim,
                TextAlignmentOptions.Center, FontStyles.Bold);
            label.characterSpacing = 2f;
            StretchToParent(label.rectTransform, 24f);

            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = i_Width;
            layout.preferredHeight = i_Height;

            if (i_OnClick != null)
            {
                button.onClick.AddListener(i_OnClick);
            }

            return button;
        }

        public static Image CreatePanelCard(Transform i_Parent, string i_Name, Vector2 i_Size)
        {
            RectTransform rect = CreateRect(i_Parent, i_Name);
            rect.sizeDelta = i_Size;

            Image fill = rect.gameObject.AddComponent<Image>();
            fill.sprite = Rounded;
            fill.type = Image.Type.Sliced;
            fill.color = PanelFill;

            Image outline = CreateRect(rect, "Outline").gameObject.AddComponent<Image>();
            outline.sprite = RoundedOutline;
            outline.type = Image.Type.Sliced;
            outline.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.55f);
            outline.raycastTarget = false;
            StretchToParent(outline.rectTransform);

            return fill;
        }

        public static Slider CreateSlider(
            Transform i_Parent,
            string i_Name,
            float i_Width,
            float i_Height,
            float i_Min,
            float i_Max,
            float i_Value)
        {
            RectTransform rect = CreateRect(i_Parent, i_Name);
            rect.sizeDelta = new Vector2(i_Width, i_Height);

            // Track background
            RectTransform bgRect = CreateRect(rect, "Background");
            StretchToParent(bgRect);
            Image bgImage = bgRect.gameObject.AddComponent<Image>();
            bgImage.sprite = Rounded;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.04f, 0.12f, 0.22f, 0.9f);

            // Fill area
            RectTransform fillArea = CreateRect(rect, "Fill Area");
            fillArea.anchorMin = new Vector2(0f, 0.2f);
            fillArea.anchorMax = new Vector2(1f, 0.8f);
            fillArea.offsetMin = new Vector2(6f, 0f);
            fillArea.offsetMax = new Vector2(-6f, 0f);

            RectTransform fillRect = CreateRect(fillArea, "Fill");
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fillRect.gameObject.AddComponent<Image>();
            fillImage.sprite = Rounded;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Cyan;

            // Handle slide area
            RectTransform handleArea = CreateRect(rect, "Handle Slide Area");
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(10f, 0f);
            handleArea.offsetMax = new Vector2(-10f, 0f);

            RectTransform handleRect = CreateRect(handleArea, "Handle");
            handleRect.sizeDelta = new Vector2(20f, 0f);
            Image handleImage = handleRect.gameObject.AddComponent<Image>();
            handleImage.sprite = Rounded;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = Color.white;

            Slider slider = rect.gameObject.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = i_Min;
            slider.maxValue = i_Max;
            slider.value = i_Value;

            return slider;
        }

        public static Toggle CreateToggle(
            Transform i_Parent,
            string i_Name,
            string i_Label,
            float i_Width,
            float i_Height,
            bool i_IsOn)
        {
            RectTransform rect = CreateRect(i_Parent, i_Name);
            rect.sizeDelta = new Vector2(i_Width, i_Height);

            // Checkbox box
            RectTransform boxRect = CreateRect(rect, "Background");
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = new Vector2(0f, 0f);
            boxRect.sizeDelta = new Vector2(32f, 32f);

            Image boxImage = boxRect.gameObject.AddComponent<Image>();
            boxImage.sprite = Rounded;
            boxImage.type = Image.Type.Sliced;
            boxImage.color = ButtonFill;

            // Outline
            Image boxOutline = CreateRect(boxRect, "Outline").gameObject.AddComponent<Image>();
            boxOutline.sprite = RoundedOutline;
            boxOutline.type = Image.Type.Sliced;
            boxOutline.color = CyanDim;
            boxOutline.raycastTarget = false;
            StretchToParent(boxOutline.rectTransform);

            // Checkmark
            RectTransform checkRect = CreateRect(boxRect, "Checkmark");
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            Image checkImage = checkRect.gameObject.AddComponent<Image>();
            checkImage.sprite = Rounded;
            checkImage.color = Cyan;

            // Label
            TextMeshProUGUI label = CreateText(rect, "Label", i_Label, Body, TextPrimary, TextAlignmentOptions.Left);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.pivot = new Vector2(0f, 0.5f);
            label.rectTransform.offsetMin = new Vector2(48f, 0f);
            label.rectTransform.offsetMax = Vector2.zero;

            Toggle toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = i_IsOn;

            return toggle;
        }

        public static void StretchToParent(RectTransform i_Rect, float i_Padding = 0f)
        {
            i_Rect.anchorMin = Vector2.zero;
            i_Rect.anchorMax = Vector2.one;
            i_Rect.offsetMin = new Vector2(i_Padding, i_Padding);
            i_Rect.offsetMax = new Vector2(-i_Padding, -i_Padding);
            i_Rect.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>Forces a scene Canvas (and every created Canvas) onto the shared reference.</summary>
        public static void ApplyCanvasScaler(Canvas i_Canvas)
        {
            if (i_Canvas == null) return;

            CanvasScaler scaler = i_Canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = i_Canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
}
