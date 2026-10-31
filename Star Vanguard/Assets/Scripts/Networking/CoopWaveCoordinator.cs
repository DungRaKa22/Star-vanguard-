using Unity.Netcode;
using UnityEngine;

namespace StarVanguard.Net
{
    /// <summary>
    /// Coordinates deterministic wave generation and Co-op boss scaling across peer clients.
    /// Uses shared random seeds so enemy formation trajectories and dive timings match 100%
    /// without sending network updates for hundreds of individual enemies.
    /// </summary>
    public class CoopWaveCoordinator : NetworkBehaviour
    {
        public static CoopWaveCoordinator Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            // If 2 or more players are connected, scale boss HP accordingly
            if (IsServer && NetworkManager.Singleton != null)
            {
                int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
                if (playerCount >= 2)
                {
                    Debug.Log($"[STARVANGUARD-NET] Scaling Boss HP for Co-op ({playerCount} players)...");
                    for (int i = 0; i < BossController.ActiveBosses.Count; i++)
                    {
                        BossController.ActiveBosses[i].ScaleForCoop(playerCount);
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnDestroy();
        }

        /// <summary>
        /// Server generates a seed and broadcasts to all clients before each wave starts.
        /// </summary>
        public void InitiateSynchronizedWave(int waveIndex)
        {
            if (!IsServer) return;

            int waveSeed = Random.Range(1, int.MaxValue);
            StartSynchronizedWaveRpc(waveIndex, waveSeed);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StartSynchronizedWaveRpc(int waveIndex, int waveSeed)
        {
            Debug.Log($"[STARVANGUARD-NET] Starting synchronized Wave {waveIndex} with Shared Seed: {waveSeed}");
            Random.InitState(waveSeed);
        }

        /// <summary>
        /// Checks whether any active player is still alive before allowing Game Over.
        /// </summary>
        public bool HasLivingPlayers()
        {
            return PlayerController.ActivePlayers.Count > 0;
        }
    }
}
