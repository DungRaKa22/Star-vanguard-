using UnityEngine;
using UnityEngine.Serialization;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Enemy Pooling")]
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private int m_EnemyPoolSize = 30; // Increased from 20 to handle larger formations
    
    [Header("Energy Crystal Pooling")]
    [FormerlySerializedAs("m_FoodPrefab")]
    [SerializeField] private EnergyCrystal m_CrystalPrefab;
    [FormerlySerializedAs("m_FoodPoolSize")]
    [SerializeField] private int m_CrystalPoolSize = 50; // Crystals spawn very frequently
    
    [Header("Bullet Pooling")]
    [SerializeField] private Bullet m_BulletPrefab;
    [SerializeField] private int m_BulletPoolSize = 32; // Bullets fire very frequently
    
    [Header("Bomb Pooling")]
    [SerializeField] private Bomb m_BombPrefab;
    [SerializeField] private int m_BombPoolSize = 10;
    
    [Header("Enemy Bullet Pooling")]
    [SerializeField] private EnemyBullet m_EnemyBulletPrefab;
    [SerializeField] private int m_EnemyBulletPoolSize = 20; // Enemies shoot less frequently than player
    
    [Header("HUD Icon Pooling")]
    [SerializeField] private HUDIcon m_HUDIconPrefab;
    [SerializeField] private int m_HUDIconPoolSize = 20; // Max lives + bombs that could be displayed
    
    private ObjectPool<EnemyController> m_EnemyPool;
    private ObjectPool<EnergyCrystal> m_CrystalPool;
    private ObjectPool<Bullet> m_BulletPool;
    private ObjectPool<Bomb> m_BombPool;
    private ObjectPool<EnemyBullet> m_EnemyBulletPool;
    private ObjectPool<HUDIcon> m_HUDIconPool;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            InitializePools();
        }
    }

    public void InitializePools()
    {
        if (m_EnemyPool != null) return; // already initialized

        // Create pools
        m_EnemyPool = new ObjectPool<EnemyController>(m_EnemyPrefab, m_EnemyPoolSize, transform);
        m_CrystalPool = new ObjectPool<EnergyCrystal>(m_CrystalPrefab, m_CrystalPoolSize, transform);
        m_BulletPool = new ObjectPool<Bullet>(m_BulletPrefab, m_BulletPoolSize, transform);
        m_BombPool = new ObjectPool<Bomb>(m_BombPrefab, m_BombPoolSize, transform);
        m_EnemyBulletPool = new ObjectPool<EnemyBullet>(m_EnemyBulletPrefab, m_EnemyBulletPoolSize, transform);
        
        // Create HUD Icon pool only if prefab is assigned
        if (m_HUDIconPrefab != null)
        {
            m_HUDIconPool = new ObjectPool<HUDIcon>(m_HUDIconPrefab, m_HUDIconPoolSize, transform);
        }
    }
    
    private void Start()
    {
        InitializePools();
        
        // Refresh HUD display now that pools are ready
        if (UIManager.HasInstance)
        {
            UIManager.Instance.RefreshHUDDisplay();
        }
    }
    
    public EnemyController GetEnemy(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        if (m_EnemyPool.PoolSize == 0)
        {
            Debug.LogWarning($"PoolManager: Enemy pool is empty! Created: {m_EnemyPool.TotalCreated}, Available: {m_EnemyPool.PoolSize}");
        }
        
        var enemy = m_EnemyPool.Get(i_Position, i_Rotation);
        if (enemy == null)
        {
            Debug.LogError("PoolManager: Failed to get enemy from pool - returned null!");
        }
        
        return enemy;
    }
    
    public void ReturnEnemy(EnemyController i_Enemy)
    {
        m_EnemyPool.Return(i_Enemy);
    }
    
    public EnergyCrystal GetCrystal(Vector3 i_Position)
    {
        return m_CrystalPool.Get(i_Position);
    }
    
    public void ReturnCrystal(EnergyCrystal i_Crystal)
    {
        m_CrystalPool.Return(i_Crystal);
    }

    // Backwards-compatible aliases
    public EnergyCrystal GetFood(Vector3 i_Position) => GetCrystal(i_Position);
    public void ReturnFood(EnergyCrystal i_Food) => ReturnCrystal(i_Food);
    
    public Bullet GetBullet(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        return m_BulletPool.Get(i_Position, i_Rotation);
    }
    
    public void ReturnBullet(Bullet i_Bullet)
    {
        m_BulletPool.Return(i_Bullet);
    }
    
    public Bomb GetBomb(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        return m_BombPool.Get(i_Position, i_Rotation);
    }
    
    public void ReturnBomb(Bomb i_Bomb)
    {
        m_BombPool.Return(i_Bomb);
    }
    
    public EnemyBullet GetEnemyBullet(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        return m_EnemyBulletPool.Get(i_Position, i_Rotation);
    }
    
    public void ReturnEnemyBullet(EnemyBullet i_EnemyBullet)
    {
        m_EnemyBulletPool.Return(i_EnemyBullet);
    }

    // Active object registries (Zero-GC queries for gameplay and bombs)
    public System.Collections.Generic.IReadOnlyList<EnemyController> ActiveEnemies => m_EnemyPool != null ? m_EnemyPool.ActiveObjects : null;
    public System.Collections.Generic.IReadOnlyList<EnemyBullet> ActiveEnemyBullets => m_EnemyBulletPool != null ? m_EnemyBulletPool.ActiveObjects : null;
    public System.Collections.Generic.IReadOnlyList<Bullet> ActiveBullets => m_BulletPool != null ? m_BulletPool.ActiveObjects : null;
    public System.Collections.Generic.IReadOnlyList<Bomb> ActiveBombs => m_BombPool != null ? m_BombPool.ActiveObjects : null;
    public System.Collections.Generic.IReadOnlyList<EnergyCrystal> ActiveCrystals => m_CrystalPool != null ? m_CrystalPool.ActiveObjects : null;
    
    public HUDIcon GetHUDIcon(Vector3 i_Position)
    {
        if (m_HUDIconPool == null)
        {
            return null;
        }
        return m_HUDIconPool.Get(i_Position);
    }
    
    public void ReturnHUDIcon(HUDIcon i_HUDIcon)
    {
        if (m_HUDIconPool == null || i_HUDIcon == null)
        {
            return;
        }
        m_HUDIconPool.Return(i_HUDIcon);
    }
    
    /// <summary>
    /// Check if HUD Icon pool is ready for use
    /// </summary>
    public bool IsHUDIconPoolReady()
    {
        return m_HUDIconPool != null;
    }
    
    // Method to check if we have enough enemies available for a formation
    public bool HasEnoughEnemies(int i_RequiredCount)
    {
        if (m_EnemyPool == null) return false;

        bool hasEnough = m_EnemyPool.PoolSize >= i_RequiredCount;
        if (!hasEnough)
        {
            Debug.LogWarning($"PoolManager: Not enough enemies available! Required: {i_RequiredCount}, Available: {m_EnemyPool.PoolSize}");
        }
        return hasEnough;
    }
    
    public int GetAvailableEnemyCount()
    {
        return m_EnemyPool != null ? m_EnemyPool.PoolSize : 0;
    }
    
    /// <summary>
    /// Reset all pools by returning all active objects - called when restarting game
    /// </summary>
    public void ResetAllPools()
    {
        // GameManager.Awake can run before PoolManager.Awake, so the pools may not exist yet
        if (m_EnemyPool == null || m_CrystalPool == null || m_BulletPool == null ||
            m_BombPool == null || m_EnemyBulletPool == null)
        {
            return;
        }

        resetEnemyPool();
        resetFoodPool();
        resetBulletPool();
        resetBombPool();
        resetEnemyBulletPool();
        // Note: HUD icons are NOT reset here as they need to persist during game state changes
        // They are managed separately by UIManager based on game state
    }
    
    /// <summary>
    /// Reset only HUD icon pool - called separately when needed
    /// </summary>
    public void ResetHUDIconPool()
    {
        resetHUDIconPool();
    }

    private void resetBombPool()
    {
        m_BombPool?.ReturnAllActive();
    }

    private void resetBulletPool()
    {
        m_BulletPool?.ReturnAllActive();
    }

    private void resetFoodPool()
    {
        m_CrystalPool?.ReturnAllActive();
    }

    private void resetEnemyPool()
    {
        m_EnemyPool?.ReturnAllActive();
    }

    private void resetEnemyBulletPool()
    {
        m_EnemyBulletPool?.ReturnAllActive();
    }
    
    private void resetHUDIconPool()
    {
        m_HUDIconPool?.ReturnAllActive();
    }
}
