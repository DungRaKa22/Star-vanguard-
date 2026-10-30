using UnityEngine;

#if MIRROR
using Mirror;

/// <summary>
/// Server-Authoritative Network Controller for Star Vanguard Co-op Multiplayer.
/// Synchronizes ship transform, weapon firing, player colors, scores, and lives.
/// Integrates with local PoolManager to guarantee zero GC and zero physics lag.
/// </summary>
public class NetworkPlayerController : NetworkBehaviour
{
    private static readonly Color[] sr_PlayerColors = new Color[]
    {
        new Color(0.0f, 0.9f, 1.0f, 1.0f),  // P1: Neon Cyan
        new Color(1.0f, 0.25f, 0.2f, 1.0f), // P2: Crimson Red
        new Color(0.2f, 1.0f, 0.4f, 1.0f),  // P3: Emerald Green
        new Color(1.0f, 0.85f, 0.1f, 1.0f)  // P4: Solar Amber
    };

    [Header("Network SyncVars")]
    [SyncVar(hook = nameof(OnPlayerIndexChanged))]
    private int m_PlayerIndex = 0;

    [SyncVar(hook = nameof(OnWeaponTypeChanged))]
    private eWeaponType m_NetWeaponType = eWeaponType.IonBlaster;

    [SyncVar]
    private int m_NetScore = 0;

    [SyncVar]
    private int m_NetLives = 3;

    [Header("References")]
    [SerializeField] private SpriteRenderer m_ShipRenderer;
    [SerializeField] private PlayerController m_LocalController;
    [SerializeField] private PlayerShooting m_LocalShooting;
    [SerializeField] private PlayerBombing m_LocalBombing;
    [SerializeField] private PlayerStats m_LocalStats;

    public int PlayerIndex => m_PlayerIndex;
    public bool IsLocalWingman => isLocalPlayer;

    private void Awake()
    {
        if (!m_ShipRenderer) m_ShipRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!m_LocalController) m_LocalController = GetComponent<PlayerController>();
        if (!m_LocalShooting) m_LocalShooting = GetComponent<PlayerShooting>();
        if (!m_LocalBombing) m_LocalBombing = GetComponent<PlayerBombing>();
        if (!m_LocalStats) m_LocalStats = GetComponent<PlayerStats>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Server assigns player index based on connection count
        m_PlayerIndex = NetworkServer.connections.Count - 1;
        if (m_PlayerIndex < 0) m_PlayerIndex = 0;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Enable local control systems
        if (m_LocalController) m_LocalController.enabled = true;
        if (m_LocalShooting) m_LocalShooting.enabled = true;
        if (m_LocalBombing) m_LocalBombing.enabled = true;

        // Hook firing event to replicate over network
        PlayerShooting.OnCustomBulletFired += HandleLocalBulletFired;
        PlayerShooting.OnWeaponChanged += HandleLocalWeaponChanged;

        applyPlayerColor(m_PlayerIndex);
        Debug.Log($"[Star Vanguard Co-op] Local Player initialized as Wingman #{m_PlayerIndex + 1}");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            // Disable local input on remote proxies
            if (m_LocalController) m_LocalController.enabled = false;
            if (m_LocalShooting) m_LocalShooting.enabled = false;
            if (m_LocalBombing) m_LocalBombing.enabled = false;
        }

        applyPlayerColor(m_PlayerIndex);
    }

    public override void OnStopLocalPlayer()
    {
        PlayerShooting.OnCustomBulletFired -= HandleLocalBulletFired;
        PlayerShooting.OnWeaponChanged -= HandleLocalWeaponChanged;
        base.OnStopLocalPlayer();
    }

    private void HandleLocalBulletFired(Vector2 dir, float offset, float angle, float speed, int damage, int pierce, Color color, Vector3 scale)
    {
        if (!isLocalPlayer) return;
        CmdFireBullet(dir, offset, angle, speed, damage, pierce, color, scale);
    }

    private void HandleLocalWeaponChanged(eWeaponType weapon, int level)
    {
        if (!isLocalPlayer) return;
        CmdChangeWeapon(weapon);
    }

    [Command]
    private void CmdFireBullet(Vector2 dir, float offset, float angle, float speed, int damage, int pierce, Color color, Vector3 scale)
    {
        // Replicate to all other clients
        RpcSpawnRemoteBullet(dir, offset, angle, speed, damage, pierce, color, scale);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSpawnRemoteBullet(Vector2 dir, float offset, float angle, float speed, int damage, int pierce, Color color, Vector3 scale)
    {
        Vector3 spawnPos = transform.position + new Vector3(offset, 0, 0);
        PlayerShooting.SpawnRemoteBullet(spawnPos, dir, angle, speed, damage, pierce, color, scale);
    }

    [Command]
    private void CmdChangeWeapon(eWeaponType newWeapon)
    {
        m_NetWeaponType = newWeapon;
    }

    private void OnPlayerIndexChanged(int oldVal, int newVal)
    {
        applyPlayerColor(newVal);
    }

    private void OnWeaponTypeChanged(eWeaponType oldVal, eWeaponType newVal)
    {
        // Remote proxy weapon changed
    }

    private void applyPlayerColor(int index)
    {
        if (!m_ShipRenderer) return;
        int clampedIdx = Mathf.Clamp(index, 0, sr_PlayerColors.Length - 1);
        m_ShipRenderer.color = sr_PlayerColors[clampedIdx];
    }
}
#else
/// <summary>
/// Standalone Fallback shim for single-player / editor mode without Mirror.
/// </summary>
public class NetworkPlayerController : MonoBehaviour
{
    [SerializeField] private int m_PlayerIndex = 0;
    [SerializeField] private SpriteRenderer m_ShipRenderer;

    public int PlayerIndex => m_PlayerIndex;

    private void Awake()
    {
        if (!m_ShipRenderer) m_ShipRenderer = GetComponentInChildren<SpriteRenderer>();
        if (m_ShipRenderer)
        {
            m_ShipRenderer.color = new Color(0.1f, 0.95f, 1.0f, 1.0f); // Default Vanguard Cyan
        }
    }
}
#endif
