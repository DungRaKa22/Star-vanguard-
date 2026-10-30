using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarVanguard.UI
{
    /// <summary>
    /// Pure uGUI Sci-Fi Overheat Gauge HUD for Star Vanguard.
    /// Replaces legacy IMGUI OnGUI implementation with zero-allocation uGUI architecture:
    /// - Serialized uGUI Slider, fill Image, and TextMeshProUGUI status text.
    /// - Static event subscription to WeaponHeatController.OnHeatChanged and WeaponHeatController.OnOverheated.
    /// - Zero-GC gameplay: TMP_Text.SetText overloads, cached color structs, pre-cached weapon strings.
    /// - Zero Find* / Camera.main / GetComponent calls in runtime hot paths.
    /// </summary>
    [DisallowMultipleComponent]
    public class OverheatGaugeUI : MonoBehaviour
    {
        [Header("uGUI Core Bindings")]
        [SerializeField] private Slider m_HeatSlider;
        [SerializeField] private Image m_HeatFillImage;
        [SerializeField] private TextMeshProUGUI m_HeatStatusText;

        [Header("Optional Secondary Bindings")]
        [SerializeField] private Slider m_CrystalSlider;
        [SerializeField] private TextMeshProUGUI m_CrystalText;
        [SerializeField] private TextMeshProUGUI m_WeaponText;

        // Static cached colors (Zero allocation struct copies)
        private static readonly Color k_ColorCyan = new Color(0.0f, 0.95f, 1.0f, 1.0f);     // Heat < 40% (Neon Cyan)
        private static readonly Color k_ColorAmber = new Color(1.0f, 0.82f, 0.0f, 1.0f);    // Heat 40% - 75% (Amber Yellow)
        private static readonly Color k_ColorRed = new Color(1.0f, 0.15f, 0.2f, 1.0f);      // Heat >= 75% (Alert Red)

        // Zero-GC string constants for TextMeshProUGUI.SetText
        private const string k_HeatFormat = "HEAT: {0:0}%";
        private const string k_OverheatedFormat = "OVERHEATED [{0:0}%]";
        private const string k_OverheatedText = "OVERHEATED";
        private const string k_OptimalText = "OPTIMAL";
        private const string k_CrystalFormat = "CRYSTALS: {0:0}/{1:0}";

        // Pre-cached weapon label matrix (5 weapons x 8 levels) for zero-allocation weapon updates
        private static readonly string[,] s_WeaponLabels;

        // Runtime state
        private float m_NormalizedHeat = 0f;
        private bool m_IsOverheated = false;
        private bool m_IsSubscribed = false;

        // Public properties for testing, verification, and inspection
        public float NormalizedHeat => m_NormalizedHeat;
        public bool IsOverheated => m_IsOverheated;
        public Slider HeatSlider => m_HeatSlider;
        public Image HeatFillImage => m_HeatFillImage;
        public TextMeshProUGUI HeatStatusText => m_HeatStatusText;
        public Slider CrystalSlider => m_CrystalSlider;
        public TextMeshProUGUI CrystalText => m_CrystalText;
        public TextMeshProUGUI WeaponText => m_WeaponText;

        static OverheatGaugeUI()
        {
            string[] weaponNames = new string[]
            {
                "RED LASER",
                "GREEN NEUTRON",
                "YELLOW VULCAN",
                "PURPLE PLASMA",
                "LASER CANNON"
            };

            s_WeaponLabels = new string[5, 8];
            for (int w = 0; w < 5; w++)
            {
                for (int lvl = 0; lvl < 8; lvl++)
                {
                    s_WeaponLabels[w, lvl] = $"{weaponNames[w]} [LV.{lvl + 1}]";
                }
            }
        }

        private void Awake()
        {
            // Defensive runtime binding fallback if fields were not assigned in Inspector/Prefab
            if (m_HeatSlider == null)
            {
                m_HeatSlider = GetComponentInChildren<Slider>(true);
            }

            if (m_HeatFillImage == null && m_HeatSlider != null && m_HeatSlider.fillRect != null)
            {
                m_HeatFillImage = m_HeatSlider.fillRect.GetComponent<Image>();
            }

            if (m_HeatStatusText == null)
            {
                m_HeatStatusText = GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();

            // Synchronize with existing runtime state if weapon controller is already active
            if (WeaponHeatController.Instance != null)
            {
                HandleHeatChanged(WeaponHeatController.Instance.CurrentHeat);
                HandleOverheated(WeaponHeatController.Instance.IsOverheated);
            }
            else
            {
                ResetUI();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (m_IsSubscribed) return;

            WeaponHeatController.OnHeatChanged += HandleHeatChanged;
            WeaponHeatController.OnOverheated += HandleOverheated;
            PlayerStats.OnCrystalChargeChanged += HandleCrystalChargeChanged;
            PlayerShooting.OnWeaponChanged += HandleWeaponChanged;

            m_IsSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!m_IsSubscribed) return;

            WeaponHeatController.OnHeatChanged -= HandleHeatChanged;
            WeaponHeatController.OnOverheated -= HandleOverheated;
            PlayerStats.OnCrystalChargeChanged -= HandleCrystalChargeChanged;
            PlayerShooting.OnWeaponChanged -= HandleWeaponChanged;

            m_IsSubscribed = false;
        }

        /// <summary>
        /// Handles weapon heat change event.
        /// Zero GC allocation guaranteed: uses TMP_Text.SetText formatting and cached color structs.
        /// </summary>
        /// <param name="i_Heat">Normalized heat in range [0..1]</param>
        private void HandleHeatChanged(float i_Heat)
        {
            m_NormalizedHeat = Mathf.Clamp01(i_Heat);

            if (m_HeatSlider != null)
            {
                m_HeatSlider.value = m_NormalizedHeat;
            }

            if (m_HeatFillImage != null)
            {
                m_HeatFillImage.color = GetHeatColor(m_NormalizedHeat);
            }

            if (m_HeatStatusText != null)
            {
                if (m_IsOverheated)
                {
                    m_HeatStatusText.color = k_ColorRed;
                    m_HeatStatusText.SetText(k_OverheatedFormat, Mathf.RoundToInt(m_NormalizedHeat * 100f));
                }
                else
                {
                    m_HeatStatusText.color = GetHeatColor(m_NormalizedHeat);
                    if (m_NormalizedHeat <= 0.001f)
                    {
                        m_HeatStatusText.SetText(k_OptimalText);
                    }
                    else
                    {
                        m_HeatStatusText.SetText(k_HeatFormat, Mathf.RoundToInt(m_NormalizedHeat * 100f));
                    }
                }
            }
        }

        /// <summary>
        /// Handles weapon overheat / jam state transition.
        /// </summary>
        /// <param name="i_IsOverheated">True when jammed, false when cooled</param>
        private void HandleOverheated(bool i_IsOverheated)
        {
            m_IsOverheated = i_IsOverheated;

            if (i_IsOverheated)
            {
                if (m_HeatFillImage != null)
                {
                    m_HeatFillImage.color = k_ColorRed;
                }

                if (m_HeatStatusText != null)
                {
                    m_HeatStatusText.color = k_ColorRed;
                    m_HeatStatusText.SetText(k_OverheatedText);
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play(eSFXId.PlasmaImpact, 0.9f, 0.5f);
                }
            }
            else
            {
                if (m_HeatStatusText != null)
                {
                    m_HeatStatusText.color = GetHeatColor(m_NormalizedHeat);
                    if (m_NormalizedHeat <= 0.001f)
                    {
                        m_HeatStatusText.SetText(k_OptimalText);
                    }
                    else
                    {
                        m_HeatStatusText.SetText(k_HeatFormat, Mathf.RoundToInt(m_NormalizedHeat * 100f));
                    }
                }
            }
        }

        /// <summary>
        /// Optional handler for crystal accumulator HUD.
        /// </summary>
        private void HandleCrystalChargeChanged(int i_Current, int i_Max)
        {
            if (m_CrystalSlider != null)
            {
                m_CrystalSlider.maxValue = Mathf.Max(1, i_Max);
                m_CrystalSlider.value = Mathf.Clamp(i_Current, 0, i_Max);
            }

            if (m_CrystalText != null)
            {
                m_CrystalText.SetText(k_CrystalFormat, i_Current, i_Max);
            }
        }

        /// <summary>
        /// Optional handler for active weapon badge HUD.
        /// </summary>
        private void HandleWeaponChanged(eWeaponType i_Weapon, int i_Level)
        {
            if (m_WeaponText == null) return;

            int weaponIdx = (int)i_Weapon;
            if (weaponIdx < 0 || weaponIdx >= 5) weaponIdx = 0;

            int levelIdx = Mathf.Clamp(i_Level - 1, 0, 7);
            m_WeaponText.SetText(s_WeaponLabels[weaponIdx, levelIdx]);
        }

        /// <summary>
        /// Resets the UI elements to their baseline zero-heat optimal state.
        /// </summary>
        public void ResetUI()
        {
            m_NormalizedHeat = 0f;
            m_IsOverheated = false;

            if (m_HeatSlider != null)
            {
                m_HeatSlider.value = 0f;
            }

            if (m_HeatFillImage != null)
            {
                m_HeatFillImage.color = k_ColorCyan;
            }

            if (m_HeatStatusText != null)
            {
                m_HeatStatusText.color = k_ColorCyan;
                m_HeatStatusText.SetText(k_OptimalText);
            }
        }

        /// <summary>
        /// Returns cached 3-tier color based on normalized heat level.
        /// Tier 1: Cyan (&lt; 40%)
        /// Tier 2: Amber Yellow (40% - 75%)
        /// Tier 3: Danger Red (&gt;= 75%)
        /// </summary>
        public static Color GetHeatColor(float i_Heat)
        {
            if (i_Heat < 0.40f) return k_ColorCyan;
            if (i_Heat < 0.75f) return k_ColorAmber;
            return k_ColorRed;
        }

        // Instance overload for backward compatibility
        public Color GetColorForHeat(float i_Heat) => GetHeatColor(i_Heat);
    }
}
