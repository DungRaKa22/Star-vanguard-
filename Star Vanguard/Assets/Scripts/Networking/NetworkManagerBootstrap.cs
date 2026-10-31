using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.Net
{
    /// <summary>
    /// Bootstrap controller for Unity Netcode for GameObjects (NGO) session management.
    /// Manages Host/Client lifecycle, UnityTransport configuration, and synchronized scene transitions.
    /// </summary>
    public class NetworkManagerBootstrap : MonoBehaviour
    {
        public static NetworkManagerBootstrap Instance { get; private set; }

        public const ushort k_DefaultPort = 7777;
        public const string k_DefaultAddress = "127.0.0.1";

        public event Action<string> OnStatusChanged;
        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;

        private UnityTransport m_Transport;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureNetworkManager();
        }

        private void EnsureNetworkManager()
        {
            NetworkManager netManager = GetComponent<NetworkManager>();
            if (netManager == null)
            {
                netManager = gameObject.AddComponent<NetworkManager>();
            }

            m_Transport = GetComponent<UnityTransport>();
            if (m_Transport == null)
            {
                m_Transport = gameObject.AddComponent<UnityTransport>();
            }

            netManager.NetworkConfig.NetworkTransport = m_Transport;
            netManager.NetworkConfig.EnableSceneManagement = true;
        }

        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool StartHostSession(ushort port = k_DefaultPort)
        {
            if (NetworkManager.Singleton == null) EnsureNetworkManager();

            m_Transport.SetConnectionData(k_DefaultAddress, port);
            bool success = NetworkManager.Singleton.StartHost();

            if (success)
            {
                OnStatusChanged?.Invoke($"Phòng Host đã tạo trên Port {port}. Đang chờ Client kết nối...");
                Debug.Log($"[STARVANGUARD-NET] Host session started on port {port}.");
            }
            else
            {
                OnStatusChanged?.Invoke("Không thể tạo phòng Host. Vui lòng thử lại!");
                Debug.LogError("[STARVANGUARD-NET] Failed to start Host session.");
            }

            return success;
        }

        public bool StartClientSession(string hostAddress, ushort port = k_DefaultPort)
        {
            if (NetworkManager.Singleton == null) EnsureNetworkManager();

            if (string.IsNullOrWhiteSpace(hostAddress))
            {
                hostAddress = k_DefaultAddress;
            }

            m_Transport.SetConnectionData(hostAddress.Trim(), port);
            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                OnStatusChanged?.Invoke($"Đang kết nối tới Host ({hostAddress}:{port})...");
                Debug.Log($"[STARVANGUARD-NET] Client connecting to {hostAddress}:{port}.");
            }
            else
            {
                OnStatusChanged?.Invoke("Không thể kết nối tới Host!");
                Debug.LogError($"[STARVANGUARD-NET] Failed to start Client session to {hostAddress}:{port}.");
            }

            return success;
        }

        public void DisconnectSession()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
                OnStatusChanged?.Invoke("Đã ngắt kết nối phiên mạng.");
                Debug.Log("[STARVANGUARD-NET] Session shut down.");
            }
        }

        public void LoadSynchronizedMission()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Debug.Log("[STARVANGUARD-NET] Host loading synchronized GameplayScene...");
                NetworkManager.Singleton.SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"[STARVANGUARD-NET] Client connected with ID: {clientId}");
            OnPlayerJoined?.Invoke(clientId);

            if (NetworkManager.Singleton.IsServer)
            {
                int totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
                OnStatusChanged?.Invoke($"Đã kết nối! Người chơi trong phòng: {totalPlayers}/2");

                if (totalPlayers >= 2)
                {
                    OnStatusChanged?.Invoke("Đã đủ 2 người chơi! Đang bắt đầu chiến dịch...");
                    Invoke(nameof(LoadSynchronizedMission), 1.0f);
                }
            }
            else
            {
                OnStatusChanged?.Invoke("Kết nối tới Host thành công! Đang vào trận...");
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.LogWarning($"[STARVANGUARD-NET] Client disconnected: {clientId}");
            OnPlayerLeft?.Invoke(clientId);

            if (!NetworkManager.Singleton.IsServer)
            {
                OnStatusChanged?.Invoke("Host đã ngắt kết nối. Đang quay về Menu...");
                DisconnectSession();

                if (SceneManager.GetActiveScene().name != "MainMenuScene")
                {
                    SceneManager.LoadScene("MainMenuScene");
                }
            }
            else
            {
                OnStatusChanged?.Invoke("Client đã rời khỏi phòng.");
            }
        }
    }
}
