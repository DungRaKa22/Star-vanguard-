using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[SceneBoundSingleton]
public class UIManager : Singleton<UIManager>
{
    // Panels/HUD live inside the scene, so this manager must die with the scene
    // instead of keeping stale references to destroyed GameObjects.
    protected override bool PersistAcrossScenes => false;

    #region UI Navigation Events
    public static event Action OnShowSettingsRequested;
    public static event Action OnHideSettingsRequested;
    public static event Action OnShowControlsRequested;
    public static event Action OnHideControlsRequested;
    public static event Action OnShowAudioSettingsRequested;
    public static event Action OnHideSubPanelsRequested;
    public static event Action OnHideAllPanelsRequested;
    #endregion

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    
    [Header("Lives Display")]
    [SerializeField] private Transform m_LivesContainer;
    [SerializeField] private Sprite m_HeartSprite; // Lives icon (ship hull/shield)
    private List<HUDIcon> m_HeartInstances = new List<HUDIcon>();
    
    [Header("Bombs Display")]
    [SerializeField] private Transform m_BombsContainer;
    [SerializeField] private Sprite m_BombSprite; // Use Bomb sprite
    private List<HUDIcon> m_BombInstances = new List<HUDIcon>();
    
    [Header("Icon Settings")]
    [SerializeField] private Vector2 m_IconSize = new Vector2(32, 32);
    
    [Header("HUD Container")]
    [SerializeField] private GameObject m_HUDContainer; // Parent container for all HUD elements
    
    [Header("UI Panels")]
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private GameObject m_PausePanel;
    [SerializeField] private GameObject m_HomeScreenPanel;
    [SerializeField] private GameObject m_ControlsPanel;
    [SerializeField] private GameObject m_SettingsPanel;

    public void SetGameOverPanel(GameObject i_Panel) => m_GameOverPanel = i_Panel;
    public void SetPausePanel(GameObject i_Panel) => m_PausePanel = i_Panel;
    public void SetSettingsPanel(GameObject i_Panel) => m_SettingsPanel = i_Panel;

    public bool HasActiveSubPanel => (m_SettingsPanel != null && m_SettingsPanel.activeSelf) ||
                                     (m_ControlsPanel != null && m_ControlsPanel.activeSelf);

    public static void RequestHideSubPanels() => OnHideSubPanelsRequested?.Invoke();

    private void Start()
    {
        // Fix Canvas scale issue that can make UI invisible
        FixCanvasScaleIssue();
        // Initialize all panels as inactive
        hideAllPanels();
        
        // Ensure we don't double-subscribe if this Start() is called multiple times
        GameManager.OnGameStateChanged -= showState; // Unsubscribe first (safe even if not subscribed)
        GameManager.OnGameStateChanged += showState; // Subscribe
        
        // Show initial state (Menu)
        showState(GameManager.Instance?.GameState ?? eGameState.Menu);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy(); // Clear the Singleton instance reference

        GameManager.OnGameStateChanged -= showState;
        
        // Clear references without querying PoolManager when scene is unloading
        m_HeartInstances.Clear();
        m_BombInstances.Clear();
    }
    
    /// <summary>
    /// Returns a list of icon instances to the pool
    /// </summary>
    private void ReturnIconInstancesToPool(List<HUDIcon> instances)
    {
        if (!PoolManager.HasInstance)
        {
            instances.Clear();
            return;
        }

        foreach (var instance in instances)
        {
            if (instance != null)
            {
                PoolManager.Instance?.ReturnHUDIcon(instance);
            }
        }
        instances.Clear();
    }
    
    /// <summary>
    /// Configures Canvas and CanvasScaler to lock strictly at 1920x1080 Full HD (16:9).
    /// Attaches CanvasAspectFitter so HUD never clips outside the camera letterboxed frame.
    /// </summary>
    private void FixCanvasScaleIssue()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;
        
