using UnityEngine;

/// <summary>
/// Handles home screen menu interactions
/// </summary>
public class HomeScreenController : MonoBehaviour
{
    /// <summary>
    /// Start a new game - called by "Launch Mission" button
    /// </summary>
    public void StartGame()
    {
        AudioManager.Instance?.Play(eSFXId.UIClick);
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
        }
        else
        {
            GameManager.Instance?.StartNewGame();
        }
    }
    
    /// <summary>
    /// Quit the application entirely - called by "Quit" button on home screen
    /// </summary>
    public void QuitApplication()
    {
        GameManager.Instance?.QuitApplication();
    }

    public void OnControlsClick()
    {
        UIManager.Instance?.ShowControlsPanel();
    }

    /// <summary>
    /// Starts hosting a LAN Co-op session
    /// </summary>
    public void StartCoopHost()
    {
        var lobbyUI = FindAnyObjectByType<NetworkLobbyUI>();
        lobbyUI?.StartHostLAN();
    }

    /// <summary>
    /// Joins an existing LAN Co-op session
    /// </summary>
    public void JoinCoopClient()
    {
        var lobbyUI = FindAnyObjectByType<NetworkLobbyUI>();
        lobbyUI?.JoinClientLAN();
    }
}
