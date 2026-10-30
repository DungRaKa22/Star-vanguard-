# Handoff Report: Bootstrap Scene Duplicate Removal Strategy (T-204c)

> **Summary**: `BootstrapScene.unity` contains redundant `Main Camera` and `Global Light 2D` objects left behind during scene cloning. Removing them via an automated, batchmode-safe Editor script (`StarVanguardBootstrapCleanup.cs`) and updating `StarVanguardBootstrapSetup.cs` completely resolves duplicate AudioListener errors, dual-camera depth conflicts, and 200% blown-out URP 2D lighting during additive loading without affecting persistent managers.

---

## 1. Observation

### 1.1 Object Hierarchy in `BootstrapScene.unity`
- **File**: `Star Vanguard/Assets/Scenes/BootstrapScene.unity` (945 lines total)
- **SceneRoots** (lines 937–945):
  ```yaml
  SceneRoots:
    m_ObjectHideFlags: 0
    m_Roots:
    - {fileID: 519420032} # Main Camera Transform
    - {fileID: 619394802} # Global Light 2D Transform
    - {fileID: 760890376} # Managers Transform
    - {fileID: 582285522} # BootstrapController Transform
  ```
- **Redundant Object 1: `Main Camera`** (lines 122–273):
  - GameObject ID: `519420028`, Name: `Main Camera`, Tag: `MainCamera`, Active: `1`, Father: `{fileID: 0}`, Children: `[]`.
  - Attached Components:
    1. `Transform &519420032` (line 245)
    2. `Camera &519420031` (line 194) — orthographic, depth `-1`
    3. `AudioListener &519420029` (line 142) — enabled: `1`
    4. `UniversalAdditionalCameraData &519420030` (line 150, GUID `a79441f348de89743a2939f4d699eac1`)
    5. `FixedAspectCamera &519420033` (line 260, GUID `a12a5cad668220944bb31561fd870019`)
- **Redundant Object 2: `Global Light 2D`** (lines 317–417):
  - GameObject ID: `619394800`, Name: `Global Light 2D`, Tag: `Untagged`, Active: `1`, Father: `{fileID: 0}`, Children: `[]`.
  - Attached Components:
    1. `Transform &619394802` (line 403)
    2. `Light2D &619394801` (line 334, GUID `073797afb82c5a1438f328866b10b3f0`) — `m_LightType: 4` (Global), `m_Intensity: 1`
- **Persistent Objects in `BootstrapScene.unity`**:
  - `Managers &760890375` (line 468) containing:
    - `SettingsManager &638974996` (line 418)
    - `PoolManager &1518732596` (line 504)
    - `LeaderboardManager &1900088710` (line 560)
    - `AudioManager &1941854439` (line 604, with 2 AudioSources SFX & Music)
    - `GameManager &2080025536` (line 864, with `GameManager` and `PickupManager`)
  - `BootstrapController &582285520` (line 273) with `BootstrapLoader` (MonoBehaviour `582285521`).

### 1.2 Cameras and Lights in Other Scenes
- **`Star Vanguard/Assets/Scenes/MainMenuScene.unity`**:
  - Line 3336: `m_Name: Main Camera` (with Camera, AudioListener, UniversalAdditionalCameraData, FixedAspectCamera)
  - Line 3580: `m_Name: Global Light 2D` (with Light2D Global, intensity 1.0)
- **`Star Vanguard/Assets/Scenes/GameplayScene.unity`**:
  - Line 1736: `m_Name: Main Camera` (with Camera, AudioListener, UniversalAdditionalCameraData, FixedAspectCamera)
  - Line 1921: `m_Name: Global Light 2D` (with Light2D Global, intensity 1.0)

### 1.3 Origin of the Redundant Objects in `StarVanguardBootstrapSetup.cs`
- **File**: `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs`
  - Lines 33–53:
    ```csharp
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
    ```
  - Lines 61–82: `BootstrapScene.unity` was cloned from `MainMenuScene` (`SceneBackups/MainMenuScene.unity.bak` or `Assets/Scenes/MainMenuScene.unity`), and only objects listed in `k_NonBootstrapObjects` were deleted via `DestroyAllNamed()`. `"Main Camera"` and `"Global Light 2D"` were not in `k_NonBootstrapObjects`, so they were preserved in `BootstrapScene.unity`.

