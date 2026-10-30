using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard
{
    /// <summary>
    /// Dedicated bootstrap controller for Star Vanguard.
    ///
    /// Sits in BootstrapScene.unity alongside the persistent managers (GameManager,
    /// AudioManager, PoolManager, LeaderboardManager, SettingsManager, PickupManager).
    /// Upon initialization, these managers mark themselves with DontDestroyOnLoad.
    /// BootstrapLoader then immediately loads MainMenuScene, ensuring neither
    /// MainMenuScene nor GameplayScene need local copies of persistent managers.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class BootstrapLoader : MonoBehaviour
    {
        private const string k_BootstrapSceneName = "BootstrapScene";
        private const string k_MainMenuSceneName = "MainMenuScene";

        private void Start()
        {
            // If BootstrapScene was the primary entry scene, transition directly to the Main Menu
            if (SceneManager.GetActiveScene().name == k_BootstrapSceneName)
            {
                SceneManager.LoadScene(k_MainMenuSceneName);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Developer convenience for the Unity Editor:
        /// If a developer opens MainMenuScene or GameplayScene and hits Play directly,
        /// this hook loads BootstrapScene additively before the active scene initializes,
        /// ensuring the persistent managers are always instantiated without duplicating them in the scene.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapInEditor()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == k_BootstrapSceneName) return;

            if (!GameManager.HasInstance)
            {
                Debug.Log($"[Bootstrap] Loading {k_BootstrapSceneName} additively for active scene '{activeScene}'...");
                SceneManager.LoadScene(k_BootstrapSceneName, LoadSceneMode.Additive);
            }
        }
#endif
    }
}
