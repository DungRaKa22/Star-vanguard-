using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static event Action<eGameState> OnGameStateChanged;
    
    [Header("Game Settings")]
    [SerializeField] private int m_StartingLives = 3;

    [Header("Difficulty (chosen on the main menu)")]
    [Range(1, 3)]
    [SerializeField] private int m_SelectedDifficulty = 1; // 1 = Cadet, 2 = Veteran, 3 = Ace

    public int SelectedDifficulty => Mathf.Clamp(m_SelectedDifficulty, 1, 3);

    /// <summary>Sets the difficulty tier picked on the main menu (1..3).</summary>
    public void SetSelectedDifficulty(int i_Tier)
    {
        m_SelectedDifficulty = Mathf.Clamp(i_Tier, 1, 3);
    }
    
    [Header("Resolution Settings (Standard 1080p 16:9)")]
    [SerializeField] private int m_TargetWidth = 1920;
    [SerializeField] private int m_TargetHeight = 1080;
    [SerializeField] private bool m_ForceResolution = true;
    
    private int m_Lives;
    private int m_Score;
    
    // Public properties for external access
    public int CurrentScore => m_Score;
    public int CurrentLives => m_Lives;
    public eGameState GameState { get; private set; } = eGameState.Menu;
    
    // Enemy scaling system
    public int CurrentDifficultyTier { get; private set; } = 1; // Tier 1 = waves 1-5, Tier 2 = waves 6-10, etc.
    
    // Automatic pickup thresholds
    private int m_LastBombScoreThreshold;
    private int m_LastLifeScoreThreshold;
    private const int k_BombScoreInterval = 10000;
    private const int k_LifeScoreInterval = 50000;
    
    protected override void Awake()
    {
        base.Awake(); // Call singleton Awake first
        if (this == Instance) // Only initialize if we're the active instance
        {
            // Force resolution before anything else initializes
            if (m_ForceResolution)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.SetResolution(m_TargetWidth, m_TargetHeight, true);
            }
            
            initializeGame();

            // React to scene transitions so Menu -> Gameplay always boots the mission.
            // The scene copy of GameManager is destroyed by the duplicate guard in
            // Singleton.Awake, which means its Start() never runs on a scene load.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene i_Scene, LoadSceneMode i_Mode)
    {
        if (this != Instance) return;

        if (i_Scene.name == "GameplayScene")
        {
            if (GameState != eGameState.Playing)
            {
                StartNewGame();
            }
        }
        else if (i_Scene.name == "MainMenuScene")
        {
            // Returning to the main menu: stop waves, hide HUD, restore menu state
            Time.timeScale = 1f;
            setGameState(eGameState.Menu);
        }
    }
    
    private void Start()
    {
        // Apply resolution again in Start to ensure it takes effect
        if (m_ForceResolution && this == Instance)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(m_TargetWidth, m_TargetHeight, false);
        }

        // Fallback for the very first scene load (sceneLoaded can fire before Awake)
        if (SceneManager.GetActiveScene().name == "GameplayScene" && GameState != eGameState.Playing)
        {
            StartNewGame();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameState == eGameState.Playing)
            {
                setGameState(eGameState.Paused);
            }
            else if (GameState == eGameState.Paused)
            {
                setGameState(eGameState.Playing);
            }
        }
    }
    
    private void initializeGame()
    {
        m_Lives = m_StartingLives;
        m_Score = 0;
        m_LastBombScoreThreshold = 0;
        m_LastLifeScoreThreshold = 0;
        CurrentDifficultyTier = 1;
        Time.timeScale = 1f;
        
        // Reset all object pools
        PoolManager.Instance?.ResetAllPools();
        
        // Clear all active pickups from previous game
        clearActivePickups();
        
        // Reset player stats
        var playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats) playerStats.ResetStats();
        
        // Initialize player as hidden (will spawn when game starts)
        var playerRespawn = FindAnyObjectByType<PlayerRespawn>();
        if (playerRespawn) playerRespawn.InitializePlayerAsHidden();
        
        // Update UI to reflect reset values
        UIManager.Instance?.SetScore(m_Score);
        UIManager.Instance?.SetLives(m_Lives);
    }
    
    public void SetDifficultyTier(int i_WaveNumber)
    {
        // Menu difficulty shifts the wave curve: Cadet runs one tier softer, Ace one harder.
        int waveTier = Mathf.Max(1, (i_WaveNumber - 1) / 5 + 1);
        int newTier = Mathf.Max(1, waveTier + (SelectedDifficulty - 2));
        if (newTier != CurrentDifficultyTier)
        {
            CurrentDifficultyTier = newTier;
        }
    }
    
    public void AddScore(int i_Amount)
    {
        m_Score += i_Amount;
        UIManager.Instance?.SetScore(m_Score);
        
        // Check for automatic bomb grants every 10000 points
        if (m_Score >= m_LastBombScoreThreshold + k_BombScoreInterval)
        {
            m_LastBombScoreThreshold = (m_Score / k_BombScoreInterval) * k_BombScoreInterval;
            var playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats) playerStats.AddBomb(1);
        }
        
        // Check for automatic life grants every 50000 points
        if (m_Score >= m_LastLifeScoreThreshold + k_LifeScoreInterval)
        {
            m_LastLifeScoreThreshold = (m_Score / k_LifeScoreInterval) * k_LifeScoreInterval;
            addLife(1);
        }
    }
    
    private void addLife(int i_Amount)
    {
        m_Lives += i_Amount;
        UIManager.Instance?.SetLives(m_Lives);
        
        // Play pickup sound when gaining lives
        if (i_Amount > 0)
        {
            AudioManager.Instance?.Play(eSFXId.Pickup);
        }
    }

    public void DamagePlayer(int i_Amount = 1)
    {
        if (GameState != eGameState.Playing) return;
        
        m_Lives = Mathf.Max(0, m_Lives - i_Amount);
        UIManager.Instance?.SetLives(m_Lives);
        
        if (m_Lives <= 0) 
        {
            AudioManager.Instance?.Play(eSFXId.GameOver);
            setGameState(eGameState.GameOver);
        }
    }

    private void setGameState(eGameState i_GameState)
    {
        if (GameState == i_GameState) return;
        
        GameState = i_GameState;
        
        // Handle time scale for different states
        if (GameState == eGameState.Paused || GameState == eGameState.Menu) 
            Time.timeScale = 0f;
        else 
            Time.timeScale = 1f;
        
        // Handle cursor visibility - hide during gameplay, show in menus
        Cursor.visible = (GameState != eGameState.Playing);
        
        OnGameStateChanged?.Invoke(GameState);
    }

    /// <summary>
    /// Start a new game from the home screen
    /// </summary>
    public void StartNewGame()
    {
        // Reset wave director to start from wave 1
        WaveDirector.Instance?.ResetWaves();
        
        initializeGame();
        setGameState(eGameState.Playing);
        
        // Spawn player from bottom of screen
        var playerRespawn = FindAnyObjectByType<PlayerRespawn>();
        if (playerRespawn) 
        {
            playerRespawn.SpawnPlayerAtGameStart();
        }
        else
        {
            Debug.LogWarning("GameManager: PlayerRespawn component not found in StartNewGame!");
        }
    }
    
    /// <summary>
    /// Return to home screen (from pause or game over)
    /// </summary>
    public void ReturnToMenu()
    {
        if (SceneManager.sceneCountInBuildSettings > 1 && SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenuScene");
            return;
        }

        initializeGame();
        setGameState(eGameState.Menu);
    }

    /// <summary>
    /// Loads the dedicated gameplay combat scene
    /// </summary>
    public void LoadGameplayScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameplayScene");
    }

    /// <summary>
    /// Loads the dedicated main menu scene
    /// </summary>
    public void LoadMainMenuScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
    
    /// <summary>
    /// Resume game from pause
    /// </summary>
    public void ResumeGame()
    {
        setGameState(eGameState.Playing);
    }
    
    /// <summary>
    /// Restart current game (unified restart without scene reload)
    /// </summary>
    public void RestartGame()
    {
        // Reset wave director to start from wave 1
        WaveDirector.Instance?.ResetWaves();
        
        // Restart music from beginning
        AudioManager.Instance?.OnGameRestart();
        
        initializeGame();
        setGameState(eGameState.Playing);
        
        // Spawn player from bottom of screen
        var playerRespawn = FindAnyObjectByType<PlayerRespawn>();
        if (playerRespawn) 
        {
            playerRespawn.SpawnPlayerAtGameStart();
        }
        else
        {
            Debug.LogWarning("GameManager: PlayerRespawn component not found in RestartGame!");
        }
    }

    /// <summary>
    /// Quit application entirely (only from home screen)
    /// </summary>
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// Clear weapon power-ups from the scene when starting a new game
    /// (Energy crystals and enemy bullets are handled by PoolManager.ResetAllPools())
    /// </summary>
    private void clearActivePickups()
    {
        // Clear weapon power-ups (these are instantiated, not pooled)
        var powerUps = FindObjectsByType<PowerUpPickup>();
        foreach (var powerUp in powerUps)
        {
            if (powerUp && powerUp.gameObject.activeInHierarchy)
            {
                Destroy(powerUp.gameObject);
            }
        }
    }
}
