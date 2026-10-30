using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Headless wiring validator for Star Vanguard.
    /// Run with:
    ///   Unity -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardValidator.ValidateAll
    /// Exits with code 0 when every check passes, 1 otherwise.
    /// </summary>
    public static class StarVanguardValidator
    {
        private const string k_PlayerPrefabPath = "Assets/Prefabs/Player_Prefab.prefab";
        private const string k_BossPrefabPath = "Assets/Prefabs/Boss_Prefab.prefab";
        private const string k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
        private const string k_GameplayScenePath = "Assets/Scenes/GameplayScene.unity";

        private static int s_Passed;
        private static int s_Failed;

        [MenuItem("Star Vanguard/Validate Project", priority = 50)]
        [MenuItem("Tools/Star Vanguard/Validate Project", priority = 50)]
        public static void ValidateAll()
        {
            s_Passed = 0;
            s_Failed = 0;

            Debug.Log("[STARVANGUARD-CHECK] ===== BEGIN VALIDATION =====");

            CheckPlayerPrefab();
            CheckBossPrefab();
            CheckMissingScripts();
            CheckBuildSettings();
            CheckGameplayScene();

            Debug.Log($"[STARVANGUARD-CHECK] SUMMARY passed={s_Passed} failed={s_Failed}");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(s_Failed == 0 ? 0 : 1);
            }
        }

        // ---------------------------------------------------------------- checks

        private static void CheckPlayerPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_PlayerPrefabPath);
            if (!Assert(prefab != null, "Player_Prefab loads")) return;

            Assert(prefab.GetComponent<PlayerController>() != null, "Player_Prefab has PlayerController");
            Assert(prefab.GetComponent<PlayerShooting>() != null, "Player_Prefab has PlayerShooting");
            Assert(prefab.GetComponent<PlayerStats>() != null, "Player_Prefab has PlayerStats");
            Assert(prefab.GetComponent<PlayerHitBox>() != null, "Player_Prefab has PlayerHitBox");
            Assert(prefab.GetComponent<PlayerRespawn>() != null, "Player_Prefab has PlayerRespawn");
            Assert(prefab.GetComponent<PlayerBombing>() != null, "Player_Prefab has PlayerBombing");
            Assert(prefab.CompareTag("Player"), "Player_Prefab is tagged Player");

            // P0-2: the overheat controller must exist and carry the REA spec constants
            WeaponHeatController heat = prefab.GetComponent<WeaponHeatController>();
            if (Assert(heat != null, "Player_Prefab has WeaponHeatController (overheat system wired)"))
            {
                SerializedObject so = new SerializedObject(heat);
                AssertFloat(so, "m_BaseHeatPerShot", 0.045f, "heat per shot = 0.045");
                AssertFloat(so, "m_HeatPerLevel", 0.005f, "heat per level = 0.005");
                AssertFloat(so, "m_CoolingRate", 0.35f, "cooling rate = 0.35/s");
                AssertFloat(so, "m_CoolDownDelay", 0.25f, "cool down delay = 0.25s");
            }

            // The shooting component must reference it explicitly
            PlayerShooting shooting = prefab.GetComponent<PlayerShooting>();
            if (Assert(shooting != null, "Player_Prefab has PlayerShooting (for heat reference)"))
            {
                SerializedObject so = new SerializedObject(shooting);
                SerializedProperty prop = so.FindProperty("m_HeatController");
                Assert(prop != null && prop.objectReferenceValue != null,
                    "PlayerShooting.m_HeatController is assigned");
                if (prop != null && prop.objectReferenceValue != null && heat != null)
                {
                    Assert(prop.objectReferenceValue == heat,
                        "PlayerShooting.m_HeatController points at the same component");
                }
            }
        }

        private static void CheckBossPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_BossPrefabPath);
            if (!Assert(prefab != null, "Boss_Prefab loads")) return;

            Assert(prefab.GetComponent<BossController>() != null, "Boss_Prefab has BossController");
            Assert(prefab.GetComponent<BossShooting>() != null, "Boss_Prefab has BossShooting");
            Assert(prefab.GetComponent<BossMover>() != null, "Boss_Prefab has BossMover");
        }

        private static void CheckMissingScripts()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            int checkedCount = 0;
            int brokenCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null) continue;

                checkedCount++;

                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour == null)
                    {
                        brokenCount++;
                        Debug.LogError($"[STARVANGUARD-CHECK] FAIL missing script reference inside {path}");
                        s_Failed++;
                    }
                }
            }

            Assert(checkedCount > 0, $"prefab scan found prefabs to check (n={checkedCount})");
            Assert(brokenCount == 0, $"no missing/broken MonoScript in prefabs (broken={brokenCount})");
        }

        private static void CheckBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Assert(scenes.Length >= 3, $"build settings contain >= 3 scenes (n={scenes.Length})");

            if (scenes.Length >= 3)
            {
                Assert(scenes[0].path.EndsWith("BootstrapScene.unity"),
                    $"build index 0 is BootstrapScene ({scenes[0].path})");
                Assert(scenes[1].path.EndsWith("MainMenuScene.unity"),
                    $"build index 1 is MainMenuScene ({scenes[1].path})");
                Assert(scenes[2].path.EndsWith("GameplayScene.unity"),
                    $"build index 2 is GameplayScene ({scenes[2].path})");
            }
        }

        private static void CheckGameplayScene()
        {
            var bootScene = EditorSceneManager.OpenScene(k_BootstrapScenePath, OpenSceneMode.Single);
            Assert(bootScene.IsValid(), "BootstrapScene opens");

            var gameScene = EditorSceneManager.OpenScene(k_GameplayScenePath, OpenSceneMode.Additive);
            Assert(gameScene.IsValid(), "GameplayScene opens additively");

            Assert(Object.FindAnyObjectByType<GameManager>() != null, "GameplayScene has GameManager");
            Assert(Object.FindAnyObjectByType<UIManager>() != null, "GameplayScene has UIManager");
            Assert(Object.FindAnyObjectByType<AudioManager>() != null, "GameplayScene has AudioManager");

            WaveDirector director = Object.FindAnyObjectByType<WaveDirector>();
            if (Assert(director != null, "GameplayScene has WaveDirector"))
            {
                SerializedObject so = new SerializedObject(director);
                Assert(so.FindProperty("m_EnemyPrefab").objectReferenceValue != null,
                    "WaveDirector.m_EnemyPrefab assigned");
                Assert(so.FindProperty("m_BossPrefab").objectReferenceValue != null,
                    "WaveDirector.m_BossPrefab assigned");
                Assert(so.FindProperty("m_Player").objectReferenceValue != null,
                    "WaveDirector.m_Player assigned");
            }

            PoolManager pool = Object.FindAnyObjectByType<PoolManager>();
            if (Assert(pool != null, "GameplayScene has PoolManager"))
            {
                SerializedObject so = new SerializedObject(pool);
                int enemyPool = so.FindProperty("m_EnemyPoolSize").intValue;
                Assert(enemyPool >= 40,
                    $"enemy pool >= 40 so Wave_Line (8x5) is never skipped (m_EnemyPoolSize={enemyPool})");
            }
        }

        // ---------------------------------------------------------------- helpers

        private static void AssertFloat(SerializedObject so, string propertyName, float expected, string label)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                s_Failed++;
                Debug.LogError($"[STARVANGUARD-CHECK] FAIL {label} (property {propertyName} not found)");
                return;
            }

            float actual = prop.floatValue;
            if (Mathf.Abs(actual - expected) <= 0.0001f)
            {
                s_Passed++;
                Debug.Log($"[STARVANGUARD-CHECK] PASS {label} (actual={actual})");
            }
            else
            {
                s_Failed++;
                Debug.LogError($"[STARVANGUARD-CHECK] FAIL {label} (expected={expected} actual={actual})");
            }
        }

        private static bool Assert(bool condition, string label)
        {
            if (condition)
            {
                s_Passed++;
                Debug.Log($"[STARVANGUARD-CHECK] PASS {label}");
            }
            else
            {
                s_Failed++;
                Debug.LogError($"[STARVANGUARD-CHECK] FAIL {label}");
            }

            return condition;
        }
    }
}
