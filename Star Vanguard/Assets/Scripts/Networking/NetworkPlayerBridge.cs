using Unity.Netcode;
using UnityEngine;

namespace StarVanguard.Net
{
    /// <summary>
    /// Bridges local Player ship components with Unity Netcode for GameObjects.
    /// Handles owner-authoritative position replication, remote input suppression,
    /// and visual projectile synchronization without allocating heavy NetworkObjects per bullet.
    /// </summary>
    public class NetworkPlayerBridge : NetworkBehaviour
    {
        [Header("Local Components to Disable for Non-Owners")]
        [SerializeField] private PlayerController m_PlayerController;
        [SerializeField] private PlayerShooting m_PlayerShooting;
        [SerializeField] private PlayerHitBox m_PlayerHitBox;

        // Owner-authoritative position variable (0ms local latency)
        private readonly NetworkVariable<Vector2> m_NetPosition = new NetworkVariable<Vector2>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<int> m_NetWeaponLevel = new NetworkVariable<int>(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (m_PlayerController == null) m_PlayerController = GetComponent<PlayerController>();
            if (m_PlayerShooting == null) m_PlayerShooting = GetComponent<PlayerShooting>();
            if (m_PlayerHitBox == null) m_PlayerHitBox = GetComponent<PlayerHitBox>();

            if (!IsOwner)
            {
                // Disable input and firing controllers for teammate's ship on this machine
                if (m_PlayerController != null) m_PlayerController.enabled = false;
                if (m_PlayerShooting != null) m_PlayerShooting.enabled = false;
            }
            else
            {
                // Assign distinct initial spawn coordinates: Host on left (-2.5), Client on right (+2.5)
                float initialX = IsServer ? -2.5f : 2.5f;
                transform.position = new Vector3(initialX, -4.0f, 0f);
                m_NetPosition.Value = transform.position;
            }
        }

        private void Update()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                m_NetPosition.Value = transform.position;
            }
            else
            {
                // Smoothly interpolate teammate's position on local client
                transform.position = Vector3.Lerp(transform.position, m_NetPosition.Value, Time.deltaTime * 25f);
            }
        }

        /// <summary>
        /// Broadcasts bullet visual firing to peer players without spawning NetworkObjects.
        /// </summary>
        [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Unreliable)]
        public void FireVisualBulletRpc(Vector2 firePos, int weaponType, int weaponLevel)
        {
            if (PoolManager.Instance == null) return;

            // Spawns visual bullet from local PoolManager
            PoolManager.Instance.GetBullet(firePos, Quaternion.identity);
        }
    }
}
