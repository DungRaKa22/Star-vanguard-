using UnityEngine;

/// <summary>
/// Weapon firing and arsenal management for Star Vanguard.
/// Implements 5 distinct weapon archetypes extracted from Chicken Invaders 5 (Uve Engine):
/// - Ion Blaster: Balanced twin cyan plasma bolts
/// - Neutron Piercer: Heavy armor-piercing green energy lance
/// - Vulcan Chaingun: High-speed rapid-fire kinetic stream with LCG PRNG spread
/// - Laser Cannon: High-damage red beam / rapid concentrated burst
/// - Plasma Swarm: Multi-angle spread purple energy orbs
/// Supports holding fire button (autofire) and switching weapons via keys 1..5.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    public static event System.Action<eWeaponType, int> OnWeaponChanged;
    public static event System.Action<Vector2, float, float, float, int, int, Color, Vector3> OnCustomBulletFired;
    public static event System.Action OnShotFiredRhythm;

    [Header("Components")]
    [SerializeField] private PlayerStats m_PlayerStats;
    [SerializeField] private Transform m_Muzzle;
    [SerializeField] private WeaponHeatController m_HeatController;

    [Header("Arsenal Settings")]
    [SerializeField] private eWeaponType m_ActiveWeapon = eWeaponType.RedLaser;

    [Header("Bullet Sprites")]
    [SerializeField] private Sprite m_RedLaserSprite;
    [SerializeField] private Sprite m_GreenNeutronSprite;
    [SerializeField] private Sprite m_YellowVulcanSprite;
    [SerializeField] private Sprite m_PurplePlasmaSprite;

    private float m_NextFireTime;
    private uint m_PrngSeed = 0x12345678;

    public eWeaponType ActiveWeapon => m_ActiveWeapon;

    private void Awake()
    {
        if (!m_HeatController)
        {
            m_HeatController = GetComponent<WeaponHeatController>();
        }
        if (!m_PlayerStats)
        {
            m_PlayerStats = GetComponent<PlayerStats>();
        }
    }

    private void Start()
    {
        OnWeaponChanged?.Invoke(m_ActiveWeapon, m_PlayerStats ? m_PlayerStats.WeaponLevel : 1);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.GameState != eGameState.Playing) return;

        // Weapon switching shortcuts (1..5)
        handleWeaponSwitchingInput();

        // Holding fire button (autofire) or click
        bool isFiring = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (isFiring && Time.time >= m_NextFireTime)
        {
            int level = m_PlayerStats ? m_PlayerStats.WeaponLevel : 1;
            if (m_HeatController != null && !m_HeatController.TryFire(level))
            {
                // Weapon is overheated/jammed
                return;
            }

            executeWeaponFire(m_ActiveWeapon, level);
        }
    }

    private void handleWeaponSwitchingInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(eWeaponType.RedLaser);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(eWeaponType.GreenNeutron);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(eWeaponType.YellowVulcan);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(eWeaponType.PurplePlasma);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchWeapon(eWeaponType.LaserCannon);
    }

    public void SwitchWeapon(eWeaponType newWeapon)
    {
        if (m_ActiveWeapon == newWeapon) return;
        m_ActiveWeapon = newWeapon;
        AudioManager.Instance?.Play(eSFXId.Pickup, 0.7f);
        OnWeaponChanged?.Invoke(m_ActiveWeapon, m_PlayerStats ? m_PlayerStats.WeaponLevel : 1);
    }

    private void executeWeaponFire(eWeaponType weapon, int level)
    {
        OnShotFiredRhythm?.Invoke();

        switch (weapon)
        {
            case eWeaponType.RedLaser:
            case eWeaponType.LaserCannon:
                fireRedLaser(level);
                break;
            case eWeaponType.GreenNeutron:
                fireGreenNeutron(level);
                break;
            case eWeaponType.YellowVulcan:
                fireYellowVulcan(level);
                break;
            case eWeaponType.PurplePlasma:
                firePurplePlasma(level);
                break;
        }
    }

    // 1. RED LASER (Default): Fiery red laser blasters with white-hot core
    private void fireRedLaser(int level)
    {
        float fireRate = 8.5f + (level * 0.45f);
        m_NextFireTime = Time.time + (1f / fireRate);
        AudioManager.Instance?.Play(eSFXId.Shoot, 0.7f, 1.05f);

        Color brightRed = new Color(1.0f, 0.4f, 0.4f, 1.0f);
        Vector3 scale = new Vector3(1.1f, 1.3f, 1f);

        if (level % 2 == 0)
        {
            spawnCustomBullet(Vector2.up, -0.22f, 0f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
            spawnCustomBullet(Vector2.up, 0.22f, 0f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
        }
        else
        {
            spawnCustomBullet(Vector2.up, 0f, 0f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
        }

        if (level >= 3)
        {
            spawnCustomBullet(Vector2.up, -0.1f, 6f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
            spawnCustomBullet(Vector2.up, 0.1f, -6f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
        }
        if (level >= 5)
        {
            spawnCustomBullet(Vector2.up, -0.35f, 0f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
            spawnCustomBullet(Vector2.up, 0.35f, 0f, 22f, 1, 0, brightRed, scale, m_RedLaserSprite);
        }
    }

    // 2. GREEN NEUTRON: Heavy armor-piercing emerald energy lance
    private void fireGreenNeutron(int level)
    {
        float fireRate = 5.2f + (level * 0.25f);
        m_NextFireTime = Time.time + (1f / fireRate);
        AudioManager.Instance?.Play(eSFXId.Shoot, 0.85f, 0.72f);

        Color emerald = new Color(0.3f, 1.0f, 0.5f, 1.0f);
        Vector3 scale = new Vector3(1.4f, 2.0f, 1f);
        int damage = 2 + (level / 2);
        int pierces = 1 + (level / 2);

        spawnCustomBullet(Vector2.up, 0f, 0f, 25f, damage, pierces, emerald, scale, m_GreenNeutronSprite);
        if (level >= 4)
        {
            spawnCustomBullet(Vector2.up, -0.28f, 0f, 25f, damage, pierces, emerald, scale, m_GreenNeutronSprite);
            spawnCustomBullet(Vector2.up, 0.28f, 0f, 25f, damage, pierces, emerald, scale, m_GreenNeutronSprite);
        }
    }

    // 3. YELLOW VULCAN: High-speed kinetic amber stream with LCG PRNG scatter
    private void fireYellowVulcan(int level)
    {
        float fireRate = 16f + (level * 1.2f);
        m_NextFireTime = Time.time + (1f / fireRate);
        AudioManager.Instance?.Play(eSFXId.Shoot, 0.45f, 1.45f);

        Color amber = new Color(1.0f, 0.85f, 0.2f, 1.0f);
        Vector3 scale = new Vector3(0.85f, 1.0f, 1f);

        // LCG PRNG Scatter (from OutletUzi::server_onFire)
        m_PrngSeed = m_PrngSeed * 214013u + 2531011u;
        float randomSpread = ((float)(m_PrngSeed & 0x7FFF) / 32767f - 0.5f) * 7.5f;

        float lateralOffset = (level % 2 == 0) ? -0.15f : 0.15f;
        spawnCustomBullet(Vector2.up, lateralOffset, randomSpread, 24f, 1, 0, amber, scale, m_YellowVulcanSprite);

        if (level >= 3)
        {
            spawnCustomBullet(Vector2.up, -lateralOffset, -randomSpread, 24f, 1, 0, amber, scale, m_YellowVulcanSprite);
        }
    }

    // 4. PURPLE PLASMA: Multi-angle wide fan-spread violet plasma spheres
    private void firePurplePlasma(int level)
    {
        float fireRate = 5.5f + (level * 0.35f);
        m_NextFireTime = Time.time + (1f / fireRate);
        AudioManager.Instance?.Play(eSFXId.Shoot, 0.75f, 1.25f);

        Color violet = new Color(0.9f, 0.35f, 1.0f, 1.0f);
        Vector3 scale = new Vector3(1.3f, 1.3f, 1f);
        int damage = 2;

        float[] angles = (level >= 5) 
            ? new float[] { -28f, -14f, 0f, 14f, 28f } 
            : ((level >= 3) ? new float[] { -18f, 0f, 18f } : new float[] { -10f, 10f });

        for (int i = 0; i < angles.Length; i++)
        {
            spawnCustomBullet(Vector2.up, 0f, angles[i], 19f, damage, 0, violet, scale, m_PurplePlasmaSprite);
        }
    }

    private void spawnCustomBullet(Vector2 direction, float lateralOffset, float angleOffset, 
                                   float speed, int damage, int pierce, Color color, Vector3 scale, Sprite customSprite = null)
    {
        Vector3 spawnPos = m_Muzzle ? (m_Muzzle.position + new Vector3(lateralOffset, 0, 0)) : (transform.position + new Vector3(lateralOffset, 0, 0));
        var bullet = PoolManager.Instance?.GetBullet(spawnPos, Quaternion.identity);
        if (bullet != null)
        {
            bullet.Configure(speed, damage, pierce, color, scale, customSprite);
            var finalDirection = Quaternion.Euler(0, 0, angleOffset) * direction;
            bullet.Fire(finalDirection);
        }

        OnCustomBulletFired?.Invoke(direction, lateralOffset, angleOffset, speed, damage, pierce, color, scale);
    }

    /// <summary>
    /// Spawns a bullet triggered remotely by a wingman over the network.
    /// Uses local object pool with zero GC and no server physics burden.
    /// </summary>
    public static void SpawnRemoteBullet(Vector3 position, Vector2 direction, float angleOffset, 
                                         float speed, int damage, int pierce, Color color, Vector3 scale, Sprite customSprite = null)
    {
        var bullet = PoolManager.Instance?.GetBullet(position, Quaternion.identity);
        if (bullet != null)
        {
            bullet.Configure(speed, damage, pierce, color, scale, customSprite);
            var finalDirection = Quaternion.Euler(0, 0, angleOffset) * direction;
            bullet.Fire(finalDirection);
        }
    }
}
