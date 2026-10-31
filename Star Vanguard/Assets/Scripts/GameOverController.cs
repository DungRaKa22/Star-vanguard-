using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles game over screen menu interactions
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI m_FinalScoreText;
    [SerializeField] private LeaderboardUI m_LeaderboardUI;
    
    [Header("Button Delay Settings")]
    [SerializeField] private float m_ButtonDelayDuration = 1f; // Delay duration
    [SerializeField] private Button m_RestartButton;
    [SerializeField] private Button m_QuitButton;

    public void Configure(
        TextMeshProUGUI i_FinalScoreText,
        LeaderboardUI i_LeaderboardUI,
        Button i_RestartButton,
        Button i_QuitButton,
        float i_ButtonDelay = 0.5f)
    {
        m_FinalScoreText = i_FinalScoreText;
        m_LeaderboardUI = i_LeaderboardUI;
        m_RestartButton = i_RestartButton;
        m_QuitButton = i_QuitButton;
        m_ButtonDelayDuration = i_ButtonDelay;

        if (m_RestartButton != null)
        {
            m_RestartButton.onClick.RemoveListener(Restart);
            m_RestartButton.onClick.AddListener(Restart);
        }

        if (m_QuitButton != null)
        {
            m_QuitButton.onClick.RemoveListener(QuitToMenu);
            m_QuitButton.onClick.AddListener(QuitToMenu);
        }
    }
    
    private void OnEnable()
    {
        // Update final score display when game over panel is shown
        updateFinalScore();
        
        // Check for high score and show leaderboard
        checkHighScore();
        
        // Delay Button functionality to avoid accidental clicks
        StartCoroutine(delayButtonFunctionality());
    }

    private IEnumerator delayButtonFunctionality()
    {
        if (m_QuitButton) m_QuitButton.interactable = false;
        if (m_RestartButton) m_RestartButton.interactable = false;
        
        yield return new WaitForSeconds(m_ButtonDelayDuration);
        
        if (m_QuitButton) m_QuitButton.interactable = true;
        if (m_RestartButton) m_RestartButton.interactable = true;
    }

    private void OnDisable()
    {
        // Hide leaderboard content when game over panel is hidden
        if (m_LeaderboardUI)
        {
            m_LeaderboardUI.HideLeaderboard();
        }
    }
    
    /// <summary>
    /// Update the final score display
    /// </summary>
    private void updateFinalScore()
    {
        if (m_FinalScoreText && GameManager.Instance != null)
        {
            m_FinalScoreText.text = $"{GameManager.Instance.CurrentScore:N0}";
        }
    }
    
    /// <summary>
    /// Check if player achieved a high score
    /// </summary>
    private void checkHighScore()
    {
        if (m_LeaderboardUI && GameManager.Instance != null)
        {
            m_LeaderboardUI.CheckForHighScore(GameManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogWarning($"GameOverController: Cannot check high score - LeaderboardUI: {m_LeaderboardUI != null}, GameManager: {GameManager.Instance != null}");
        }
    }
    
    
    /// <summary>
    /// Restart the current game - called by "Restart" button
    /// </summary>
    public void Restart()
    {
        GameManager.Instance?.RestartGame();
    }
    
    /// <summary>
    /// Return to home screen - called by "Quit" button on game over screen
    /// </summary>
    public void QuitToMenu()
    {
        GameManager.Instance?.ReturnToMenu();
    }
}
