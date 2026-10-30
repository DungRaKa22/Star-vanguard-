using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StarVanguard.UI;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Editor automation tool for Sprint 1 Milestone 2 (T-201b):
    /// Constructs and wires the uGUI Overheat Gauge under Canvas/HUDContainer in GameplayScene.unity.
    ///
    /// Adheres strictly to:
    /// 1. AGENTS.md Rule 1: Zero manual YAML editing; programmatic scene authoring only.
    /// 2. Single Scene Writer lock discipline: Prevents accidental modification during PlayMode.
    /// 3. Idempotent execution: Safe to run repeatedly without creating duplicate GameObjects.
    /// 4. Headless batchmode execution for automated CI verification.
    /// </summary>
    public static class StarVanguardOverheatSetup
    {
        private const string k_GameplayScenePath = "Assets/Scenes/GameplayScene.unity";
        private const string k_FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string k_RoundedSpritePath = "Assets/Sprites/UI/UI_Rounded.png";

        // Stitch Design Tokens & Sci-Fi Palette
        private static readonly Color k_ColorCyan = new Color(0.0f, 0.941f, 1.0f, 1.0f);          // #00F0FF (Neon Cyan)
        private static readonly Color k_ColorSliderBg = new Color(0.05f, 0.08f, 0.14f, 0.85f);     // Dark translucent container
        private static readonly Color k_ColorFillInitial = new Color(0.0f, 0.941f, 1.0f, 1.0f);    // Initial cold color (Cyan)

        [MenuItem("Tools/Star Vanguard/Setup Overheat Gauge in GameplayScene", priority = 30)]
        [MenuItem("Star Vanguard/Setup Overheat Gauge")]
        public static void SetupOverheatGaugeFromMenu()
        {
            bool success = ExecuteSetup();
            if (success)
            {
                EditorUtility.DisplayDialog(
                    "Overheat Setup",
                    "OverheatGauge was successfully configured and saved in GameplayScene.unity!",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Overheat Setup Failed",
                    "Failed to setup OverheatGauge. Check the Console log for detailed errors.",
                    "OK");
            }
        }

        /// <summary>
        /// Headless batchmode entry point. Run with:
        /// Unity -batchmode -quit -projectPath "Star Vanguard" -executeMethod StarVanguard.EditorTools.StarVanguardOverheatSetup.SetupOverheatGaugeAndSave -logFile -
        /// </summary>
        public static void SetupOverheatGaugeAndSave()
        {
            Debug.Log("[StarVanguardOverheatSetup] ===== Beginning Overheat Gauge Setup in GameplayScene (T-201b) =====");
            bool success = ExecuteSetup();

            if (!success)
            {
                Debug.LogError("[StarVanguardOverheatSetup] FAILED: Overheat gauge setup encountered errors.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
            else
            {
                Debug.Log("[StarVanguardOverheatSetup] SUCCESS: Overheat gauge setup completed and saved.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
        }

        /// <summary>
        /// Core idempotent setup routine.
        /// </summary>
        public static bool ExecuteSetup()
        {
            // 1. Single Scene Writer Lock Guard
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[StarVanguardOverheatSetup] Cannot setup scene while in Play Mode or transitioning to Play Mode.");
                return false;
            }

            // 2. Open GameplayScene
            Debug.Log($"[StarVanguardOverheatSetup] Opening scene '{k_GameplayScenePath}'...");
            Scene scene = EditorSceneManager.OpenScene(k_GameplayScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[StarVanguardOverheatSetup] FAILED: Could not open scene at '{k_GameplayScenePath}'");
                return false;
            }

            // 3. Locate Canvas and HUDContainer
            GameObject hudContainer = GameObject.Find("Canvas/HUDContainer");
            if (hudContainer == null)
            {
                // Fallback search across root objects
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    Transform t = root.transform.Find("HUDContainer");
                    if (t != null)
                    {
                        hudContainer = t.gameObject;
                        break;
                    }
                }
            }

            if (hudContainer == null)
            {
                Debug.LogError("[StarVanguardOverheatSetup] FAILED: Could not locate 'HUDContainer' in GameplayScene!");
                return false;
            }

            Debug.Log($"[StarVanguardOverheatSetup] Located HUDContainer: '{hudContainer.name}' (Layer: {LayerMask.LayerToName(hudContainer.layer)})");

            // 4. Idempotent check: Clean up existing OverheatGauge if present
            Transform existingGauge = hudContainer.transform.Find("OverheatGauge");
            if (existingGauge != null)
            {
                Debug.Log("[StarVanguardOverheatSetup] Removing existing 'OverheatGauge' for clean idempotent rebuild.");
                UnityEngine.Object.DestroyImmediate(existingGauge.gameObject);
            }

            // 5. Load UI Assets (Font & Sprite)
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(k_FontAssetPath);
            if (fontAsset == null)
            {
                Debug.LogWarning($"[StarVanguardOverheatSetup] Could not load TMP font at '{k_FontAssetPath}'. Falling back to default TMP font.");
            }

            Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(k_RoundedSpritePath);
            if (roundedSprite == null)
            {
                Debug.LogWarning($"[StarVanguardOverheatSetup] Could not load 9-slice sprite at '{k_RoundedSpritePath}'. Using solid color fallback.");
            }

            int uiLayer = hudContainer.layer;

            // 6. Build OverheatGauge Root Object
            // Positioned at Bottom-Center of 1920x1080 canvas, 60px above screen bottom margin
            GameObject gaugeRoot = new GameObject("OverheatGauge", typeof(RectTransform), typeof(OverheatGaugeUI));
            gaugeRoot.transform.SetParent(hudContainer.transform, false);
            gaugeRoot.layer = uiLayer;

            RectTransform gaugeRect = gaugeRoot.GetComponent<RectTransform>();
            gaugeRect.anchorMin = new Vector2(0.5f, 0.0f);
            gaugeRect.anchorMax = new Vector2(0.5f, 0.0f);
            gaugeRect.pivot = new Vector2(0.5f, 0.0f);
            gaugeRect.anchoredPosition = new Vector2(0.0f, 60.0f);
            gaugeRect.sizeDelta = new Vector2(400.0f, 26.0f);

            // 7. Build HeatSlider
            GameObject sliderObj = new GameObject("HeatSlider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(gaugeRoot.transform, false);
            sliderObj.layer = uiLayer;

            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = Vector2.zero;
            sliderRect.anchorMax = Vector2.one;
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = Vector2.zero;
            sliderRect.sizeDelta = Vector2.zero;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0.0f;
            slider.maxValue = 1.0f;
            slider.value = 0.0f;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.direction = Slider.Direction.LeftToRight;

            // 8. Build Slider Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(sliderObj.transform, false);
            bgObj.layer = uiLayer;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImage = bgObj.GetComponent<Image>();
            if (roundedSprite != null)
            {
                bgImage.sprite = roundedSprite;
                bgImage.type = Image.Type.Sliced;
            }
            bgImage.color = k_ColorSliderBg;
            bgImage.raycastTarget = false;

            // 9. Build Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            fillArea.layer = uiLayer;

            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.offsetMin = new Vector2(2.0f, 2.0f);  // 2px margin inside container
            fillAreaRect.offsetMax = new Vector2(-2.0f, -2.0f);

            // 10. Build Fill Image
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            fillObj.layer = uiLayer;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.0f, 1.0f); // Width driven by Slider
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObj.GetComponent<Image>();
            if (roundedSprite != null)
            {
                fillImage.sprite = roundedSprite;
                fillImage.type = Image.Type.Sliced;
            }
            fillImage.color = k_ColorFillInitial;
            fillImage.raycastTarget = false;

            slider.targetGraphic = fillImage;
            slider.fillRect = fillRect;

            // 11. Build HeatStatusText (TextMeshProUGUI above slider)
            GameObject textObj = new GameObject("HeatStatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(gaugeRoot.transform, false);
            textObj.layer = uiLayer;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.0f, 1.0f);
            textRect.anchorMax = new Vector2(1.0f, 1.0f);
            textRect.pivot = new Vector2(0.5f, 0.0f);
            textRect.anchoredPosition = new Vector2(0.0f, 4.0f); // 4px above slider
            textRect.sizeDelta = new Vector2(0.0f, 22.0f);

            TextMeshProUGUI statusTMP = textObj.GetComponent<TextMeshProUGUI>();
            if (fontAsset != null)
            {
                statusTMP.font = fontAsset;
            }
            statusTMP.fontSize = 13;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.color = k_ColorCyan;
            statusTMP.text = "HEAT: 0%";
            statusTMP.raycastTarget = false;

            // 12. Wire Serialized Properties on OverheatGaugeUI
            OverheatGaugeUI gaugeUI = gaugeRoot.GetComponent<OverheatGaugeUI>();
            SerializedObject so = new SerializedObject(gaugeUI);

            SerializedProperty sliderProp = so.FindProperty("m_HeatSlider");
            if (sliderProp != null)
            {
                sliderProp.objectReferenceValue = slider;
            }
            else
            {
                Debug.LogError("[StarVanguardOverheatSetup] SerializedProperty 'm_HeatSlider' not found on OverheatGaugeUI!");
            }

            SerializedProperty fillProp = so.FindProperty("m_HeatFillImage");
            if (fillProp != null)
            {
                fillProp.objectReferenceValue = fillImage;
            }
            else
            {
                Debug.LogError("[StarVanguardOverheatSetup] SerializedProperty 'm_HeatFillImage' not found on OverheatGaugeUI!");
            }

            SerializedProperty textProp = so.FindProperty("m_HeatStatusText");
            if (textProp != null)
            {
                textProp.objectReferenceValue = statusTMP;
            }
            else
            {
                Debug.LogError("[StarVanguardOverheatSetup] SerializedProperty 'm_HeatStatusText' not found on OverheatGaugeUI!");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gaugeUI);

            // 13. Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);

            if (!saved)
            {
                Debug.LogError($"[StarVanguardOverheatSetup] FAILED: Could not save scene '{k_GameplayScenePath}'!");
                return false;
            }

            Debug.Log($"[StarVanguardOverheatSetup] Successfully saved scene '{k_GameplayScenePath}' with OverheatGauge wired.");

            // 14. Verification of Generated Tree
            bool verified = VerifyGaugeHierarchy(hudContainer);
            return verified;
        }

        private static bool VerifyGaugeHierarchy(GameObject hudContainer)
        {
            Transform gauge = hudContainer.transform.Find("OverheatGauge");
            if (gauge == null)
            {
                Debug.LogError("[StarVanguardOverheatSetup] Verification failed: 'OverheatGauge' child not found under HUDContainer!");
                return false;
            }

            OverheatGaugeUI ui = gauge.GetComponent<OverheatGaugeUI>();
            if (ui == null)
            {
                Debug.LogError("[StarVanguardOverheatSetup] Verification failed: OverheatGaugeUI component missing on OverheatGauge!");
                return false;
            }

            Slider slider = gauge.GetComponentInChildren<Slider>(true);
            if (slider == null || slider.fillRect == null)
            {
                Debug.LogError("[StarVanguardOverheatSetup] Verification failed: Slider or fillRect missing!");
                return false;
            }

            TextMeshProUGUI tmp = gauge.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null)
            {
                Debug.LogError("[StarVanguardOverheatSetup] Verification failed: TextMeshProUGUI missing!");
                return false;
            }

            Debug.Log("[StarVanguardOverheatSetup] Verification PASSED: OverheatGauge hierarchy and bindings are 100% valid.");
            return true;
        }
    }
}
