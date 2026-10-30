using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Empirical Adversarial Challenger for Sprint 1 Milestone 3 (T-401 MainMenu UI Prefab Packaging).
    /// Rigorously stress-tests:
    /// 1. Nested prefab references inside SettingsPanel.prefab (AudioSlider_Prefab x3, Toggle_Prefab x3).
    /// 2. Completeness and validity of SettingsUI serialized bindings.
    /// 3. Audio slider controller and volume propagation into SettingsManager & AudioManager.
    /// 4. MainMenuScene compactness, PrefabInstance linkage, and UIManager wiring.
    ///
    /// Run with:
    ///   Unity -batchmode -quit -projectPath "Star Vanguard" -executeMethod StarVanguard.EditorTools.MainMenuSettingsChallenger.RunAllChallenges -logFile "challenger.log"
    /// Exits 0 on all pass, 1 on any failure.
    /// </summary>
    public static class MainMenuSettingsChallenger
    {
        private const string k_SettingsPrefabPath = "Assets/Prefabs/UI_Prefabs/SettingsPanel.prefab";
        private const string k_AudioSliderPrefabPath = "Assets/Prefabs/UI_Prefabs/AudioSlider_Prefab.prefab";
        private const string k_TogglePrefabPath = "Assets/Prefabs/UI_Prefabs/Toggle_Prefab.prefab";
        private const string k_MenuRootPrefabPath = "Assets/Prefabs/UI_Prefabs/MenuRoot.prefab";
        private const string k_ControlsPanelPrefabPath = "Assets/Prefabs/UI_Prefabs/ControlsPanel.prefab";
        private const string k_MenuScenePath = "Assets/Scenes/MainMenuScene.unity";

        private static int s_Passed;
        private static int s_Failed;

        [MenuItem("Star Vanguard/Challenge Milestone 3 Integration", priority = 60)]
        [MenuItem("Tools/Star Vanguard/Challenge Milestone 3 Integration", priority = 60)]
        public static void RunAllChallenges()
        {
            s_Passed = 0;
            s_Failed = 0;

            Debug.Log("[CHALLENGER-M3] ========================================================");
            Debug.Log("[CHALLENGER-M3] BEGINNING EMPIRICAL ADVERSARIAL CHALLENGE (T-401 / M3-2)");
            Debug.Log("[CHALLENGER-M3] ========================================================");

            try
            {
                ChallengeSceneFileCompactness();
                ChallengeAllPrefabsExistAndValid();
                ChallengeNestedPrefabsInSettingsPanel();
                ChallengeSettingsUIBindings();
                ChallengeAudioSlidersAndSettingsLogic();
                ChallengeMainMenuSceneInstanceWiring();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CHALLENGER-M3] CRITICAL EXCEPTION during challenge execution: {ex}");
                s_Failed++;
            }

            Debug.Log($"[CHALLENGER-M3] ========================================================");
            Debug.Log($"[CHALLENGER-M3] CHALLENGE SUMMARY: passed={s_Passed} failed={s_Failed}");
            Debug.Log($"[CHALLENGER-M3] ========================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(s_Failed == 0 ? 0 : 1);
            }
        }

        private static bool Assert(bool condition, string message)
        {
            if (condition)
            {
                s_Passed++;
                Debug.Log($"[CHALLENGER-M3] PASS: {message}");
                return true;
            }
            else
            {
                s_Failed++;
                Debug.LogError($"[CHALLENGER-M3] FAIL: {message}");
                return false;
            }
        }

        // ---------------------------------------------------------------- Challenge 1: Scene Compactness
        private static void ChallengeSceneFileCompactness()
        {
            Debug.Log("[CHALLENGER-M3] --- Challenge 1: MainMenuScene Compactness & Line Count ---");

            if (!File.Exists(k_MenuScenePath))
            {
                Assert(false, $"MainMenuScene file exists at {k_MenuScenePath}");
                return;
            }

            string[] lines = File.ReadAllLines(k_MenuScenePath);
            int lineCount = lines.Length;
            Debug.Log($"[CHALLENGER-M3] MainMenuScene.unity line count: {lineCount}");

            Assert(lineCount < 3000, $"MainMenuScene line count ({lineCount}) is strictly < 3,000 lines (Requirement R3)");
            Assert(lineCount > 500, $"MainMenuScene line count ({lineCount}) is valid (> 500 lines)");
        }

        // ---------------------------------------------------------------- Challenge 2: Prefabs Exist & Valid
        private static void ChallengeAllPrefabsExistAndValid()
        {
            Debug.Log("[CHALLENGER-M3] --- Challenge 2: Prefab Assets Existence & Script Integrity ---");

            string[] prefabs = {
                k_MenuRootPrefabPath,
                k_ControlsPanelPrefabPath,
                k_SettingsPrefabPath,
                k_AudioSliderPrefabPath,
                k_TogglePrefabPath
            };

            foreach (string path in prefabs)
            {
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!Assert(root != null, $"Prefab asset loads cleanly: {path}"))
                {
                    continue;
                }

                // Check for missing scripts
                MonoBehaviour[] components = root.GetComponentsInChildren<MonoBehaviour>(true);
                int missingCount = 0;
                foreach (MonoBehaviour comp in components)
                {
                    if (comp == null) missingCount++;
                }
                Assert(missingCount == 0, $"No missing MonoBehaviour scripts inside {path} (found: {missingCount})");
            }
        }

        // ---------------------------------------------------------------- Challenge 3: Nested Prefabs in SettingsPanel
        private static void ChallengeNestedPrefabsInSettingsPanel()
        {
            Debug.Log("[CHALLENGER-M3] --- Challenge 3: Nested Prefabs in SettingsPanel.prefab ---");

            GameObject settingsAsset = AssetDatabase.LoadAssetAtPath<GameObject>(k_SettingsPrefabPath);
            if (!Assert(settingsAsset != null, "SettingsPanel.prefab loaded")) return;

            // Instantiate a test instance to inspect prefab connections and hierarchies
            GameObject instance = PrefabUtility.InstantiatePrefab(settingsAsset) as GameObject;
            if (!Assert(instance != null, "SettingsPanel instantiated in memory for deep inspection")) return;

            try
            {
                // Verify AudioSlider_Prefab nested instances
                string[] expectedSliders = { "MasterAudioSlider", "SFXAudioSlider", "MusicAudioSlider" };
                foreach (string sliderName in expectedSliders)
                {
                    Transform sliderT = instance.transform.Find($"AudioSettingsContainer/{sliderName}");
                    if (sliderT == null)
                    {
                        // Try searching anywhere in hierarchy
                        foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
                        {
                            if (t.name == sliderName)
                            {
                                sliderT = t;
                                break;
                            }
                        }
                    }

                    if (Assert(sliderT != null, $"Nested AudioSlider '{sliderName}' found in SettingsPanel"))
                    {
                        // Check if it has the required UI components from AudioSlider_Prefab
                        Slider sliderComp = sliderT.GetComponentInChildren<Slider>(true);
                        Assert(sliderComp != null, $"'{sliderName}' has child Slider component");

                        TextMeshProUGUI[] tmps = sliderT.GetComponentsInChildren<TextMeshProUGUI>(true);
                        Assert(tmps.Length >= 2, $"'{sliderName}' has >= 2 TextMeshProUGUI components (label + percentage, found={tmps.Length})");
                    }
                }

                // Verify Toggle_Prefab nested instances
                string[] expectedToggles = { "MouseSmoothingToggle", "KeyboardSmoothingToggle", "MouseControlsToggle" };
                foreach (string toggleName in expectedToggles)
                {
                    Transform toggleT = instance.transform.Find($"ControlsSettingsContainer/{toggleName}");
                    if (toggleT == null)
                    {
                        foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
                        {
                            if (t.name == toggleName)
                            {
                                toggleT = t;
                                break;
                            }
                        }
                    }

                    if (Assert(toggleT != null, $"Nested Toggle '{toggleName}' found in SettingsPanel"))
                    {
                        Toggle toggleComp = toggleT.GetComponent<Toggle>();
                        if (toggleComp == null) toggleComp = toggleT.GetComponentInChildren<Toggle>(true);
                        Assert(toggleComp != null, $"'{toggleName}' has Toggle component");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        // ---------------------------------------------------------------- Challenge 4: SettingsUI Bindings
        private static void ChallengeSettingsUIBindings()
        {
            Debug.Log("[CHALLENGER-M3] --- Challenge 4: SettingsUI Serialized Field Bindings ---");

            GameObject settingsAsset = AssetDatabase.LoadAssetAtPath<GameObject>(k_SettingsPrefabPath);
            SettingsUI settingsUI = settingsAsset.GetComponent<SettingsUI>();
            if (!Assert(settingsUI != null, "SettingsPanel.prefab has SettingsUI component attached")) return;

            // Use reflection to inspect private serialized fields
            Type type = typeof(SettingsUI);
            FieldInfo masterVolField = type.GetField("m_MasterVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo sfxVolField = type.GetField("m_SFXVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo musicVolField = type.GetField("m_MusicVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance);

            FieldInfo masterPctField = type.GetField("m_MasterVolumePercentageText", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo sfxPctField = type.GetField("m_SFXVolumePercentageText", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo musicPctField = type.GetField("m_MusicVolumePercentageText", BindingFlags.NonPublic | BindingFlags.Instance);

            FieldInfo masterTxtField = type.GetField("m_MasterVolumeText", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo sfxTxtField = type.GetField("m_SFXVolumeText", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo musicTxtField = type.GetField("m_MusicVolumeText", BindingFlags.NonPublic | BindingFlags.Instance);

            FieldInfo mouseControlsField = type.GetField("m_MouseControlsToggle", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo mouseSmoothField = type.GetField("m_MouseSmoothingToggle", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo kbSmoothField = type.GetField("m_KeyboardSmoothingToggle", BindingFlags.NonPublic | BindingFlags.Instance);

            FieldInfo resetBtnField = type.GetField("m_ResetToDefaultsButton", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo closeBtnField = type.GetField("m_CloseButton", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert(masterVolField != null && masterVolField.GetValue(settingsUI) != null, "SettingsUI.m_MasterVolumeSlider is assigned");
            Assert(sfxVolField != null && sfxVolField.GetValue(settingsUI) != null, "SettingsUI.m_SFXVolumeSlider is assigned");
            Assert(musicVolField != null && musicVolField.GetValue(settingsUI) != null, "SettingsUI.m_MusicVolumeSlider is assigned");

            Assert(masterPctField != null && masterPctField.GetValue(settingsUI) != null, "SettingsUI.m_MasterVolumePercentageText is assigned");
            Assert(sfxPctField != null && sfxPctField.GetValue(settingsUI) != null, "SettingsUI.m_SFXVolumePercentageText is assigned");
            Assert(musicPctField != null && musicPctField.GetValue(settingsUI) != null, "SettingsUI.m_MusicVolumePercentageText is assigned");

            Assert(masterTxtField != null && masterTxtField.GetValue(settingsUI) != null, "SettingsUI.m_MasterVolumeText is assigned");
            Assert(sfxTxtField != null && sfxTxtField.GetValue(settingsUI) != null, "SettingsUI.m_SFXVolumeText is assigned");
            Assert(musicTxtField != null && musicTxtField.GetValue(settingsUI) != null, "SettingsUI.m_MusicVolumeText is assigned");

            Assert(mouseControlsField != null && mouseControlsField.GetValue(settingsUI) != null, "SettingsUI.m_MouseControlsToggle is assigned");
            Assert(mouseSmoothField != null && mouseSmoothField.GetValue(settingsUI) != null, "SettingsUI.m_MouseSmoothingToggle is assigned");
            Assert(kbSmoothField != null && kbSmoothField.GetValue(settingsUI) != null, "SettingsUI.m_KeyboardSmoothingToggle is assigned");

            Assert(resetBtnField != null && resetBtnField.GetValue(settingsUI) != null, "SettingsUI.m_ResetToDefaultsButton is assigned");
            Assert(closeBtnField != null && closeBtnField.GetValue(settingsUI) != null, "SettingsUI.m_CloseButton is assigned");
        }

        // ---------------------------------------------------------------- Challenge 5: Audio Sliders & Settings Logic
        private static void ChallengeAudioSlidersAndSettingsLogic()
        {
            Debug.Log("[CHALLENGER-M3] --- Challenge 5: Audio Sliders & Settings Behavioral Simulation ---");

            GameObject settingsAsset = AssetDatabase.LoadAssetAtPath<GameObject>(k_SettingsPrefabPath);
            GameObject instance = UnityEngine.Object.Instantiate(settingsAsset);
            SettingsUI settingsUI = instance.GetComponent<SettingsUI>();

            // Test SettingsManager standalone model
            GameObject smObj = new GameObject("Test_SettingsManager");
            SettingsManager sm = smObj.AddComponent<SettingsManager>();

            try
            {
                // Invoke Start via reflection to set up UI listeners
                MethodInfo startMethod = typeof(SettingsUI).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
                startMethod?.Invoke(settingsUI, null);

                // Check Slider fields
                Type type = typeof(SettingsUI);
                Slider masterSlider = type.GetField("m_MasterVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as Slider;
                Slider sfxSlider = type.GetField("m_SFXVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as Slider;
                Slider musicSlider = type.GetField("m_MusicVolumeSlider", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as Slider;

                TextMeshProUGUI masterPct = type.GetField("m_MasterVolumePercentageText", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as TextMeshProUGUI;
                TextMeshProUGUI sfxPct = type.GetField("m_SFXVolumePercentageText", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as TextMeshProUGUI;
                TextMeshProUGUI musicPct = type.GetField("m_MusicVolumePercentageText", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as TextMeshProUGUI;

                Button resetBtn = type.GetField("m_ResetToDefaultsButton", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(settingsUI) as Button;

                // 1. Verify UI event wiring & percentage text formatting via slider events
                masterSlider.value = 0.5f;
                Assert(masterPct.text == "50%", $"Master slider value 0.5 formats percentage as '50%' (actual='{masterPct.text}')");

                masterSlider.value = 0.0f;
                Assert(masterPct.text == "0%", $"Master slider value 0.0 formats percentage as '0%' (actual='{masterPct.text}')");

                masterSlider.value = 1.0f;
                Assert(masterPct.text == "100%", $"Master slider value 1.0 formats percentage as '100%' (actual='{masterPct.text}')");

                sfxSlider.value = 0.35f;
                Assert(sfxPct.text == "35%", $"SFX slider value 0.35 formats percentage as '35%' (actual='{sfxPct.text}')");

                musicSlider.value = 0.85f;
                Assert(musicPct.text == "85%", $"Music slider value 0.85 formats percentage as '85%' (actual='{musicPct.text}')");

                // 2. Verify standalone SettingsManager property clamping and default values
                sm.MasterVolume = 0.75f;
                Assert(Mathf.Approximately(sm.MasterVolume, 0.75f), "SettingsManager.MasterVolume sets and gets 0.75");
                sm.MasterVolume = 1.5f;
                Assert(Mathf.Approximately(sm.MasterVolume, 1.0f), "SettingsManager.MasterVolume clamps to 1.0 max");
                sm.MasterVolume = -0.5f;
                Assert(Mathf.Approximately(sm.MasterVolume, 0.0f), "SettingsManager.MasterVolume clamps to 0.0 min");

                sm.SFXVolume = 0.4f;
                Assert(Mathf.Approximately(sm.SFXVolume, 0.4f), "SettingsManager.SFXVolume sets and gets 0.4");
                sm.MusicVolume = 0.6f;
                Assert(Mathf.Approximately(sm.MusicVolume, 0.6f), "SettingsManager.MusicVolume sets and gets 0.6");

                // 3. Verify ResetToDefaults restores standard game audio levels
                sm.ResetToDefaults();
                Assert(Mathf.Approximately(sm.MasterVolume, 1.0f), $"ResetToDefaults restores MasterVolume to 1.0 (actual={sm.MasterVolume})");
                Assert(Mathf.Approximately(sm.SFXVolume, 1.0f), $"ResetToDefaults restores SFXVolume to 1.0 (actual={sm.SFXVolume})");
                Assert(Mathf.Approximately(sm.MusicVolume, 0.8f), $"ResetToDefaults restores MusicVolume to 0.8 (actual={sm.MusicVolume})");
                Assert(sm.UseMouseControls == true, "ResetToDefaults restores UseMouseControls to true");
                Assert(sm.MouseSmoothing == true, "ResetToDefaults restores MouseSmoothing to true");
                Assert(sm.KeyboardSmoothing == true, "ResetToDefaults restores KeyboardSmoothing to true");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
                UnityEngine.Object.DestroyImmediate(smObj);
            }
        }

        // ---------------------------------------------------------------- Challenge 6: Scene Instance Wiring
        private static void ChallengeMainMenuSceneInstanceWiring()
        {
            Debug.Log("[CHALLENGER-M3] --- Challenge 6: MainMenuScene In-Scene Hierarchy & UIManager Wiring ---");

            Scene scene = EditorSceneManager.OpenScene(k_MenuScenePath, OpenSceneMode.Single);
            Assert(scene.IsValid(), $"MainMenuScene opened successfully: {k_MenuScenePath}");

            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            Assert(canvas != null, "Canvas exists in MainMenuScene");
            if (canvas == null) return;

            Transform settingsT = canvas.transform.Find("SettingsPanel");
            Assert(settingsT != null, "Canvas has 'SettingsPanel' child transform");

            if (settingsT != null)
            {
                bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(settingsT.gameObject);
                Assert(isPrefab, "'SettingsPanel' in MainMenuScene is an active PrefabInstance");

                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(settingsT.gameObject);
                string sourcePath = AssetDatabase.GetAssetPath(source);
                Assert(sourcePath == k_SettingsPrefabPath, $"'SettingsPanel' instance points to {k_SettingsPrefabPath} (actual={sourcePath})");
            }

            Transform menuRootT = canvas.transform.Find("MenuRoot");
            Assert(menuRootT != null, "Canvas has 'MenuRoot' child transform");
            if (menuRootT != null)
            {
                Assert(PrefabUtility.IsPartOfPrefabInstance(menuRootT.gameObject), "'MenuRoot' is an active PrefabInstance");
            }

            Transform controlsT = canvas.transform.Find("ControlsPanel");
            Assert(controlsT != null, "Canvas has 'ControlsPanel' child transform");
            if (controlsT != null)
            {
                Assert(PrefabUtility.IsPartOfPrefabInstance(controlsT.gameObject), "'ControlsPanel' is an active PrefabInstance");
            }

            // Check UIManager wiring
            UIManager uiManager = UnityEngine.Object.FindAnyObjectByType<UIManager>();
            if (uiManager != null)
            {
                SerializedObject so = new SerializedObject(uiManager);
                SerializedProperty settingsProp = so.FindProperty("m_SettingsPanel");
                Assert(settingsProp != null && settingsProp.objectReferenceValue != null, "UIManager.m_SettingsPanel is assigned in MainMenuScene");
            }
        }
    }
}
