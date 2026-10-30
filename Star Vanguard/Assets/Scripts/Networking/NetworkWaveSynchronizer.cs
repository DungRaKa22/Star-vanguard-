using UnityEngine;

#if MIRROR
using Mirror;

/// <summary>
/// Server-authoritative synchronizer for Waves and Boss encounters in Star Vanguard Co-op.
/// Guarantees that all connected clients receive identical wave seeds and boss HP scaling.
/// </summary>
public class NetworkWaveSynchronizer : NetworkBehaviour
{
    [Header("Wave Synchronization")]
    [SyncVar(hook = nameof(OnWaveNumberChanged))]
    private int m_SyncedWaveNumber = 1;

    [SyncVar]
    private int m_SyncedWaveSeed = 1337;

    public int SyncedWaveNumber => m_SyncedWaveNumber;
    public int SyncedWaveSeed => m_SyncedWaveSeed;

    public override void OnStartServer()
    {
        base.OnStartServer();
        m_SyncedWaveSeed = System.Environment.TickCount;
        Random.InitState(m_SyncedWaveSeed);
    }

    [Server]
    public void ServerSetWave(int waveNumber)
    {
        m_SyncedWaveNumber = waveNumber;
        m_SyncedWaveSeed = System.Environment.TickCount + waveNumber;

        // Auto-scale boss if boss wave
        if (waveNumber % 7 == 0)
        {
            int players = NetworkServer.connections.Count;
            var boss = FindAnyObjectByType<BossController>();
            if (boss != null)
            {
                boss.ScaleForCoop(players);
            }
        }
    }

    private void OnWaveNumberChanged(int oldWave, int newWave)
    {
        Debug.Log($"[Star Vanguard Net] Synced wave updated: Wave {newWave} (Seed: {m_SyncedWaveSeed})");
    }
}
#else
/// <summary>
/// Single-player fallback shim.
/// </summary>
public class NetworkWaveSynchronizer : MonoBehaviour
{
    public int SyncedWaveNumber => 1;
}
#endif
