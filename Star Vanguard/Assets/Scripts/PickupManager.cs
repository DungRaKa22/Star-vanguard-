using UnityEngine;
using UnityEngine.Serialization;

public class PickupManager : Singleton<PickupManager>
{
    [Header("Prefabs")]
    [SerializeField] private GameObject m_WeaponPowerUpPrefab;
    [FormerlySerializedAs("m_FoodPrefab")]
    [SerializeField] private GameObject m_CrystalPrefab;

    [Header("Sprites for Drops")]
    [SerializeField] private Sprite m_NetherStarSprite;
    [SerializeField] private Sprite m_GiftBoxRed;
    [SerializeField] private Sprite m_GiftBoxGreen;
    [SerializeField] private Sprite m_GiftBoxYellow;
    [SerializeField] private Sprite m_GiftBoxPurple;
    
    [Header("Wave Tracking")]
    private int m_WeaponPowerUpsDroppedThisWave;
    private const int k_MaxWeaponPowerUps = 2;
    private const int k_MinWeaponPowerUps = 0;
    
    [Header("Drop Settings & REA Pity Counter")]
    [SerializeField] private float m_CrystalDropChance = 0.75f;
    [SerializeField] private float m_BasePowerUpChance = 0.05f;
    [SerializeField] private int m_PityThresholdKills = 15;
    [SerializeField] private int m_PityGuaranteedKills = 30;
    
    private int m_KillsSinceLastPowerUp = 0;
    
    public void OnWaveStart()
    {
        m_WeaponPowerUpsDroppedThisWave = 0;
    }
    
    public void OnEnemyKilled(Vector3 i_Position)
    {
        m_KillsSinceLastPowerUp++;
        
        // Drop energy crystal
        if (Random.value <= m_CrystalDropChance)
        {
            PoolManager.Instance?.GetCrystal(i_Position);
        }
        
        // Check if we should drop weapon power up with REA Pity Timer
        tryDropWeaponPowerUp(i_Position);
    }
    
    private void tryDropWeaponPowerUp(Vector3 i_Position)
    {
        // Don't drop more than max per wave
        if (m_WeaponPowerUpsDroppedThisWave >= k_MaxWeaponPowerUps) return;
        
        // Calculate drop chance based on REA Pity Timer formula:
        float dropChance = m_BasePowerUpChance;
        if (m_KillsSinceLastPowerUp >= m_PityGuaranteedKills)
        {
            dropChance = 1.0f; // Guaranteed drop
        }
        else if (m_KillsSinceLastPowerUp > m_PityThresholdKills)
        {
            // Linear progression from base to 100%
            float progress = (float)(m_KillsSinceLastPowerUp - m_PityThresholdKills) / (m_PityGuaranteedKills - m_PityThresholdKills);
            dropChance = Mathf.Lerp(m_BasePowerUpChance, 1.0f, progress);
        }
        
        if (Random.value <= dropChance && m_WeaponPowerUpPrefab)
        {
            var go = Instantiate(m_WeaponPowerUpPrefab, i_Position, Quaternion.identity);
            var pickup = go.GetComponent<PowerUpPickup>();

            if (pickup != null)
            {
                // 50% chance: Nether Star (Level Up)
                // 50% chance: Colored Gift Box (Weapon Switcher)
                if (Random.value < 0.5f)
                {
                    pickup.ConfigureAsLevelUp(m_NetherStarSprite);
                }
                else
                {
                    // Choose one of the 4 weapons randomly
                    eWeaponType[] availableWeapons = new eWeaponType[]
                    {
                        eWeaponType.RedLaser,
                        eWeaponType.GreenNeutron,
                        eWeaponType.YellowVulcan,
                        eWeaponType.PurplePlasma
                    };

                    eWeaponType selected = availableWeapons[Random.Range(0, availableWeapons.Length)];
                    Sprite selectedBox = selected switch
                    {
                        eWeaponType.RedLaser => m_GiftBoxRed,
                        eWeaponType.GreenNeutron => m_GiftBoxGreen,
                        eWeaponType.YellowVulcan => m_GiftBoxYellow,
                        eWeaponType.PurplePlasma => m_GiftBoxPurple,
                        _ => m_GiftBoxRed
                    };

                    pickup.ConfigureAsWeaponSwitcher(selected, selectedBox);
                }
            }

            m_WeaponPowerUpsDroppedThisWave++;
            m_KillsSinceLastPowerUp = 0; // Reset pity counter
        }
    }
    
    /// <summary>
    /// Spawn energy crystal at specific position (used by boss drops)
    /// </summary>
    public void SpawnCrystal(Vector3 i_Position)
    {
        PoolManager.Instance?.GetCrystal(i_Position);
    }

    // Backwards-compatible alias for legacy calls
    public void SpawnFood(Vector3 i_Position) => SpawnCrystal(i_Position);
}
