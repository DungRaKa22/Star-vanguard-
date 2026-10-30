using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float m_Speed = 18f;
    [SerializeField] private float m_LifeTime = 2f; // seconds
    [SerializeField] private int m_Damage = 1;
    [SerializeField] private int m_RemainingPierces = 0;

    private float m_DeathTime;
    private Vector2 m_Direction;
    private bool m_Active;
    private SpriteRenderer m_SpriteRenderer;
    private Sprite m_DefaultSprite;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer != null) m_DefaultSprite = m_SpriteRenderer.sprite;
    }

    /// <summary>
    /// Configures projectile parameters based on the active weapon archetype.
    /// </summary>
    public void Configure(float speed, int damage, int pierceCount, Color bulletColor, Vector3 scale, Sprite customSprite = null)
    {
        m_Speed = speed;
        m_Damage = damage;
        m_RemainingPierces = pierceCount;
        transform.localScale = scale;
        if (m_SpriteRenderer == null) m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer != null)
        {
            if (customSprite != null) m_SpriteRenderer.sprite = customSprite;
            m_SpriteRenderer.color = bulletColor;
        }
    }

    public void Fire(Vector2 i_Direction)
    {
        m_Direction = i_Direction.normalized;
        m_DeathTime = Time.time + m_LifeTime;
        m_Active = true;
    }
    
    public void OnPoolGet()
    {
        m_Active = false;
        m_Direction = Vector2.zero;
        m_DeathTime = 0f;
        m_RemainingPierces = 0;
        transform.localScale = Vector3.one;
        if (m_SpriteRenderer != null)
        {
            if (m_DefaultSprite != null) m_SpriteRenderer.sprite = m_DefaultSprite;
            m_SpriteRenderer.color = Color.white;
        }
    }
    
    public void OnPoolReturn()
    {
        m_Active = false;
        m_Direction = Vector2.zero;
        m_RemainingPierces = 0;
    }

    void Update()
    {
        if (!m_Active) return;
        
        transform.Translate(Time.deltaTime * m_Speed * m_Direction, Space.World);
        if(Time.time >= m_DeathTime)
        {
            deactivate();
        }
    }

    void OnTriggerEnter2D(Collider2D i_Other)
    {
        // Only hit enemies
        if (i_Other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(i_Other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(m_Damage);
            }
            
            // Handle Armor-Piercing rounds (Neutron Gun / Laser)
            if (m_RemainingPierces > 0)
            {
                m_RemainingPierces--;
            }
            else
            {
                deactivate();
            }
        }
    }

    private void deactivate()
    {
        m_Active = false;
        PoolManager.Instance?.ReturnBullet(this);
    }
}
