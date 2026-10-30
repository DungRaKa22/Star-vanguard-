using StarVanguard;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Editor maintenance tool that creates a dedicated BootstrapScene for persistent managers
    /// (GameManager, AudioManager, PoolManager, LeaderboardManager, SettingsManager, PickupManager)
    /// and strips all duplicate manager copies from GameplayScene and MainMenuScene.
    /// </summary>
    public static class StarVanguardBootstrapSetup
    {
        private const string k_BackupScenePath = "SceneBackups/MainMenuScene.unity.bak";
        private const string k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
        private const string k_MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string k_GameplayScenePath = "Assets/Scenes/GameplayScene.unity";

        private static readonly string[] k_PersistentManagerNames =
        {
            "GameManager",
            "AudioManager",
            "PoolManager",
            "LeaderboardManager",
            "SettingsManager"
        };

        private static readonly string[] k_NonBootstrapObjects =
        {
            "Canvas",
            "EventSystem",
            "ParallaxRoot",
            "UIManager",
            "MenuRoot",
            "FirstTile",
            "SecondTile",
            "Player",
            "Boundaries",
            "WaveDirector",
            "InterWaveMessageManager",
            "HUDContainer",
            "HUDBombsContainer",
            "HUDLivesContainer",
            "PausePanel",
            "GameOverPanel",
            "NewHighScorePanel",
            "InterWaveMessagePanel"
        };

        [MenuItem("Star Vanguard/Maintenance/Setup Bootstrap Scene")]
        public static void ExecuteSetup()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[BOOTSTRAP-SETUP] ===== Starting Bootstrap Scene Setup =====");

            // Step 1: Create BootstrapScene from backup (which contains full manager configurations)
            string sourcePath = File.Exists(k_BackupScenePath) ? k_BackupScenePath : k_MainMenuScenePath;
            if (File.Exists(k_BootstrapScenePath))
            {
                AssetDatabase.DeleteAsset(k_BootstrapScenePath);
                sb.AppendLine($"[BOOTSTRAP-SETUP] Deleted existing {k_BootstrapScenePath}");
            }

            File.Copy(sourcePath, k_BootstrapScenePath, true);
            AssetDatabase.ImportAsset(k_BootstrapScenePath);
            sb.AppendLine($"[BOOTSTRAP-SETUP] Cloned {sourcePath} -> {k_BootstrapScenePath}");

            // Step 2: Clean up BootstrapScene
            Scene bootstrapScene = EditorSceneManager.OpenScene(k_BootstrapScenePath, OpenSceneMode.Single);
            foreach (string objName in k_NonBootstrapObjects)
            {
                int count = DestroyAllNamed(bootstrapScene, objName);
                if (count > 0)
                {
                    sb.AppendLine($"[BOOTSTRAP-SETUP] [BootstrapScene] Removed {count}x {objName}");
                }
            }

            // Ensure BootstrapLoader exists in BootstrapScene
            GameObject loaderObj = GameObject.Find("BootstrapController");
            if (loaderObj == null)
            {
                loaderObj = new GameObject("BootstrapController");
            }
            if (loaderObj.GetComponent<BootstrapLoader>() == null)
            {
                loaderObj.AddComponent<BootstrapLoader>();
            }
            sb.AppendLine("[BOOTSTRAP-SETUP] [BootstrapScene] Added BootstrapController with BootstrapLoader");

            EditorSceneManager.MarkSceneDirty(bootstrapScene);
            EditorSceneManager.SaveScene(bootstrapScene);
            sb.AppendLine("[BOOTSTRAP-SETUP] Saved BootstrapScene");

            // Step 3: Strip persistent managers from GameplayScene
            Scene gameplayScene = EditorSceneManager.OpenScene(k_GameplayScenePath, OpenSceneMode.Single);
            foreach (string mgrName in k_PersistentManagerNames)
            {
                int count = DestroyAllNamed(gameplayScene, mgrName);
                sb.AppendLine($"[BOOTSTRAP-SETUP] [GameplayScene] Removed {count}x {mgrName}");
            }
            CleanDeadListeners(gameplayScene, sb);
            EditorSceneManager.MarkSceneDirty(gameplayScene);
            EditorSceneManager.SaveScene(gameplayScene);
            sb.AppendLine("[BOOTSTRAP-SETUP] Saved GameplayScene (0 persistent manager copies)");

            // Step 4: Strip persistent managers from MainMenuScene
            Scene menuScene = EditorSceneManager.OpenScene(k_MainMenuScenePath, OpenSceneMode.Single);
            foreach (string mgrName in k_PersistentManagerNames)
            {
                int count = DestroyAllNamed(menuScene, mgrName);
                sb.AppendLine($"[BOOTSTRAP-SETUP] [MainMenuScene] Removed {count}x {mgrName}");
            }
            CleanDeadListeners(menuScene, sb);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);
            sb.AppendLine("[BOOTSTRAP-SETUP] Saved MainMenuScene (0 persistent manager copies)");

            // Step 5: Update EditorBuildSettings
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(k_BootstrapScenePath, true),
                new EditorBuildSettingsScene(k_MainMenuScenePath, true),
                new EditorBuildSettingsScene(k_GameplayScenePath, true),
            };
            sb.AppendLine("[BOOTSTRAP-SETUP] Updated Build Settings order:");
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                sb.AppendLine($"[BOOTSTRAP-SETUP]   [{i}] {EditorBuildSettings.scenes[i].path} (enabled={EditorBuildSettings.scenes[i].enabled})");
            }

            Debug.Log(sb.ToString());
        }

        private static int DestroyAllNamed(Scene i_Scene, string i_Name)
        {
            List<GameObject> matches = new List<GameObject>();
            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name == i_Name)
                    {
                        matches.Add(candidate.gameObject);
                    }
                }
            }

            foreach (GameObject match in matches)
            {
                Object.DestroyImmediate(match);
            }

            return matches.Count;
        }

        private static void CleanDeadListeners(Scene i_Scene, StringBuilder io_Log)
        {
            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                foreach (UnityEngine.UI.Button button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                {
                    for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                    {
                        if (button.onClick.GetPersistentTarget(i) == null)
                        {
                            io_Log.AppendLine($"[BOOTSTRAP-SETUP] Removed dead persistent listener on {button.gameObject.name}: {button.onClick.GetPersistentMethodName(i)}");
                            UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
                        }
                    }
                }
            }
        }
    }
}