### 1.4 Runtime Additive Loading Behavior in `BootstrapLoader.cs`
- **File**: `Star Vanguard/Assets/Scripts/BootstrapLoader.cs`
  - Lines 21–28:
    ```csharp
    private void Start()
    {
        // If BootstrapScene was the primary entry scene, transition directly to the Main Menu
        if (SceneManager.GetActiveScene().name == k_BootstrapSceneName)
        {
            SceneManager.LoadScene(k_MainMenuSceneName);
        }
    }
    ```
  - Lines 37–48:
    ```csharp
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
    ```

### 1.5 Persistent Manager Singleton Lifecycle in `Singleton.cs`
- **File**: `Star Vanguard/Assets/Scripts/Singleton.cs`
  - Lines 73–82:
    ```csharp
    _instance = this as T;
    if (transform.parent != null)
    {
        transform.SetParent(null);
    }

    if (PersistAcrossScenes)
    {
        DontDestroyOnLoad(gameObject);
    }
    ```
  - Lines 9–12 & `WaveDirector.cs:6–11`: `WaveDirector` is decorated with `[SceneBoundSingleton]` and has `PersistAcrossScenes => false`. It lives exclusively in `GameplayScene.unity`.

### 1.6 Existing Editor Cleanup Tool Pattern in `StarVanguardCoopCleanup.cs`
- **File**: `Star Vanguard/Assets/Editor/StarVanguardCoopCleanup.cs`
  - Demonstrates the approved project standard for scene modifications:
    1. Uses `[MenuItem("Tools/Star Vanguard/...")]`
    2. Safeguards against running in Play Mode (`EditorApplication.isPlayingOrWillChangePlaymode`)
    3. Opens target scene via `EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)`
    4. Locates candidate GameObjects and applies `Undo.DestroyObjectImmediate(go)`
    5. Marks scene dirty (`EditorSceneManager.MarkSceneDirty`) and saves (`EditorSceneManager.SaveScene`)
    6. Is completely idempotent (reports 0 if already clean)

---

## 2. Logic Chain

1. **Root Cause of Duplicate Camera and Lighting Bugs**:
   - Observation 1.3 shows that `BootstrapScene.unity` was generated by cloning `MainMenuScene.unity` and stripping only objects named in `k_NonBootstrapObjects`.
   - Because `k_NonBootstrapObjects` omitted `"Main Camera"` and `"Global Light 2D"`, both root objects were copied into `BootstrapScene.unity` (Observation 1.1).
   - In Editor workflow, entering Play Mode from `MainMenuScene` or `GameplayScene` invokes `EnsureBootstrapInEditor()`, loading `BootstrapScene` additively (Observation 1.4).
   - Because the active scene is `MainMenuScene` or `GameplayScene`, `BootstrapLoader.Start()`'s condition `SceneManager.GetActiveScene().name == k_BootstrapSceneName` evaluates to false, so `BootstrapScene` remains loaded in memory additively.
   - Both `MainMenuScene` and `GameplayScene` already possess their own `Main Camera` and `Global Light 2D` (Observation 1.2).
   - Consequently, the runtime environment contains:
     - 2 active `AudioListener` components -> triggers Unity warning: *"There are 2 audio listeners in the scene. Please ensure there is always exactly one audio listener in the scene."*
     - 2 active `Camera` components -> causes depth-buffer competition, redundant rendering, and `Camera.main` resolution ambiguity.
     - 2 active URP `Light2D` components of type Global -> because URP 2D lights are additive, 1.0 + 1.0 intensity yields 200% illumination, blowing out all sprites and 2D materials.

2. **Zero Need for Camera and Light in `BootstrapScene`**:
   - In a standalone player or batch smoke test, `BootstrapScene` is loaded at Build Index 0.
   - On frame 1, `BootstrapLoader.Start()` calls `SceneManager.LoadScene(k_MainMenuSceneName)` in single mode (Observation 1.4), destroying `BootstrapScene` immediately.
   - Persistent managers (`GameManager`, `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager`) unparent themselves in `Awake()` and call `DontDestroyOnLoad(gameObject)` (Observation 1.5), allowing them to persist into `MainMenuScene`.
   - `BootstrapScene` has no visual UI or gameplay rendering of its own; it serves purely as an initialization container.
   - Therefore, neither `Main Camera` nor `Global Light 2D` is needed in `BootstrapScene`.

