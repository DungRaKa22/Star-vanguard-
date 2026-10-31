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
            StarVanguard.UI.UIPanelRouter.OnPanelRegistered -= HandlePanelRegistered;
            StarVanguard.UI.UIPanelRouter.OnPanelRegistered += HandlePanelRegistered;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryAttach();
        }

        private static void HandlePanelRegistered(StarVanguard.UI.UIPanelId id, GameObject panel)
        {
            if (id == StarVanguard.UI.UIPanelId.Coop && panel != null)
            {
                if (panel.GetComponent<CoopMenuPresenter>() == null)
                {
                    panel.AddComponent<CoopMenuPresenter>();
                    Debug.Log("[STARVANGUARD-NET] Successfully attached CoopMenuPresenter via UIPanelRouter event!");
                }
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttach();
        }

        private static void TryAttach()
        {
            if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
                foreach (Canvas canvas in canvases)
                {
                    Transform t = canvas.transform.Find("CoopModalPanel");
                    if (t != null)
                    {
                        if (t.GetComponent<CoopMenuPresenter>() == null)
                        {
                            t.gameObject.AddComponent<CoopMenuPresenter>();
                            Debug.Log("[STARVANGUARD-NET] Successfully attached CoopMenuPresenter to CoopModalPanel!");
                        }
                        return;
                    }
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

        private void OnEnable()
        {
            ResolveUIBindings();
            AttachListeners();

            if (m_IpInputField != null && string.IsNullOrEmpty(m_IpInputField.text))
            {
                m_IpInputField.text = NetworkManagerBootstrap.k_DefaultAddress;
            }

            UpdateStatus("Sẵn sàng tạo phòng Host hoặc kết nối tới địa chỉ IP...");
        }

        private void OnDisable()
        {
            DetachListeners();
        }

        private void AttachListeners()
        {
            if (m_HostButton != null)
            {
                m_HostButton.onClick.RemoveListener(OnHostClicked);
                m_HostButton.onClick.AddListener(OnHostClicked);
            }
            if (m_JoinButton != null)
            {
                m_JoinButton.onClick.RemoveListener(OnJoinClicked);
                m_JoinButton.onClick.AddListener(OnJoinClicked);
            }
            if (m_BackButton != null)
            {
                m_BackButton.onClick.RemoveListener(OnBackClicked);
                m_BackButton.onClick.AddListener(OnBackClicked);
            }

            if (NetworkManagerBootstrap.Instance != null)
            {
                NetworkManagerBootstrap.Instance.OnStatusChanged -= UpdateStatus;
                NetworkManagerBootstrap.Instance.OnStatusChanged += UpdateStatus;
            }
        }

        private void DetachListeners()
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

            if (StarVanguard.UI.UIPanelRouter.Instance != null)
            {
                StarVanguard.UI.UIPanelRouter.Instance.Back();
            }
            else
            {
                gameObject.SetActive(false);
            }
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
