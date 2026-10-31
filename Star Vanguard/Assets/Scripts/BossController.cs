using UnityEngine;
using UnityEngine.Serialization;

public class BossController : MonoBehaviour, IDamageable
{
    private static readonly int sr_Explode = Animator.StringToHash("Explode");
    private static readonly System.Collections.Generic.List<BossController> s_ActiveBosses = new System.Collections.Generic.List<BossController>();
    public static System.Collections.Generic.IReadOnlyList<BossController> ActiveBosses => s_ActiveBosses;

    static BossController()
    {
        SingletonRegistry.Register(ResetStaticData);
    }

    private static void ResetStaticData()
    {
        s_ActiveBosses.Clear();
    }
    [Header("Boss Settings")]
    [SerializeField] private int m_BaseMaxHp = 50; // Boss base HP
    [SerializeField] private int m_ScoreReward = 5000; // Points awarded for killing boss
    [FormerlySerializedAs("m_MinFoodDrops")]
    [SerializeField] private int m_MinCrystalDrops = 10; // Minimum crystal drops
    [FormerlySerializedAs("m_MaxFoodDrops")]
    [SerializeField] private int m_MaxCrystalDrops = 20; // Maximum crystal drops
    [SerializeField] private float m_ExplosionAnimationDuration = 1.5f; // Duration of explosion animation
    
    [Header("Components")]
    [SerializeField] private BossShooting m_BossShooting; // Boss shooting component
    [SerializeField] private Animator m_Animator;
    
    private int m_MaxHp = 50; // Actual max HP after scaling
    private int m_Hp = 50;
    private int m_CoopPlayerCount = 1; // Remembered so bosses spawned later still scale correctly
    private bool m_IsDead = false; // Prevent multiple death calls
    private bool m_IsActive = false; // Track if boss is currently active
    private Coroutine m_ExplosionCoroutine; // Track explosion animation coroutine
    
    // Debug properties for inspector visibility
    public bool IsActive => m_IsActive;
    
    private void OnEnable()
    {
        if (!s_ActiveBosses.Contains(this)) s_ActiveBosses.Add(this);
        initializeBoss();
    }

    private void OnDisable()
    {
        s_ActiveBosses.Remove(this);
    }
    
    /// <summary>
    /// Initialize boss when spawned
    /// </summary>
    public void InitializeBoss()
    {
        initializeBoss();
    }
    