3. **Complete Independence of Persistent Managers**:
   - Inspection of `BootstrapScene.unity` (Observation 1.1) reveals that `Main Camera` and `Global Light 2D` are distinct root GameObjects with no children (`m_Father: 0`, `m_Children: []`).
   - None of the persistent managers under `Managers` (`GameManager`, `PickupManager`, `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager`) or `BootstrapController` hold any serialized field references to `Main Camera` or `Global Light 2D`.
   - `WaveDirector` is scene-bound to `GameplayScene` and does not exist in `BootstrapScene` (Observation 1.5).
   - Destroying `Main Camera` and `Global Light 2D` leaves `Managers` and `BootstrapController` completely undisturbed.

4. **Compliance with AGENTS.md Rules**:
   - AGENTS.md Rule 1 prohibits manual YAML editing of scene files (`.unity`).
   - Implementing an automated Editor script following the proven architecture of `StarVanguardCoopCleanup.cs` (Observation 1.6) allows Unity's `EditorSceneManager` and `Undo` APIs to safely remove the objects and regenerate the scene YAML without dangling references.

---

## 3. Caveats

1. **Single Scene Writer Lock**:
   - Tasks T-204a (`MainMenuScene.unity`), T-204b (`MainMenuScene.unity` and `GameplayScene.unity`), and T-204c (`BootstrapScene.unity`) all operate on scenes. To avoid file lock contention in Unity, T-204c should be executed sequentially after acquiring the scene writer lock for `BootstrapScene.unity`.
2. **Batchmode UI Dialogs**:
   - In automated / CI runs (`-batchmode`), calls to `EditorUtility.DisplayDialog` can block execution if not guarded by `if (!Application.isBatchMode)`. The proposed script explicitly handles batchmode vs. interactive editor.
3. **Preventing Future Setup Regressions**:
   - If `StarVanguardBootstrapSetup.ExecuteSetup()` is ever re-run in the future without updating its `k_NonBootstrapObjects` array, it would re-clone from backup and recreate `Main Camera` and `Global Light 2D`. Therefore, updating `k_NonBootstrapObjects` in `StarVanguardBootstrapSetup.cs` must accompany the cleanup.

---

## 4. Conclusion

1. **Direct Action Required for T-204c**:
   - Create a dedicated Editor cleanup tool `Assets/Editor/StarVanguardBootstrapCleanup.cs` with MenuItem `Tools/Star Vanguard/1c. Remove Duplicate Camera and Light from BootstrapScene and Save` and public static method `RemoveDuplicatesAndSave()`.
   - Update `k_NonBootstrapObjects` in `Assets/Editor/StarVanguardBootstrapSetup.cs` to include `"Main Camera"` and `"Global Light 2D"`.
   - Execute `RemoveDuplicatesAndSave()` via Unity Editor menu or batchmode command.
2. **Expected Scene State Post-Cleanup**:
   - `BootstrapScene.unity` will contain only 2 root GameObjects: `Managers` (ID `760890375`) and `BootstrapController` (ID `582285520`).
   - `Main Camera` (ID `519420028`) and `Global Light 2D` (ID `619394800`) are permanently deleted.
   - Persistent managers (`GameManager`, `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager`) remain completely intact.

### Proposed Code Implementation

