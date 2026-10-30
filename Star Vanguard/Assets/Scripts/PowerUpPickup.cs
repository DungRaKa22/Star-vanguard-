using UnityEngine;

public enum ePickupType
{
    WeaponLevelUp,   // Nether Star: increases weapon power level (+1)
    WeaponSwitcher   // Colored Gift Box: switches weapon type (Red, Green, Yellow, Purple)
}

/// <summary>
/// Handles both Nether Star (Firepower Level Up) and 4-Colored Gift Boxes (Weapon Switcher).
/// </summary>
public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private float m_FallSpeed = 2.2f;
    [SerializeField] private ePickupType m_PickupType = ePickupType.WeaponLevelUp;
    [SerializeField] private eWeaponType m_TargetWeapon = eWeaponType.RedLaser;
    
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer m_SpriteRenderer;

    private float m_BobTimer = 0f;

    public ePickupType PickupType => m_PickupType;
    public eWeaponType TargetWeapon => m_TargetWeapon;

    private void Awake()
    {
        if (!m_SpriteRenderer) m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Configures the pickup as a Nether Star (Firepower Level Up)
    /// </summary>
    public void ConfigureAsLevelUp(Sprite netherStarSprite)
    {
        m_PickupType = ePickupType.WeaponLevelUp;
        if (!m_SpriteRenderer) m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer != null && netherStarSprite != null)
        {
            m_SpriteRenderer.sprite = netherStarSprite;
            m_SpriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Configures the pickup as a Weapon Switcher Gift Box (Red, Green, Yellow, or Purple)
    /// </summary>
    public void ConfigureAsWeaponSwitcher(eWeaponType weaponType, Sprite giftBoxSprite)
    {
        m_PickupType = ePickupType.WeaponSwitcher;
        m_TargetWeapon = weaponType;
        if (!m_SpriteRenderer) m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer != null && giftBoxSprite != null)
        {
            m_SpriteRenderer.sprite = giftBoxSprite;
            m_SpriteRenderer.color = Color.white;
        }
    }

    private void Update()
    {
        // Gentle descent
        transform.Translate(m_FallSpeed * Time.deltaTime * Vector2.down, Space.World);

        // Gentle horizontal sway
        m_BobTimer += Time.deltaTime * 2.5f;
        transform.position += new Vector3(Mathf.Sin(m_BobTimer) * 0.005f, 0, 0);

        // Rotate Nether Star gently
        if (m_PickupType == ePickupType.WeaponLevelUp)
        {
            transform.Rotate(0, 0, 45f * Time.deltaTime);
        }

        // Clean up when falling below screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        if (i_Other.CompareTag("Player") || i_Other.GetComponent<PlayerStats>() != null || i_Other.GetComponent<PlayerHitBox>() != null)
        {
            var playerStats = i_Other.GetComponent<PlayerStats>() ?? FindAnyObjectByType<PlayerStats>();
            var playerShooting = i_Other.GetComponent<PlayerShooting>() ?? FindAnyObjectByType<PlayerShooting>();

            if (m_PickupType == ePickupType.WeaponLevelUp)
            {
                if (playerStats) playerStats.AddWeaponLevel(1);
                AudioManager.Instance?.Play(eSFXId.Pickup, 0.9f, 1.25f);
            }
            else if (m_PickupType == ePickupType.WeaponSwitcher)
            {
                if (playerShooting) playerShooting.SwitchWeapon(m_TargetWeapon);
                AudioManager.Instance?.Play(eSFXId.Pickup, 0.9f, 1.0f);
            }

            Destroy(gameObject);
        }
    }
}