        Vector3 currentScale = canvas.transform.localScale;
        if (currentScale.x == 0f || currentScale.y == 0f || currentScale.z == 0f)
        {
            canvas.transform.localScale = Vector3.one;
        }

        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (m_HUDContainer != null && !m_HUDContainer.GetComponent<CanvasAspectFitter>())
        {
            m_HUDContainer.AddComponent<CanvasAspectFitter>();
        }
    }
    
    public void SetScore(int i_Value)
    {
        // Zero-allocation update: TMP formats the integer without creating a managed string
        if (m_ScoreText) m_ScoreText.SetText("{0}", i_Value);
    }
    
    public void SetLives(int i_Value)
    {
        UpdateImageDisplay(m_HeartInstances, m_HeartSprite, m_LivesContainer, i_Value);
    }       

    private void showState(eGameState i_GameState)
    {
        // Handle HUD state transitions
        bool wasPlaying = m_HUDContainer && m_HUDContainer.activeInHierarchy;
        bool willBePlaying = i_GameState == eGameState.Playing;
        
        if (wasPlaying && !willBePlaying)
        {
            OnLeavePlayingState();
        }
        
        // Hide all panels first
        hideAllPanels();
        
        // Control HUD visibility - only show during gameplay
        if (m_HUDContainer)
        {
            m_HUDContainer.SetActive(willBePlaying);
        }

        // Handle gameplay state transitions
        switch (i_GameState)
        {
            case eGameState.Menu:
                SetPanelActive(m_HomeScreenPanel, true);
                break;
            case eGameState.Paused:
                SetPanelActive(m_PausePanel, true);
                break;
            case eGameState.GameOver:
                SetPanelActive(m_GameOverPanel, true);
                break;
            case eGameState.Playing:
                // No panels active during gameplay, but HUD is visible
                OnEnterPlayingState();
                break;
        }
    }

    private void hideAllPanels()
    {
        // Fallback for serialized scene panels if assigned
        SetPanelActive(m_GameOverPanel, false);
        SetPanelActive(m_PausePanel, false);
        SetPanelActive(m_HomeScreenPanel, false);
        SetPanelActive(m_ControlsPanel, false);
        SetPanelActive(m_SettingsPanel, false);
    }

    /// <summary>
    /// Destroys-safe SetActive. Using "panel?.SetActive()" is NOT safe in Unity:
    /// the null-conditional operator bypasses Unity's destroyed-object check and throws
    /// MissingReferenceException, which used to abort the OnGameStateChanged event chain
    /// (so WaveDirector never received Playing after a scene load).
    /// </summary>
    private static void SetPanelActive(GameObject i_Panel, bool i_Active)
    {
        if (i_Panel != null)
        {
            i_Panel.SetActive(i_Active);
        }
    }

    public void SetBombs(int i_Amount)
    {
        UpdateImageDisplay(m_BombInstances, m_BombSprite, m_BombsContainer, i_Amount);
    }
    
    /// <summary>
    /// Refresh HUD display - called when pools are ready
    /// </summary>
    public void RefreshHUDDisplay()
    {
        if (GameManager.Instance != null)
        {
            SetLives(GameManager.Instance.CurrentLives);
            // Get current bomb count from PlayerStats
            var playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                SetBombs(playerStats.BombCount);
            }
        }
    }
    
    /// <summary>
    /// Called when entering playing state to ensure HUD is properly displayed
    /// </summary>
    public void OnEnterPlayingState()
    {
        // Force refresh HUD display when entering playing state
        RefreshHUDDisplay();
    }
    
    /// <summary>
    /// Called when leaving playing state to clean up HUD
    /// </summary>
    public void OnLeavePlayingState()
    {
        // Return HUD icons to pool when leaving playing state
        ReturnIconInstancesToPool(m_HeartInstances);
        ReturnIconInstancesToPool(m_BombInstances);
    }
    
    /// <summary>
    /// Updates an image-based display using pooled instances
    /// </summary>
    private void UpdateImageDisplay(List<HUDIcon> instances, Sprite sprite, Transform container, int targetCount)
    {
        if (!sprite || !container || !PoolManager.Instance) return;
        
        // Check if pool is ready (will be refreshed automatically once PoolManager.Start() completes)
        if (!PoolManager.Instance.IsHUDIconPoolReady())
        {
            return;
        }
        
        // Return excess instances to pool
        while (instances.Count > targetCount)
        {
            int lastIndex = instances.Count - 1;
            if (instances[lastIndex] != null)
            {
                PoolManager.Instance.ReturnHUDIcon(instances[lastIndex]);
            }
            instances.RemoveAt(lastIndex);
        }
        
        // Get missing instances from pool
        while (instances.Count < targetCount)
        {
            HUDIcon newInstance = PoolManager.Instance.GetHUDIcon(Vector3.zero);
            if (newInstance != null)
            {
                SetupHUDIcon(newInstance, sprite, container);
                instances.Add(newInstance);
            }
            else
            {
                Debug.LogWarning("UIManager: Failed to get HUD icon from pool!");
                break;
            }
        }
    }
    
    /// <summary>
    /// Sets up a pooled HUD icon with the given sprite and parent
    /// </summary>
    private void SetupHUDIcon(HUDIcon hudIcon, Sprite sprite, Transform parent)
    {
        hudIcon.transform.SetParent(parent, false);
        hudIcon.SetSprite(sprite);
        hudIcon.SetSize(m_IconSize);
        
        // Set RectTransform properties for UI layout
        RectTransform rectTransform = hudIcon.GetComponent<RectTransform>();
        if (rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    public void ShowControlsPanel()
    {
        OnShowControlsRequested?.Invoke();
    }

    public void HideControlsPanel()
    {
        OnHideControlsRequested?.Invoke();
        OnHideSubPanelsRequested?.Invoke();
    }

    public void ShowSettingsPanel()
    {
        OnShowSettingsRequested?.Invoke();
    }

    public void HideSettingsPanel()
    {
        OnHideSettingsRequested?.Invoke();
        OnHideSubPanelsRequested?.Invoke();
    }

    public void ShowAudioSettingsPanel()
    {
        OnShowAudioSettingsRequested?.Invoke();
    }

    public void HideSubPanels()
    {
        OnHideSubPanelsRequested?.Invoke();
    }

    public void HideAllPanels()
    {
        OnHideAllPanelsRequested?.Invoke();
        hideAllPanels();
    }
}
