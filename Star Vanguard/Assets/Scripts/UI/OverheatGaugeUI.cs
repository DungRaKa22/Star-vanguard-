using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Futuristic Sci-Fi HUD Component for Weapon Overheat, Firing Rhythm, and Energy Crystal Bomb Manufacturing.
/// - Locked strictly to 1920x1080 Full HD Viewport.
/// - Renders a dynamic firing rhythm cadence pulse on each shot.
/// - Displays continuous heat gauge with 3-tier color shifting (Cyan -> Amber -> Danger Red).
/// - Flashes warning alert with cooldown timer when weapon is overheated/jammed.
/// - Tracks 50-Crystal manufacturing progress towards +1 Plasma Bomb.
/// </summary>
public class OverheatGaugeUI : MonoBehaviour
{
    [Header("Optional Canvas UI Bindings")]
    [SerializeField] private Slider m_HeatSlider;
    [SerializeField] private Image m_HeatFillImage;
    [SerializeField] private TextMeshProUGUI m_HeatStatusText;

    [Header("Procedural HUD Display")]
    [SerializeField] private bool m_EnableProceduralHUD = true;

    private float m_NormalizedHeat = 0f;
    private bool m_IsOverheated = false;
    private float m_OverheatCooldownTimer = 0f;
    private const float k_OverheatDuration = 2.85f;

    private int m_CurrentCrystals = 0;
    private int m_MaxCrystals = 50;
    private string m_WeaponName = "RED LASER";
    private int m_WeaponLevel = 1;

    // Firing rhythm pulse animation
    private float m_RhythmPulse = 0f;

    // Zero-alloc GUI textures
    private Texture2D m_BgTex;
    private Texture2D m_BorderTex;
    private Texture2D m_CyanTex;
    private Texture2D m_YellowTex;
    private Texture2D m_RedTex;
    private Texture2D m_EmeraldTex;
    private GUIStyle m_LabelStyle;
    private GUIStyle m_WarningStyle;
    private GUIStyle m_WeaponBadgeStyle;
    private bool m_StylesInitialized = false;

    private void Awake()
    {
        createTextures();
    }

    private void OnEnable()
    {
        WeaponHeatController.OnAnyHeatChanged += HandleHeatChanged;
        WeaponHeatController.OnAnyOverheatStateChanged += HandleOverheatStateChanged;
        PlayerStats.OnCrystalChargeChanged += HandleCrystalChargeChanged;
        PlayerShooting.OnWeaponChanged += HandleWeaponChanged;
        PlayerShooting.OnShotFiredRhythm += HandleShotFiredRhythm;
    }

    private void OnDisable()
    {
        WeaponHeatController.OnAnyHeatChanged -= HandleHeatChanged;
        WeaponHeatController.OnAnyOverheatStateChanged -= HandleOverheatStateChanged;
        PlayerStats.OnCrystalChargeChanged -= HandleCrystalChargeChanged;
        PlayerShooting.OnWeaponChanged -= HandleWeaponChanged;
        PlayerShooting.OnShotFiredRhythm -= HandleShotFiredRhythm;
    }

    private void Update()
    {
        // Smoothly decay firing rhythm pulse
        if (m_RhythmPulse > 0f)
        {
            m_RhythmPulse = Mathf.MoveTowards(m_RhythmPulse, 0f, Time.deltaTime * 5f);
        }

        // Count down overheat penalty
        if (m_IsOverheated && m_OverheatCooldownTimer > 0f)
        {
            m_OverheatCooldownTimer = Mathf.Max(0f, m_OverheatCooldownTimer - Time.deltaTime);
        }
    }

    private void HandleShotFiredRhythm()
    {
        m_RhythmPulse = 1.0f;
    }

    private void HandleWeaponChanged(eWeaponType weapon, int level)
    {
        m_WeaponName = weapon switch
        {
            eWeaponType.RedLaser => "RED LASER",
            eWeaponType.GreenNeutron => "GREEN NEUTRON",
            eWeaponType.YellowVulcan => "YELLOW VULCAN",
            eWeaponType.PurplePlasma => "PURPLE PLASMA",
            eWeaponType.LaserCannon => "LASER CANNON",
            _ => "RED LASER"
        };
        m_WeaponLevel = level;
    }

    private void HandleHeatChanged(float heat)
    {
        m_NormalizedHeat = heat;
        if (m_HeatSlider != null) m_HeatSlider.value = heat;
        if (m_HeatFillImage != null)
        {
            m_HeatFillImage.fillAmount = heat;
            m_HeatFillImage.color = GetHeatColor(heat);
        }
    }