    private void initializeBoss()
    {
        // Calculate scaled HP based on current difficulty (bosses scale more than regular enemies)
        int tier = GameManager.Instance != null ? GameManager.Instance.CurrentDifficultyTier : 1;
        int difficultyBonus = (tier - 1) * 25; // 25 HP per tier
        m_MaxHp = Mathf.RoundToInt((m_BaseMaxHp + difficultyBonus) * (1f + 0.65f * Mathf.Max(0, m_CoopPlayerCount - 1)));
        m_Hp = m_MaxHp;
        m_IsDead = false;
        m_IsActive = true;
        
        // Enable shooting if component exists
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = true;
            m_BossShooting.UpdateHpPercent(1f);
        }
    }
    
    public void TakeDamage(int i_Amount)
    {
        // Don't take damage if already dead or not active
        if (m_IsDead || !m_IsActive) return;
        
        m_Hp -= i_Amount;
        AudioManager.Instance?.Play(eSFXId.BossHit, 0.3f, 0.8f); // Reduced volume for boss hits
        
        if (m_BossShooting != null && m_MaxHp > 0)
        {
            m_BossShooting.UpdateHpPercent((float)m_Hp / m_MaxHp);
        }
        
        if (m_Hp <= 0)
        {
            die();
        }
    }
    
    private void die()
    {
        // Prevent multiple death calls
        if (m_IsDead) return;
        m_IsDead = true;
        m_IsActive = false;
        
        // Disable shooting immediately
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = false;
        }
        
        // Disable collider to prevent collision with player during death animation
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Stop movement by disabling BossMover component
        var bossMover = GetComponent<BossMover>();
        if (bossMover != null)
        {
            bossMover.enabled = false;
        }
        
        // Start explosion sequence - this will handle all the effects synchronously
        m_ExplosionCoroutine = StartCoroutine(explosionSequence());
    }
    
    /// <summary>
    /// Handle the complete explosion sequence with proper timing
    /// </summary>
    private System.Collections.IEnumerator explosionSequence()
    {
        // Force immediate animation start by updating animator immediately
        m_Animator.SetTrigger(sr_Explode);
        m_Animator.Update(0f); // Force animator to process the trigger immediately
        
        // Wait one frame to ensure animation has started
        yield return null;
        
        // All effects happen at the exact moment the explosion animation starts
        AudioManager.Instance?.Play(eSFXId.Explosion, 0.4f, 0.5f); // Reduced from 0.7f
        AudioManager.Instance?.Play(eSFXId.BossHit, 0.5f, 0.6f);   // Reduced from 0.8f
        StarVanguard.CameraShakeController.Shake(0.8f, 0.8f);
        GameManager.Instance?.AddScore(m_ScoreReward);
        dropEnergyCrystals();
        
        // Notify systems
        AudioManager.Instance?.OnBossDefeated();
        WaveDirector.Instance?.OnBossKilled();
        
        // Show boss defeated congratulations message after 0.5 seconds
        StartCoroutine(ShowBossDefeatedMessageDelayed());
        
        // Wait for explosion animation to complete (minus the frame we already waited)
        yield return new WaitForSeconds(m_ExplosionAnimationDuration - Time.deltaTime);
        
        // Clean up
        m_ExplosionCoroutine = null;
        Destroy(gameObject);
    }
    
    private System.Collections.IEnumerator ShowBossDefeatedMessageDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        InterWaveMessageManager.Instance?.ShowBossDefeatedMessage();
    }
    
    private void dropEnergyCrystals()
    {
        // Drop random amount of crystals between min and max
        int count = Random.Range(m_MinCrystalDrops, m_MaxCrystalDrops + 1);
        
        for (int i = 0; i < count; i++)
        {
            // Spread crystal drops around boss position
            Vector3 dropPosition = transform.position + new Vector3(
                Random.Range(-2f, 2f), 
                Random.Range(-1f, 1f), 
                0f
            );
            
            // Use pickup manager to spawn crystal
            PickupManager.Instance?.SpawnCrystal(dropPosition);
        }
    }

    /// <summary>
    /// Scales boss health for Co-op Multiplayer according to REA extracted formula:
    /// HP_coop(N) = HP_base * (1 + 0.65 * (N - 1))
    /// Dormant hook: the single-player build (ADR-001) always runs with N = 1, so nothing calls
    /// this today. Keep it as the extension point for the future co-op pass (backlog T-203).
    /// </summary>
    public void ScaleForCoop(int playerCount)
    {
        m_CoopPlayerCount = Mathf.Max(1, playerCount);

        // If no boss is alive right now, remember the count for the next spawn instead
        if (!m_IsActive) return;

        int tier = GameManager.Instance != null ? GameManager.Instance.CurrentDifficultyTier : 1;
        int difficultyBonus = (tier - 1) * 25;
        m_MaxHp = Mathf.RoundToInt((m_BaseMaxHp + difficultyBonus) * (1f + 0.65f * (m_CoopPlayerCount - 1)));
        m_Hp = m_MaxHp;

        if (m_BossShooting != null && m_MaxHp > 0)
        {
            m_BossShooting.UpdateHpPercent(1f);
        }
    }
    
    /// <summary>
    /// Deactivate boss (called when boss moves off screen or game resets)
    /// </summary>
    public void DeactivateBoss()
    {
        m_IsActive = false;
        
        // Disable shooting
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = false;
        }
        
        // Stop explosion coroutine if it's running (for game resets)
        if (m_ExplosionCoroutine != null)
        {
            StopCoroutine(m_ExplosionCoroutine);
            m_ExplosionCoroutine = null;
        }
        
        // Destroy boss object
        Destroy(gameObject);
    }
}
