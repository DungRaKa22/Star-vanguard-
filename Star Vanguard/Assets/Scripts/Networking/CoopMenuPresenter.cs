using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarVanguard.Net
{
    /// <summary>
    /// Presenter bridging the uGUI LAN Co-op panel with NetworkManagerBootstrap.
    /// Keeps UI presentation decoupled from netcode transport implementation.
    /// </summary>
    public class CoopMenuPresenter : MonoBehaviour
    {
        [Header("UI Element Bindings")]
        [SerializeField] private TMP_InputField m_IpInputField;
        [SerializeField] private Button m_HostButton;
        [SerializeField] private Button m_JoinButton;
        [SerializeField] private Button m_BackButton;
        [SerializeField] private TextMeshProUGUI m_StatusText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoRegisterHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryAttach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttach();
        }

        private static void TryAttach()
        {
            if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                GameObject coopObj = GameObject.Find("CoopModalPanel");
                if (coopObj != null && coopObj.GetComponent<CoopMenuPresenter>() == null)
                {
                    coopObj.AddComponent<CoopMenuPresenter>();
                }
            }
        }

        private void Awake()
        {
            ResolveUIBindings();
        }

        private void ResolveUIBindings()
        {
            if (m_HostButton == null) m_HostButton = transform.Find("Card/Btn_Host")?.GetComponent<Button>();
            if (m_JoinButton == null) m_JoinButton = transform.Find("Card/Btn_Join")?.GetComponent<Button>();
            if (m_BackButton == null) m_BackButton = transform.Find("Card/Btn_Back")?.GetComponent<Button>();
            if (m_IpInputField == null) m_IpInputField = transform.Find("Card/IpContainer")?.GetComponentInChildren<TMP_InputField>();
            if (m_StatusText == null) m_StatusText = transform.Find("Card/StatusBox/Txt_Status")?.GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            ResolveUIBindings();

            if (m_IpInputField != null && string.IsNullOrEmpty(m_IpInputField.text))
            {
                m_IpInputField.text = NetworkManagerBootstrap.k_DefaultAddress;
            }

            if (m_HostButton != null) m_HostButton.onClick.AddListener(OnHostClicked);
            if (m_JoinButton != null) m_JoinButton.onClick.AddListener(OnJoinClicked);
            if (m_BackButton != null) m_BackButton.onClick.AddListener(OnBackClicked);

            if (NetworkManagerBootstrap.Instance != null)
            {
                NetworkManagerBootstrap.Instance.OnStatusChanged += UpdateStatus;
            }

            UpdateStatus("Sẵn sàng tạo phòng Host hoặc kết nối tới địa chỉ IP...");
        }

        private void OnDestroy()
        {
            if (m_HostButton != null) m_HostButton.onClick.RemoveListener(OnHostClicked);
            if (m_JoinButton != null) m_JoinButton.onClick.RemoveListener(OnJoinClicked);
            if (m_BackButton != null) m_BackButton.onClick.RemoveListener(OnBackClicked);

            if (NetworkManagerBootstrap.Instance != null)
            {
                NetworkManagerBootstrap.Instance.OnStatusChanged -= UpdateStatus;
            }
        }

        private void OnHostClicked()
        {
            UpdateStatus("Đang khởi tạo phòng Host (Port 7777)...");
            EnsureBootstrap();
            NetworkManagerBootstrap.Instance.StartHostSession();
        }

        private void OnJoinClicked()
        {
            string ip = m_IpInputField != null ? m_IpInputField.text : NetworkManagerBootstrap.k_DefaultAddress;
            UpdateStatus($"Đang kết nối tới {ip}...");
            EnsureBootstrap();
            NetworkManagerBootstrap.Instance.StartClientSession(ip);
        }

        private void OnBackClicked()
        {
            if (NetworkManagerBootstrap.Instance != null)
            {
                NetworkManagerBootstrap.Instance.DisconnectSession();
            }

            gameObject.SetActive(false);
        }

        private void UpdateStatus(string message)
        {
            if (m_StatusText != null)
            {
                m_StatusText.text = message;
            }
        }

        private void EnsureBootstrap()
        {
            if (NetworkManagerBootstrap.Instance == null)
            {
                GameObject bootObj = new GameObject("NetworkManagerBootstrap");
                bootObj.AddComponent<NetworkManagerBootstrap>();
            }
        }
    }
}