    private void HandleOverheatStateChanged(bool isOverheated)
    {
        m_IsOverheated = isOverheated;
        if (isOverheated)
        {
            m_OverheatCooldownTimer = k_OverheatDuration;
            AudioManager.Instance?.Play(eSFXId.PlasmaImpact, 0.9f, 0.5f);
        }
    }

    private void HandleCrystalChargeChanged(int current, int max)
    {
        m_CurrentCrystals = current;
        m_MaxCrystals = max;
    }

    public Color GetHeatColor(float heat)
    {
        if (heat < 0.40f) return new Color(0.0f, 0.95f, 1.0f); // Neon Cyan
        if (heat < 0.75f) return new Color(1.0f, 0.82f, 0.0f); // Amber Yellow
        return new Color(1.0f, 0.15f, 0.2f);                   // Danger Red
    }

    private void createTextures()
    {
        m_BgTex = makeColorTexture(new Color(0.04f, 0.08f, 0.16f, 0.92f));
        m_BorderTex = makeColorTexture(new Color(0.12f, 0.35f, 0.55f, 0.95f));
        m_CyanTex = makeColorTexture(new Color(0.0f, 0.95f, 1.0f, 0.95f));
        m_YellowTex = makeColorTexture(new Color(1.0f, 0.85f, 0.1f, 0.95f));
        m_RedTex = makeColorTexture(new Color(1.0f, 0.2f, 0.2f, 0.95f));
        m_EmeraldTex = makeColorTexture(new Color(0.1f, 1.0f, 0.5f, 0.95f));
    }

    private Texture2D makeColorTexture(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    private void OnDestroy()
    {
        if (m_BgTex) Destroy(m_BgTex);
        if (m_BorderTex) Destroy(m_BorderTex);
        if (m_CyanTex) Destroy(m_CyanTex);
        if (m_YellowTex) Destroy(m_YellowTex);
        if (m_RedTex) Destroy(m_RedTex);
        if (m_EmeraldTex) Destroy(m_EmeraldTex);
    }

    private void initStyles()
    {
        if (m_StylesInitialized) return;

        float uiScale = Screen.height / 1080f;

        m_LabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(12f * uiScale), 10, 16),
            fontStyle = FontStyle.Bold
        };
        m_LabelStyle.normal.textColor = new Color(0.85f, 0.95f, 1.0f);

