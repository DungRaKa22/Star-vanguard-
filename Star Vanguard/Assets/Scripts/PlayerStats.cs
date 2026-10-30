using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Weapon")]
    [Range(1, 8), SerializeField] private int m_WeaponLevel = 1;

    [Header("Shield")]
    [SerializeField] private float m_ShieldDuration = 4f;
    [SerializeField] private GameObject m_ShieldVisualPrefab;
    private GameObject m_ShieldVisualInstance;
    public bool ShieldActive { get; private set; }
    
    [Header("Bombs")]
    [SerializeField] private int m_BombCount;
    private const int k_MaxBombs = 10;

    [Header("Energy Charge (50 Crystals = 1 Bomb)")]
    [SerializeField] private int m_CrystalCharge = 0;
    public const int k_CrystalsPerBomb = 50;
    public static event System.Action<int, int> OnCrystalChargeChanged; // (current, max)

    public int CrystalCharge => m_CrystalCharge;

    private void Start()
    {
        UIManager.Instance?.SetBombs(m_BombCount);
        
        // Create shield visual instance once at start and keep it disabled
        if (m_ShieldVisualPrefab && !m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance = Instantiate(m_ShieldVisualPrefab, transform);
            m_ShieldVisualInstance.transform.localPosition = Vector3.zero;
            m_ShieldVisualInstance.SetActive(false); // Start disabled
        }
    }

    public int WeaponLevel
    {
        get => m_WeaponLevel;
        private set => m_WeaponLevel = value;
    }
    
    public int BombCount => m_BombCount;
    
    public void AddWeaponLevel(int i_Delta)
    {
        WeaponLevel = Mathf.Clamp(WeaponLevel + i_Delta, 1, 8);
        var shooting = GetComponent<PlayerShooting>() ?? FindAnyObjectByType<PlayerShooting>();
        if (shooting != null)
        {
            shooting.SwitchWeapon(shooting.ActiveWeapon);
        }
    }
    
    public void AddBomb(int i_Delta)
    {
        int oldCount = m_BombCount;
        m_BombCount = Mathf.Clamp(m_BombCount + i_Delta, 0, k_MaxBombs);
        UIManager.Instance?.SetBombs(m_BombCount);
        
        // Play pickup sound when gaining bombs (not when using them)
        if (i_Delta > 0 && m_BombCount > oldCount)
        {
            AudioManager.Instance?.Play(eSFXId.Pickup);
        }
    }

    /// <summary>
    /// Accumulates energy crystal charge. When reaching 50 crystals,
    /// automatically manufactures +1 Plasma Bomb (REA Verified Spec).
    /// </summary>
    public void AddCrystalCharge(int amount)
    {
        m_CrystalCharge += amount;
        if (m_CrystalCharge >= k_CrystalsPerBomb)
        {
            m_CrystalCharge -= k_CrystalsPerBomb;
            AddBomb(1);
            AudioManager.Instance?.Play(eSFXId.Pickup, 0.8f);
        }
        OnCrystalChargeChanged?.Invoke(m_CrystalCharge, k_CrystalsPerBomb);
    }
    
    /// <summary>
    /// Reset player stats to initial values - called when starting new game
    /// </summary>
    public void ResetStats()
    {
        // Reset to initial values
        m_WeaponLevel = 1;
        m_BombCount = 0;
        m_CrystalCharge = 0;
        OnCrystalChargeChanged?.Invoke(0, k_CrystalsPerBomb);
        
        // Stop any active shield
        StopAllCoroutines();
        ShieldActive = false;
        if (m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance.SetActive(false);
        }
        
        // Update UI
        UIManager.Instance?.SetBombs(m_BombCount);
    }
    
    public void ActivateShield()
    {
        if (ShieldActive) StopAllCoroutines();
        StartCoroutine(shieldCR(m_ShieldDuration));
    }

    private IEnumerator shieldCR(float i_Duration)
    {
        ShieldActive = true;
        if(m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance.SetActive(true);
        }
        
        yield return new WaitForSeconds(i_Duration);
        
        ShieldActive = false;
        if(m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance.SetActive(false);
        }
    }
}
