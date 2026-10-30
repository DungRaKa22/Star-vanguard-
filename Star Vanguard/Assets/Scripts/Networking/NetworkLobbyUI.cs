using UnityEngine;

#if MIRROR
using Mirror;
#endif

/// <summary>
/// Sci-Fi Cybernetic Co-op LAN Lobby UI for Star Vanguard.
/// Provides zero-alloc procedural GUI for hosting and joining multiplayer sessions.
/// Operates seamlessly both in Unity Editor and Standalone 1080p builds.
/// </summary>
public class NetworkLobbyUI : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string m_TargetIP = "127.0.0.1";
    [SerializeField] private ushort m_TargetPort = 7777;

    private bool m_ShowMultiplayerMenu = false;
    private string m_StatusMessage = "NETWORK PROTOCOL: READY (LAN STANDALONE)";
    private GUIStyle m_BoxStyle;
    private GUIStyle m_HeaderStyle;
    private GUIStyle m_ButtonStyle;
    private GUIStyle m_StatusStyle;
    private GUIStyle m_LabelStyle;
    private GUIStyle m_TextFieldStyle;
    private Texture2D m_BgTex;
    private Texture2D m_BtnTex;
    private bool m_StylesInitialized = false;

    private void Start()
    {
        // Auto-detect local machine host if possible
        if (string.IsNullOrEmpty(m_TargetIP))
        {
            m_TargetIP = "127.0.0.1";
        }
    }

    private void OnDestroy()
    {
        if (m_BgTex != null) Destroy(m_BgTex);
        if (m_BtnTex != null) Destroy(m_BtnTex);
    }

    private void initializeStyles()
    {
        if (m_StylesInitialized) return;

        m_BgTex = new Texture2D(1, 1);
        m_BgTex.SetPixel(0, 0, new Color(0.02f, 0.05f, 0.12f, 0.92f));
        m_BgTex.Apply();

        m_BtnTex = new Texture2D(1, 1);
        m_BtnTex.SetPixel(0, 0, new Color(0.08f, 0.25f, 0.45f, 0.85f));
        m_BtnTex.Apply();

        m_BoxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = m_BgTex }
        };

        m_HeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.0f, 0.95f, 1.0f) }
        };

        m_StatusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.2f, 1.0f, 0.5f) }
        };

        m_LabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.8f, 0.9f, 1.0f) }
        };

        m_TextFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleLeft
        };

        m_ButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { background = m_BtnTex, textColor = Color.white }
        };

        m_StylesInitialized = true;
    }

    private void OnGUI()
    {
        // Only render during Menu, and only while the lobby panel is explicitly opened.
        // MainMenuUI owns the single "CO-OP MULTIPLAYER" entry point so the two IMGUI
        // overlays no longer draw on top of each other.
        if (GameManager.Instance != null && GameManager.Instance.GameState != eGameState.Menu)
        {
            return;
        }

        if (!m_ShowMultiplayerMenu)
        {
            return;
        }

        initializeStyles();

        // 1080p responsive coordinate calculation
        float panelWidth = 460f;
        float panelHeight = m_ShowMultiplayerMenu ? 320f : 55f;
        float panelX = (Screen.width - panelWidth) * 0.5f;
        float panelY = Screen.height - panelHeight - 30f;

        Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
        GUI.Box(panelRect, GUIContent.none, m_BoxStyle);

        GUILayout.BeginArea(panelRect);
        GUILayout.Space(6);

        if (!m_ShowMultiplayerMenu)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("🛰️ CO-OP MULTIPLAYER (LAN PROTOCOL)", m_ButtonStyle, GUILayout.Height(40)))
            {
                m_ShowMultiplayerMenu = true;
                AudioManager.Instance?.Play(eSFXId.UIClick);
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
        }
        else
        {
            // Full Co-op configuration panel
            GUILayout.Label("🛰️ STAR VANGUARD: CO-OP LAN PROTOCOL", m_HeaderStyle);
            GUILayout.Label(m_StatusMessage, m_StatusStyle);
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("TARGET HOST IP:", m_LabelStyle, GUILayout.Width(130));
            m_TargetIP = GUILayout.TextField(m_TargetIP, m_TextFieldStyle, GUILayout.Width(180));
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("PORT:", m_LabelStyle, GUILayout.Width(130));
            GUILayout.Label(m_TargetPort.ToString(), m_LabelStyle, GUILayout.Width(180));
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Action Buttons
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);

            if (GUILayout.Button("🛰️ HOST LAN", m_ButtonStyle, GUILayout.Height(36)))
            {
                StartHostLAN();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("📡 JOIN WINGMAN", m_ButtonStyle, GUILayout.Height(36)))
            {
                JoinClientLAN();
            }

            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("❌ CLOSE PROTOCOL", m_ButtonStyle, GUILayout.Height(30)))
            {
                CloseLobbyPanel();
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Opens the Co-op LAN lobby panel (called by the main menu button).
    /// </summary>
    public void OpenLobbyPanel()
    {
        m_ShowMultiplayerMenu = true;
        AudioManager.Instance?.Play(eSFXId.UIClick);
    }

    /// <summary>
    /// Closes the Co-op LAN lobby panel.
    /// </summary>
    public void CloseLobbyPanel()
    {
        m_ShowMultiplayerMenu = false;
        AudioManager.Instance?.Play(eSFXId.UIClick);
    }

    /// <summary>
    /// Loads the dedicated combat scene (or starts the mission if we are already in it).
    /// </summary>
    private void LoadGameplayOrStart()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameplayScene")
        {
            GameManager.Instance?.StartNewGame();
        }
        else
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
        }
    }

    public void StartHostLAN()
    {
        AudioManager.Instance?.Play(eSFXId.UIClick);
#if MIRROR
        if (NetworkServer.active)
        {
            m_StatusMessage = "STATUS: SERVER ALREADY ACTIVE!";
            return;
        }

        if (NetworkManager.singleton != null)
        {
            NetworkManager.singleton.StartHost();
            m_StatusMessage = "STATUS: HOSTING ON PORT " + m_TargetPort + " (WAITING FOR WINGMAN)";
            GameManager.Instance?.StartNewGame();
        }
        else
        {
            m_StatusMessage = "STATUS: NETWORK MANAGER NOT FOUND";
        }
#else
        m_StatusMessage = "STATUS: HOST SIMULATED (SOLO STANDALONE)";
        LoadGameplayOrStart();
#endif
    }

    public void JoinClientLAN()
    {
        AudioManager.Instance?.Play(eSFXId.UIClick);
#if MIRROR
        if (NetworkClient.isConnected)
        {
            m_StatusMessage = "STATUS: CLIENT ALREADY CONNECTED!";
            return;
        }

        if (NetworkManager.singleton != null)
        {
            NetworkManager.singleton.networkAddress = m_TargetIP;
            NetworkManager.singleton.StartClient();
            m_StatusMessage = "STATUS: CONNECTING TO " + m_TargetIP + "...";
            GameManager.Instance?.StartNewGame();
        }
        else
        {
            m_StatusMessage = "STATUS: NETWORK MANAGER NOT FOUND";
        }
#else
        m_StatusMessage = "STATUS: JOINING " + m_TargetIP + " (SOLO STANDALONE)";
        LoadGameplayOrStart();
#endif
    }

    public void DisconnectLAN()
    {
        AudioManager.Instance?.Play(eSFXId.UIClick);
#if MIRROR
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
#endif
        m_StatusMessage = "STATUS: DISCONNECTED (LAN STANDALONE)";
    }
}