        m_WarningStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(13f * uiScale), 11, 18),
            fontStyle = FontStyle.Bold
        };
        m_WarningStyle.normal.textColor = new Color(1.0f, 0.25f, 0.25f);

        m_WeaponBadgeStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(11f * uiScale), 9, 15),
            fontStyle = FontStyle.Bold
        };
        m_WeaponBadgeStyle.normal.textColor = new Color(0.3f, 1.0f, 0.8f);

        m_StylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!m_EnableProceduralHUD) return;
        if (GameManager.Instance == null || GameManager.Instance.GameState != eGameState.Playing) return;

        initStyles();

        // 1080p Reference bounds calculated strictly inside camera rect
        Camera cam = Camera.main;
        Rect camPixelRect = cam != null ? cam.pixelRect : new Rect(0, 0, Screen.width, Screen.height);

        float uiScale = camPixelRect.height / 1080f;
        float barWidth = 380f * uiScale;
        float heatBarHeight = 14f * uiScale;
        float crystalBarHeight = 8f * uiScale;

        // Apply firing rhythm micro-pulse expansion
        float pulseExpand = m_RhythmPulse * 6f * uiScale;
        float finalBarWidth = barWidth + pulseExpand;

        float centerX = camPixelRect.x + (camPixelRect.width * 0.5f);
        float bottomY = camPixelRect.y + 60f * uiScale; // Positioned cleanly at lower screen margin

        // 0. TOP HUD STATUS BARS (Top-Left Score, Top-Right Lives & Bombs)
        int score = GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0;
        int lives = GameManager.Instance != null ? GameManager.Instance.CurrentLives : 3;
        var playerStats = FindAnyObjectByType<PlayerStats>();
        int bombs = playerStats != null ? playerStats.BombCount : 0;

        float topMarginY = camPixelRect.y + (20f * uiScale);

        // Top-Left Score Badge
        Rect scoreRect = new Rect(camPixelRect.x + (25f * uiScale), topMarginY, 220f * uiScale, 36f * uiScale);
        GUI.DrawTexture(scoreRect, m_BgTex);
        GUI.Label(scoreRect, $"⭐ SCORE: {score:D6}", m_LabelStyle);

        // Top-Right Lives and Bombs Badge
        float statsWidth = 260f * uiScale;
        Rect statsRect = new Rect(camPixelRect.x + camPixelRect.width - statsWidth - (25f * uiScale), topMarginY, statsWidth, 36f * uiScale);
        GUI.DrawTexture(statsRect, m_BgTex);
        GUI.Label(statsRect, $"❤️ HULL: {lives}   |   💣 BOMBS: {bombs}", m_LabelStyle);

        // 1. OVERHEAT HEAT GAUGE (Bottom center)
        float heatBarX = centerX - (finalBarWidth * 0.5f);
        float heatBarY = Screen.height - bottomY;

        // Background box with border
        Rect heatOuterRect = new Rect(heatBarX - 2, heatBarY - 2, finalBarWidth + 4, heatBarHeight + 4);
        GUI.DrawTexture(heatOuterRect, m_BorderTex);

        Rect heatBgRect = new Rect(heatBarX, heatBarY, finalBarWidth, heatBarHeight);
        GUI.DrawTexture(heatBgRect, m_BgTex);

        // Heat fill
        float fillWidth = finalBarWidth * Mathf.Clamp01(m_NormalizedHeat);
        if (fillWidth > 1f)
        {
            Texture2D fillTex = m_NormalizedHeat < 0.40f ? m_CyanTex : (m_NormalizedHeat < 0.75f ? m_YellowTex : m_RedTex);
            GUI.DrawTexture(new Rect(heatBarX, heatBarY, fillWidth, heatBarHeight), fillTex);
        }

        // Overheat status banner
        string heatText;
        if (m_IsOverheated)
        {
            bool blink = (Time.unscaledTime * 5f % 1f) > 0.5f;
            heatText = blink ? $"⚠️ WEAPON OVERHEATED - COOLING [{m_OverheatCooldownTimer:F1}s]" : "⚠️ OVERHEATED ⚠️";
        }
        else
        {
            heatText = $"HEAT LEVEL  [{Mathf.RoundToInt(m_NormalizedHeat * 100)}%]";
        }

        GUIStyle statusStyle = m_IsOverheated ? m_WarningStyle : m_LabelStyle;
        GUI.Label(new Rect(centerX - (200f * uiScale), heatBarY - (22f * uiScale), 400f * uiScale, 20f * uiScale), heatText, statusStyle);

        // 2. CRYSTAL ENERGY BOMB ACCUMULATOR BAR (Just above overheat bar)
        float crystalBarY = heatBarY - (crystalBarHeight + 26f * uiScale);
        float crystalBarX = centerX - (barWidth * 0.5f);

        Rect crystalOuterRect = new Rect(crystalBarX - 1, crystalBarY - 1, barWidth + 2, crystalBarHeight + 2);
        GUI.DrawTexture(crystalOuterRect, m_BorderTex);

        Rect crystalBgRect = new Rect(crystalBarX, crystalBarY, barWidth, crystalBarHeight);
        GUI.DrawTexture(crystalBgRect, m_BgTex);

        float crystalFill = barWidth * Mathf.Clamp01((float)m_CurrentCrystals / Mathf.Max(1, m_MaxCrystals));
        if (crystalFill > 1f)
        {
            GUI.DrawTexture(new Rect(crystalBarX, crystalBarY, crystalFill, crystalBarHeight), m_EmeraldTex);
        }

        string crystalText = $"CRYSTAL ENERGY  [{m_CurrentCrystals} / {m_MaxCrystals}]  ->  +1 PLASMA BOMB";
        GUI.Label(new Rect(centerX - (220f * uiScale), crystalBarY - (18f * uiScale), 440f * uiScale, 18f * uiScale), crystalText, m_LabelStyle);

        // 3. WEAPON IDENTIFIER BADGE (Below overheat bar)
        string weaponBadge = $"⚡ {m_WeaponName}  [LV.{m_WeaponLevel}]  •  KEYS 1-5";
        GUI.Label(new Rect(centerX - (200f * uiScale), heatBarY + heatBarHeight + (4f * uiScale), 400f * uiScale, 20f * uiScale), weaponBadge, m_WeaponBadgeStyle);
    }
}
