using UnityEngine;

/// <summary>
/// Multi-phase bullet hell shooter for Bosses in Star Vanguard.
/// Implements REA-extracted damage stage attack patterns:
/// - Stage 1 (100% -> 65% HP): Directed single laser burst.
/// - Stage 2 (65% -> 30% HP): 3-way spread lasers (-20 deg, 0 deg, +20 deg).
/// - Stage 3 (< 30% HP / Enraged): 12-way rotating spiral bullet barrage with doubled fire rate.
/// </summary>
public class BossShooting : MonoBehaviour
{
    [Header("Boss Shooting Settings")]
    [SerializeField] private float m_MinShootInterval = 1.5f;
    [SerializeField] private float m_MaxShootInterval = 3.0f;
    [SerializeField] private int m_BurstCount = 1;
    [SerializeField] private float m_BurstDelay = 0.2f;

    [Header("Multi-Phase Bullet Hell (REA Spec)")]
    [SerializeField] private int m_CurrentStage = 1;
    [SerializeField] private float m_SpiralAngleStep = 15f;

    private float m_NextShootTime;
    private bool m_IsActive = true;
    private float m_CurrentSpiralOffset = 0f;
    private Transform m_TargetPlayer;

    private void Awake()
    {
        // Cache target player if available
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            m_TargetPlayer = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        m_IsActive = true;
        m_CurrentStage = 1;
        m_CurrentSpiralOffset = 0f;
        scheduleNextShot();
    }

    private void OnDisable()
    {
        m_IsActive = false;
    }

    /// <summary>
    /// Update HP percentage to transition between attack stages according to REA specifications.
    /// Stage 1: > 65% HP
    /// Stage 2: 30% - 65% HP
    /// Stage 3: < 30% HP (Enraged)
    /// </summary>
    public void UpdateHpPercent(float hpPercent)
    {
        if (hpPercent > 0.65f)
        {
            m_CurrentStage = 1;
            m_MinShootInterval = 1.5f;
            m_MaxShootInterval = 3.0f;
            m_BurstCount = 1;
        }
        else if (hpPercent > 0.30f)
        {
            m_CurrentStage = 2;
            m_MinShootInterval = 1.0f;
            m_MaxShootInterval = 2.0f;
            m_BurstCount = 2;
        }
        else
        {
            m_CurrentStage = 3;
            m_MinShootInterval = 0.6f;
            m_MaxShootInterval = 1.2f;
            m_BurstCount = 3;
        }
    }

    private void Update()
    {
        if (!m_IsActive || GameManager.Instance?.GameState != eGameState.Playing)
            return;

        if (Time.time >= m_NextShootTime)
        {
            StartCoroutine(shootBurst());
            scheduleNextShot();
        }
    }

    private System.Collections.IEnumerator shootBurst()
    {
        for (int i = 0; i < m_BurstCount; i++)
        {
            switch (m_CurrentStage)
            {
                case 1:
                    shootStage1Aimed();
                    break;
                case 2:
                    shootStage2Spread();
                    break;
                case 3:
                    shootStage3SpiralBarrage();
                    break;
            }

            AudioManager.Instance?.Play(eSFXId.EnemyShoot, 0.7f, Random.Range(0.85f, 1.15f));

            if (i < m_BurstCount - 1)
            {
                yield return new WaitForSeconds(m_BurstDelay);
            }
        }
    }

    /// <summary>
    /// Stage 1: Single directed projectile towards player or straight down with slight variance.
    /// </summary>
    private void shootStage1Aimed()
    {
        Vector2 direction = Vector2.down;
        if (m_TargetPlayer != null && m_TargetPlayer.gameObject.activeInHierarchy)
        {
            Vector2 toPlayer = (m_TargetPlayer.position - transform.position).normalized;
            // Limit angle to downward hemisphere
            if (toPlayer.y < -0.2f)
            {
                direction = toPlayer;
            }
        }

        float spreadAngle = Random.Range(-8f, 8f);
        direction = Quaternion.Euler(0, 0, spreadAngle) * direction;

        fireSingleBullet(direction);
    }

    /// <summary>
    /// Stage 2: 3-way spread lasers (-20 deg, 0 deg, +20 deg).
    /// </summary>
    private void shootStage2Spread()
    {
        float[] spreadAngles = { -20f, 0f, 20f };
        for (int i = 0; i < spreadAngles.Length; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, spreadAngles[i]) * Vector2.down;
            fireSingleBullet(dir);
        }
    }

    /// <summary>
    /// Stage 3: Enraged 12-way rotating spiral barrage covering 360 degrees.
    /// </summary>
    private void shootStage3SpiralBarrage()
    {
        const int rays = 12;
        const float anglePerRay = 360f / rays;

        m_CurrentSpiralOffset += m_SpiralAngleStep;
        if (m_CurrentSpiralOffset >= 360f) m_CurrentSpiralOffset -= 360f;

        for (int i = 0; i < rays; i++)
        {
            float angle = m_CurrentSpiralOffset + i * anglePerRay;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            fireSingleBullet(dir);
        }
    }

    private void fireSingleBullet(Vector2 direction)
    {
        var projectile = PoolManager.Instance?.GetEnemyBullet(transform.position);
        if (projectile != null)
        {
            projectile.Fire(direction);
        }
    }

    private void scheduleNextShot()
    {
        float interval = Random.Range(m_MinShootInterval, m_MaxShootInterval);
        m_NextShootTime = Time.time + interval;
    }
}
