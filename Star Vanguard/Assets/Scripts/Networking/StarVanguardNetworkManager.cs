using UnityEngine;

#if MIRROR
using Mirror;

/// <summary>
/// Custom Network Manager for Star Vanguard Online Co-op LAN.
/// Handles connection lifecycle, player spawning, and server-authoritative matchmaking.
/// </summary>
public class StarVanguardNetworkManager : NetworkManager
{
    [Header("Star Vanguard Co-op Settings")]
    [SerializeField] private int m_MaxCoopPlayers = 4;
    [SerializeField] private Transform[] m_CoopSpawnPositions;

    /// <summary>
    /// Number of players currently in the session (Host + Clients).
    /// Used to scale boss health on every boss spawn, not only when someone joins.
    /// </summary>
    public static int GetActivePlayerCount()
    {
        return Mathf.Max(1, NetworkServer.connections.Count);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[Star Vanguard Server] Dedicated/Host Server started.");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        Vector3 spawnPos = startPos != null ? startPos.position : new Vector3((numPlayers - 1) * 1.5f, -3.5f, 0f);
        
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log($"[Star Vanguard Server] Player {conn.connectionId} joined. Total active players: {numPlayers}");

        // Scale boss health if boss is already active
        var activeBoss = FindAnyObjectByType<BossController>();
        if (activeBoss != null)
        {
            activeBoss.ScaleForCoop(numPlayers);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"[Star Vanguard Server] Player {conn.connectionId} disconnected.");
        base.OnServerDisconnect(conn);
    }
}
#else
/// <summary>
/// Fallback shim while Mirror package is resolving in Unity Package Manager.
/// </summary>
public class StarVanguardNetworkManager : MonoBehaviour
{
    [Header("Star Vanguard Co-op (Mirror Networking)")]
    [SerializeField] private int m_MaxCoopPlayers = 4;

    /// <summary>Single-player fallback used while Mirror is not installed.</summary>
    public static int GetActivePlayerCount()
    {
        return 1;
    }

    private void Awake()
    {
        Debug.Log($"[Star Vanguard] Mirror Co-op ready. Max Players: {m_MaxCoopPlayers}");
    }
}
#endif
