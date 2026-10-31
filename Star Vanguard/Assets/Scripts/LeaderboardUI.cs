using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying and managing the leaderboard
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject m_LeaderboardContent;
    [SerializeField] private Transform m_LeaderboardContainer;
    [SerializeField] private GameObject m_LeaderboardEntryPrefab;
    [SerializeField] private TextMeshProUGUI m_NoScoresText;
    
    [Header("New High Score UI")]
    [SerializeField] private GameObject m_NewHighScorePanel;
    [SerializeField] private TMP_InputField m_PlayerNameInput;
    [SerializeField] private Button m_SubmitScoreButton;
    [SerializeField] private TextMeshProUGUI m_NewScoreText;
    
    private List<GameObject> m_LeaderboardEntryObjects = new List<GameObject>();
    private int m_PendingScore = 0;

    public void Configure(
        GameObject i_LeaderboardContent,
        Transform i_LeaderboardContainer,
        GameObject i_LeaderboardEntryPrefab,
        TextMeshProUGUI i_NoScoresText,
        GameObject i_NewHighScorePanel,
        TMP_InputField i_PlayerNameInput,
        Button i_SubmitScoreButton,
        TextMeshProUGUI i_NewScoreText)
    {
        m_LeaderboardContent = i_LeaderboardContent;
        m_LeaderboardContainer = i_LeaderboardContainer;
        m_LeaderboardEntryPrefab = i_LeaderboardEntryPrefab;
        m_NoScoresText = i_NoScoresText;
        m_NewHighScorePanel = i_NewHighScorePanel;
        m_PlayerNameInput = i_PlayerNameInput;
        m_SubmitScoreButton = i_SubmitScoreButton;
        m_NewScoreText = i_NewScoreText;

        if (m_SubmitScoreButton != null)
        {
            m_SubmitScoreButton.onClick.RemoveListener(submitScore);
            m_SubmitScoreButton.onClick.AddListener(submitScore);
        }

        if (m_PlayerNameInput != null)
        {
            m_PlayerNameInput.characterLimit = 20;
        }
    }
    
    private void Start()
    {
        // Set up submit button
        if (m_SubmitScoreButton)
        {
            m_SubmitScoreButton.onClick.AddListener(submitScore);
        }
        
        // Set up input field
        if (m_PlayerNameInput)
        {
            m_PlayerNameInput.characterLimit = 20; // Reasonable name length limit
        }
    }
    
    /// <summary>
    /// Show the leaderboard content (embedded in game over panel)
    /// </summary>
    public void ShowLeaderboard()
    {
        // Hide new high score panel and show leaderboard
        if (m_NewHighScorePanel) m_NewHighScorePanel.SetActive(false);
        if (m_LeaderboardContent)
        {
            m_LeaderboardContent.SetActive(true);
            refreshLeaderboard();
        }
        else
        {
            Debug.LogError("LeaderboardUI: m_LeaderboardContent is null! Cannot show leaderboard.");
        }
    }
    
    /// <summary>
    /// Hide the leaderboard content and new high score panel
    /// </summary>
    public void HideLeaderboard()
    {
        if (m_LeaderboardContent)
        {
            m_LeaderboardContent.SetActive(false);
        }
        if (m_NewHighScorePanel)
        {
            m_NewHighScorePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Check if player achieved a high score and show name input if needed
    /// </summary>
    public void CheckForHighScore(int i_Score)
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardUI: LeaderboardManager.Instance is null!");
            return;
        }
        
        if (LeaderboardManager.Instance.IsHighScore(i_Score))
        {
            showNewHighScoreInput(i_Score);
        }
        else
        {
            // Add the score to leaderboard (even if it doesn't qualify for top 5)
            LeaderboardManager.Instance.AddScore(i_Score);
            // Show the regular leaderboard
            ShowLeaderboard();
        }
    }
    
    /// <summary>
    /// Show the new high score input panel (embedded in game over panel)
    /// </summary>
    private void showNewHighScoreInput(int i_Score)
    {
        m_PendingScore = i_Score;
        
        // Hide leaderboard and show new high score input
        if (m_LeaderboardContent) m_LeaderboardContent.SetActive(false);
        if (m_NewHighScorePanel)
        {
            m_NewHighScorePanel.SetActive(true);
        }
        
        if (m_NewScoreText)
        {
            m_NewScoreText.text = $"{i_Score:N0}";
        }
        
        if (m_PlayerNameInput)
        {
            // Pre-fill with saved name if available
            string savedName = LeaderboardManager.Instance?.GetPlayerName() ?? "";
            m_PlayerNameInput.text = savedName;
            m_PlayerNameInput.Select();
            m_PlayerNameInput.ActivateInputField();
        }
    }
    
    /// <summary>
    /// Submit the high score with player name
    /// </summary>
    private void submitScore()
    {
        string playerName = m_PlayerNameInput?.text?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Anonymous";
        }
        
        // Save the name for future use
        LeaderboardManager.Instance?.SetPlayerName(playerName);
        
        // Add the score to leaderboard
        LeaderboardManager.Instance?.AddScore(m_PendingScore, playerName);
        
        // Hide new high score panel and show leaderboard
        if (m_NewHighScorePanel)
        {
            m_NewHighScorePanel.SetActive(false);
        }
        
        ShowLeaderboard();
    }
    
    /// <summary>
    /// Handle input field submit (Enter key)
    /// </summary>
    public void OnNameInputEndEdit()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            submitScore();
        }
    }
    
    /// <summary>
    /// Refresh the leaderboard display
    /// </summary>
    private void refreshLeaderboard()
    {
        // Clear existing entries
        clearLeaderboardEntries();
        
        if (LeaderboardManager.Instance == null) return;
        
        var entries = LeaderboardManager.Instance.GetLeaderboard();
        
        if (entries.Count == 0)
        {
            // Show "no scores" message
            if (m_NoScoresText)
            {
                m_NoScoresText.gameObject.SetActive(true);
            }
            return;
        }
        
        // Hide "no scores" message
        if (m_NoScoresText)
        {
            m_NoScoresText.gameObject.SetActive(false);
        }
        
        // Create leaderboard entries
        for (int i = 0; i < entries.Count; i++)
        {
            createLeaderboardEntry(i + 1, entries[i]);
        }
    }
    
    /// <summary>
    /// Create a single leaderboard entry UI element
    /// </summary>
    private void createLeaderboardEntry(int i_Rank, LeaderboardManager.LeaderboardEntry i_Entry)
    {
        if (m_LeaderboardContainer == null) return;
        
        GameObject entryObject;
        if (m_LeaderboardEntryPrefab != null)
        {
            entryObject = Instantiate(m_LeaderboardEntryPrefab, m_LeaderboardContainer);
        }
        else
        {
            entryObject = CreateDefaultEntry(m_LeaderboardContainer);
        }
        m_LeaderboardEntryObjects.Add(entryObject);
        
        // Find and set text components
        var rankText = entryObject.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        var nameText = entryObject.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        var scoreText = entryObject.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        var dateText = entryObject.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        
        if (rankText) rankText.text = $"{i_Rank}.";
        if (nameText) nameText.text = i_Entry.playerName;
        if (scoreText) scoreText.text = i_Entry.score.ToString("N0");
        if (dateText) dateText.text = i_Entry.date;
        
        // Highlight top 3 positions with different colors
        Color entryColor = i_Rank switch
        {
            1 => new Color(1f, 0.84f, 0f), // Gold
            2 => new Color(0.75f, 0.75f, 0.75f), // Silver
            3 => new Color(0.8f, 0.5f, 0.2f), // Bronze
            _ => Color.white // Default
        };
        
        if (rankText) rankText.color = entryColor;
        if (nameText) nameText.color = entryColor;
        if (scoreText) scoreText.color = entryColor;
    }
    
    /// <summary>
    /// Clear all leaderboard entry objects
    /// </summary>
    private GameObject CreateDefaultEntry(Transform i_Parent)
    {
        GameObject row = new GameObject("LeaderboardEntry", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(i_Parent, false);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(700f, 32f);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateEntryText(row.transform, "RankText", 45f, TextAlignmentOptions.Left);
        CreateEntryText(row.transform, "NameText", 280f, TextAlignmentOptions.Left);
        CreateEntryText(row.transform, "ScoreText", 200f, TextAlignmentOptions.Right);
        CreateEntryText(row.transform, "DateText", 140f, TextAlignmentOptions.Right);

        return row;
    }

    private TextMeshProUGUI CreateEntryText(Transform i_Parent, string i_Name, float i_Width, TextAlignmentOptions i_Align)
    {
        GameObject go = new GameObject(i_Name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(i_Parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(i_Width, 28f);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = i_Width;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 20;
        tmp.alignment = i_Align;
        tmp.color = Color.white;
        return tmp;
    }

    private void clearLeaderboardEntries()
    {
        foreach (var entry in m_LeaderboardEntryObjects)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        m_LeaderboardEntryObjects.Clear();
    }
    
    /// <summary>
    /// Clear all leaderboard data (for testing/reset button)
    /// </summary>
    public void ClearAllScores()
    {
        LeaderboardManager.Instance?.ClearLeaderboard();
        refreshLeaderboard();
        AudioManager.Instance?.OnUIButtonClick();
    }
}
