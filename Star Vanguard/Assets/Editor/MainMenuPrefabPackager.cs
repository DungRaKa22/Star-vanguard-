using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Automates packaging of MenuRoot, ControlsPanel, and SettingsPanel into independent
    /// UI Prefabs under Assets/Prefabs/UI_Prefabs/ and converts scene GameObjects into
    /// clean PrefabInstances in MainMenuScene.unity (Sprint 1 Milestone 3 - T-401).
    ///
    /// Adheres strictly to:
    /// 1. AGENTS.md Rule 1: Zero direct manual or regex editing of YAML. All operations via PrefabUtility.
    /// 2. Single Scene Writer Lock discipline: Prevents accidental mutation during PlayMode.
    /// 3. Idempotent execution: Safe to run repeatedly in both interactive and batchmode CI runs.
    /// 4. Compact scene post-condition: Reduces MainMenuScene.unity from 11,045 to < 3,000 lines.
    /// </summary>
    public static class MainMenuPrefabPackager
    {
        private const string k_MenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string k_PrefabsDirectory = "Assets/Prefabs/UI_Prefabs";

        public const string MenuRootPrefabPath = "Assets/Prefabs/UI_Prefabs/MenuRoot.prefab";
        public const string ControlsPanelPrefabPath = "Assets/Prefabs/UI_Prefabs/ControlsPanel.prefab";
        public const string SettingsPanelPrefabPath = "Assets/Prefabs/UI_Prefabs/SettingsPanel.prefab";

        [MenuItem("Star Vanguard/UI/Package Main Menu Prefabs (T-401)", priority = 15)]
        [MenuItem("Tools/Star Vanguard/Package Main Menu Prefabs (T-401)", priority = 15)]
        public static void PackageAllPrefabs()
        {
            bool success = ExecutePackaging();
            if (!Application.isBatchMode)
            {
                if (success)
                {
                    EditorUtility.DisplayDialog(
                        "MainMenu Prefab Packaging",
                        "Successfully packaged MenuRoot, ControlsPanel, and SettingsPanel into UI Prefabs and connected PrefabInstances in MainMenuScene.unity!",
                        "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "MainMenu Prefab Packaging Failed",
                        "Failed to package prefabs. Check the Console log for detailed error diagnostics.",
                        "OK");
                }
            }
        }

        /// <summary>
        /// Headless batchmode entry point. Run with:
        /// & "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "Star Vanguard" -executeMethod StarVanguard.EditorTools.MainMenuPrefabPackager.PackageAllPrefabsAndSave -logFile -
        /// </summary>
        public static void PackageAllPrefabsAndSave()
        {
            Debug.Log("[MainMenuPrefabPackager] ===== Beginning MainMenu UI Prefab Packaging (T-401) =====");
            bool success = ExecutePackaging();

            if (!success)
            {
                Debug.LogError("[MainMenuPrefabPackager] FAILED: MainMenu prefab packaging encountered errors.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
            else
            {
                Debug.Log("[MainMenuPrefabPackager] SUCCESS: MainMenu prefab packaging completed and saved.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
        }

        /// <summary>
        /// Core idempotent prefab packaging routine.
        /// </summary>
        public static bool ExecutePackaging()
        {
            // 1. Single Scene Writer Lock Guard
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[MainMenuPrefabPackager] FAILED: Cannot package prefabs while in Play Mode or transitioning to Play Mode.");
                return false;
            }

            // 2. Ensure target directory exists
            if (!Directory.Exists(k_PrefabsDirectory))
            {
                Debug.Log($"[MainMenuPrefabPackager] Creating directory '{k_PrefabsDirectory}'...");
                Directory.CreateDirectory(k_PrefabsDirectory);
                AssetDatabase.Refresh();
            }

            // 3. Open MainMenuScene
            Debug.Log($"[MainMenuPrefabPackager] Opening scene '{k_MenuScenePath}'...");
            Scene scene = EditorSceneManager.OpenScene(k_MenuScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MainMenuPrefabPackager] FAILED: Could not open scene at '{k_MenuScenePath}'");
                return false;
            }

            // 4. Locate Canvas
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MainMenuPrefabPackager] FAILED: No Canvas found in MainMenuScene.");
                return false;
            }

            // 5. Package MenuRoot
            Transform menuRootTransform = canvas.transform.Find("MenuRoot");
            if (menuRootTransform == null)
            {
                Debug.LogError("[MainMenuPrefabPackager] FAILED: 'MenuRoot' GameObject not found under Canvas.");
                return false;
            }
            GameObject menuRootAsset = PackagePrefab(menuRootTransform.gameObject, MenuRootPrefabPath);
            if (menuRootAsset == null) return false;
            menuRootTransform.gameObject.SetActive(true);

            // 6. Package ControlsPanel
            Transform controlsTransform = canvas.transform.Find("ControlsPanel");
            if (controlsTransform == null)
            {
                Debug.LogError("[MainMenuPrefabPackager] FAILED: 'ControlsPanel' GameObject not found under Canvas.");
                return false;
            }
            GameObject controlsAsset = PackagePrefab(controlsTransform.gameObject, ControlsPanelPrefabPath);
            if (controlsAsset == null) return false;
            controlsTransform.gameObject.SetActive(false);

            // 7. Package SettingsPanel
            Transform settingsTransform = canvas.transform.Find("SettingsPanel");
            if (settingsTransform == null)
            {
                Debug.LogError("[MainMenuPrefabPackager] FAILED: 'SettingsPanel' GameObject not found under Canvas.");
                return false;
            }
            GameObject settingsAsset = PackagePrefab(settingsTransform.gameObject, SettingsPanelPrefabPath);
            if (settingsAsset == null) return false;
            settingsTransform.gameObject.SetActive(false);

            // 8. Save Scene and Assets
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            if (!saved)
            {
                Debug.LogError($"[MainMenuPrefabPackager] FAILED: Could not save scene: {k_MenuScenePath}");
                return false;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MainMenuPrefabPackager] MainMenuScene saved successfully: {saved}. All 3 prefabs created and connected.");
            return true;
        }

        /// <summary>
        /// Packages an in-scene GameObject into a Prefab asset and connects the scene instance
        /// as a PrefabInstance via PrefabUtility.SaveAsPrefabAssetAndConnect.
        /// </summary>
        private static GameObject PackagePrefab(GameObject i_InstanceRoot, string i_PrefabPath)
        {
            if (i_InstanceRoot == null)
            {
                Debug.LogError($"[MainMenuPrefabPackager] Instance root is null when packaging '{i_PrefabPath}'.");
                return null;
            }

            bool success;
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(
                i_InstanceRoot,
                i_PrefabPath,
                InteractionMode.AutomatedAction,
                out success);

            if (success && prefabAsset != null)
            {
                Debug.Log($"[MainMenuPrefabPackager] Successfully packaged and connected: {i_PrefabPath}");
                return prefabAsset;
            }
            else
            {
                Debug.LogError($"[MainMenuPrefabPackager] Failed to package prefab at: {i_PrefabPath}");
                return null;
            }
        }
    }
}