#### 1. New File: `Star Vanguard/Assets/Editor/StarVanguardBootstrapCleanup.cs`
```csharp
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Editor cleanup tool for T-204c:
    /// Removes duplicate 'Main Camera' and 'Global Light 2D' from BootstrapScene.unity.
    ///
    /// These objects were leftover copies from MainMenuScene during initial bootstrap setup.
    /// When BootstrapScene is loaded additively in Editor, they cause:
    /// 1. "There are 2 audio listeners in the scene" warning.
    /// 2. Duplicate Main Cameras competing for rendering.
    /// 3. Double URP 2D Global Light intensity (200% blown-out lighting).
    ///
    /// Idempotent: safe to run multiple times.
    /// </summary>
    public static class StarVanguardBootstrapCleanup
    {
        private const string k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
        private static readonly string[] k_DuplicateObjectNames = { "Main Camera", "Global Light 2D" };

        [MenuItem("Tools/Star Vanguard/1c. Remove Duplicate Camera and Light from BootstrapScene and Save", priority = 3)]
        public static void RemoveDuplicatesAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Exit Play Mode first",
                        "Stop Play Mode before cleaning BootstrapScene.",
                        "OK");
                }
                else
                {
                    Debug.LogError("[Bootstrap Cleanup] Cannot clean scene while in Play Mode.");
                }
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.path != k_BootstrapScenePath)
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }
                active = EditorSceneManager.OpenScene(k_BootstrapScenePath, OpenSceneMode.Single);
            }

            int removed = removeFromScene(active);

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(active);
                EditorSceneManager.SaveScene(active);
                Debug.Log($"[Bootstrap Cleanup] Removed {removed} duplicate object(s) from BootstrapScene and saved it.");
            }
            else
            {
                Debug.Log("[Bootstrap Cleanup] BootstrapScene has no duplicate objects. Nothing to save.");
            }
        }

        private static int removeFromScene(Scene i_Scene)
        {
            int removed = 0;
            List<GameObject> toDestroy = new List<GameObject>();

            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                if (root == null) continue;

                if (System.Array.IndexOf(k_DuplicateObjectNames, root.name) >= 0)
                {
                    toDestroy.Add(root);
                }
            }

            foreach (GameObject go in toDestroy)
            {
                Debug.Log($"[Bootstrap Cleanup] Destroying '{go.name}' in scene '{i_Scene.name}'.");
                Undo.DestroyObjectImmediate(go);
                removed++;
            }

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(i_Scene);
            }

            return removed;
        }
    }
}
```

#### 2. Update to `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs` (lines 33–53)
Add `"Main Camera"` and `"Global Light 2D"` to `k_NonBootstrapObjects`:
```csharp
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
            "InterWaveMessagePanel",
            "Main Camera",
            "Global Light 2D"
        };
```

---

## 5. Verification Method

### 5.1 Automated Execution via Unity Batchmode
The cleanup can be executed headlessly without human intervention:
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardBootstrapCleanup.RemoveDuplicatesAndSave -logFile -
```
- **Expected Console Output**:
  `[Bootstrap Cleanup] Destroying 'Main Camera' in scene 'BootstrapScene'.`
  `[Bootstrap Cleanup] Destroying 'Global Light 2D' in scene 'BootstrapScene'.`
  `[Bootstrap Cleanup] Removed 2 duplicate object(s) from BootstrapScene and saved it.`
- **Idempotence Check**: Running the command a second time outputs:
  `[Bootstrap Cleanup] BootstrapScene has no duplicate objects. Nothing to save.`

### 5.2 Scene YAML Inspection (Gate G3 / Read-Only Audit)
Inspect `Assets/Scenes/BootstrapScene.unity` after execution:
1. `m_Name: Main Camera` must return 0 occurrences in `BootstrapScene.unity`.
2. `m_Name: Global Light 2D` must return 0 occurrences in `BootstrapScene.unity`.
3. `SceneRoots` must contain exactly 2 fileIDs:
   - `{fileID: 760890376}` (`Managers`)
   - `{fileID: 582285522}` (`BootstrapController`)
4. Verify persistent managers under `Managers` (`GameManager`, `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager`) remain completely intact.

### 5.3 Gate G1 & Gate G3 Verification
1. **Gate G1 (Compile Check)**:
   Ensure Unity compiles cleanly with 0 errors and no new warnings.
2. **Gate G3 (Static Project Audit)**:
   ```powershell
   python Tools/check_project.py
   ```
   - Must output `0 FAIL`.
   - `[ OK ] build_scenes: Build order dung: BootstrapScene.unity -> MainMenuScene.unity -> GameplayScene.unity`.

### 5.4 Gate G2 (Play Mode Smoke Test & Log Verification)
Run the Play Mode smoke test:
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile -
```
- **Invalidation Condition**:
  - If Unity console contains `"There are 2 audio listeners in the scene"`, T-204c has failed.
  - If `cameras.Length > 1` during `DumpVisualHierarchy()`, T-204c has failed.
  - If smoke test reports any failure or timeout, investigate regressions.
