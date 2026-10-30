# Handoff Report — Explorer M1-1: Co-op UI Cleanup Implementation Plan (T-204a)

## 1. Observation

### 1.1 Tooling: `StarVanguardCoopCleanup.cs`
- **File**: `Star Vanguard/Assets/Editor/StarVanguardCoopCleanup.cs` (131 lines total)
  - **Namespace**: `StarVanguard.EditorTools`
  - **Class**: `public static class StarVanguardCoopCleanup`
  - **Target scene path**: line 21: `private const string k_MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";`
  - **Target object names**: line 24: `private static readonly string[] k_LegacyCoopNames = { "CoopPanel", "CoopCard", "CoopButton" };`
  - **Menu item**: line 56:
    ```csharp
    [MenuItem("Tools/Star Vanguard/1b. Remove Legacy Co-op UI from MainMenuScene and Save", priority = 2)]
    public static void RemoveFromMainMenuSceneAndSave()
    ```
  - **Scene Loading & Saving logic** (lines 68–86):
    ```csharp
    Scene active = SceneManager.GetActiveScene();
    if (active.path != k_MainMenuScenePath)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        active = EditorSceneManager.OpenScene(k_MainMenuScenePath, OpenSceneMode.Single);
    }

    int removed = removeFromScene(active);

    if (removed > 0)
    {
        EditorSceneManager.MarkSceneDirty(active);
        EditorSceneManager.SaveScene(active);
        Debug.Log($"[Co-op Cleanup] Removed {removed} legacy co-op object(s) from MainMenuScene and saved it.");
    }
    ```
  - **Destruction logic** (lines 89–128):
    Traverses `i_Scene.GetRootGameObjects()` and calls `GetComponentsInChildren<Transform>(true)`. For each candidate matching `k_LegacyCoopNames`, if `!hasLegacyAncestor(candidate)`, it invokes `Undo.DestroyObjectImmediate(candidate.gameObject)` and marks the scene dirty.
  - **Compilation status**: Solution build `dotnet build "Star Vanguard/Star Vanguard.sln"` completed with 0 errors.

### 1.2 Target Objects & Hierarchy in `MainMenuScene.unity`
`MainMenuScene.unity` contains 434 total YAML blocks (12,808 lines). Within these, exactly **67 YAML blocks** belong to the legacy co-op subtrees:
1. **`CoopButton` Subtree** (under `MenuRoot/LeftColumn`):
   - `GameObject &1283642853` (`m_Name: CoopButton`, line 7989)
   - `RectTransform &1283642854` (line 8009): Father is `&648767580` (`LeftColumn`), Child is `&1433009940` (`Label`)
   - `LayoutElement &1283642855` (line 8029)
   - `Button &1283642856` (line 8049): `m_OnClick: m_PersistentCalls: m_Calls: []` (empty persistent call list)
   - `Image &1283642857` (line 8093)
   - `CanvasRenderer &1283642858` (line 8123)
   - `GameObject &1433009939` (`m_Name: Label`, child of `CoopButton`)
   - `RectTransform &1433009940` (line 8845)
   - `TextMeshProUGUI &1433009941` (line 8864)
   - `CanvasRenderer &1433009942` (line 8931)
2. **`CoopPanel` Subtree** (under `Canvas`):
   - `GameObject &1723830002` (`m_Name: CoopPanel`, line 10515, `m_IsActive: 0`)
   - `RectTransform &1723830003` (line 10533): Father is `&134044670` (`Canvas`), Child is `&1951282684` (`CoopCard`)
   - `Image &1723830004` (line 10553)
   - `CanvasRenderer &1723830005` (line 10583)
3. **`CoopCard` and Descendants** (under `CoopPanel`):
   - `GameObject &1951282683` (`m_Name: CoopCard`, line 11690)
   - `RectTransform &1951282684` (line 11708): Father is `&1723830003` (`CoopPanel`), Children: `&1933019121` (`Outline`), `&1800015116` (`Title`), `&1037428439` (`Info`), `&1521203888` (`Buttons`)
   - `Buttons` (`GameObject &1521203887`) contains `HostButton` (`&729178736`), `JoinButton` (`&1109811536`), and `BackButton` (`&1648592515`) and their child `Label`s and `Outline`s.

### 1.3 Reference & Dependency Analysis in `MainMenuScene.unity`
Exhaustive cross-referencing across all 434 blocks in `MainMenuScene.unity` revealed **exactly 2 external references** to any of the 67 co-op fileIDs:
1. `RectTransform &134044670` (`Canvas`): references `&1723830003` (`CoopPanel`) in its `m_Children` array.
2. `RectTransform &648767580` (`LeftColumn`): references `&1283642854` (`CoopButton`) in its `m_Children` array.
- **Zero** script references: No MonoBehaviour or ScriptableObject references any co-op GameObject or component.
- **Zero** UnityEvent references: Neither `LaunchButton`, `ControlsButton`, `SettingsButton`, nor any other button listeners target any co-op object.
- **Layout Behavior**: `LeftColumn` (`&648767579`) has a `VerticalLayoutGroup` (`&648767581`) with `m_Spacing: 16`. When `CoopButton` is deleted, `VerticalLayoutGroup` automatically repositions the remaining buttons (`ControlsButton`, `SettingsButton`, `ExitButton`) smoothly beneath `LaunchButton`.

### 1.4 Codebase Alignment in `MainMenuScreen.cs`
- **File**: `Star Vanguard/Assets/Scripts/UI/MainMenuScreen.cs`
  - Lines 89 & 147: Calls `RemoveLegacyCoopObjects()`.
  - Lines 200–226: Implements runtime fallback removal for `CoopPanel`, `CoopCard`, and `CoopButton`.
  - Lines 254–262: Specifically deactivates `CoopButton` and strips its listeners if present.
  - Lines 386–392: In `BuildHome()`, the active menu layout explicitly instantiates only `LaunchButton`, `ControlsButton`, `SettingsButton`, and `ExitButton`.
  - Therefore, removing `CoopButton` and `CoopPanel` from the scene file brings the serialized scene asset into 100% parity with runtime menu construction.

### 1.5 Static Audit Detection
- Command: `python Tools/check_project.py`
  - Output:
    ```
    [WARN] legacy_coop: Scene con object Co-op (crash khong, nhung UI chet). Chay menu 'Tools/Star Vanguard/1b. Remove Legacy Co-op UI from MainMenuScene and Save'
           - Assets/Scenes/MainMenuScene.unity -> CoopPanel
           - Assets/Scenes/MainMenuScene.unity -> CoopCard
           - Assets/Scenes/MainMenuScene.unity -> CoopButton
    Tong ket: 0 FAIL, 10 WARN, 13 OK
    ```

---

## 2. Logic Chain

1. **Direct Suitability of `StarVanguardCoopCleanup.cs`**:
   - `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave()` (Observation 1.1) is a zero-argument `public static void` method designed specifically for batch or menu execution.
   - It targets `Assets/Scenes/MainMenuScene.unity`, opens it, destroys legacy co-op objects via Unity's Editor API (`Undo.DestroyObjectImmediate`), marks the scene dirty, and saves it.
   - Because it uses Unity's Editor API rather than direct YAML editing, it adheres strictly to `AGENTS.md` Rule 1 ("KHÔNG sửa trực tiếp YAML của .unity, .prefab, .asset bằng tay hoặc regex").

2. **Cascade & Safety of Object Removal**:
   - As observed in Section 1.2 and 1.3, `CoopPanel` contains `CoopCard` and 57 descendant blocks. In Unity, destroying `CoopPanel.gameObject` automatically destroys all nested child GameObjects and attached components.
   - `hasLegacyAncestor(candidate)` and `candidate == null` checks in `StarVanguardCoopCleanup.cs` (lines 97, 104) prevent accessing destroyed child objects in the traversal.
   - Observation 1.3 proved that no scripts or buttons reference any of these 67 objects; external references are strictly limited to the parent `m_Children` lists of `Canvas` and `LeftColumn`.
   - Unity's `DestroyObjectImmediate` automatically unlinks destroyed children from their parent `RectTransform.m_Children` arrays.
   - In `LeftColumn`, the attached `VerticalLayoutGroup` automatically collapses the spacing, cleanly joining `LaunchButton` and `ControlsButton`.

3. **Batch Execution Capability**:
   - `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave` can be invoked in headless Unity batch mode:
     ```powershell
     "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave -logFile -
     ```
   - When executed with `-batchmode -quit`, Unity boots headlessly, loads `Assembly-CSharp-Editor`, runs the method, removes the objects, saves `MainMenuScene.unity`, and exits with code 0.

4. **Single Scene Writer Lock & Milestone 1 Sequencing**:
   - Milestone 1 contains two tasks that modify `MainMenuScene.unity`:
     - **T-204a**: Co-op UI cleanup (`MainMenuScene.unity`)
     - **T-204b**: InputSystemUIInputModule ActionsAsset wiring (`MainMenuScene.unity` and `GameplayScene.unity`)
   - To prevent scene file contention, dirty state collisions, or overwriting changes, the Single Scene Writer lock requires that scene mutations occur sequentially.
   - **Recommended execution sequence for Milestone 1 Worker**:
     1. Step 1 (T-204c): Remove duplicates from `BootstrapScene.unity`.
     2. Step 2 (T-204a): Run `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave` on `MainMenuScene.unity`.
     3. Step 3 (T-204b): Run EventSystem ActionsAsset wiring on `MainMenuScene.unity` and `GameplayScene.unity`.
     4. Step 4: Verify with `python Tools/check_project.py` and smoke tests.

---

## 3. Caveats

1. **Unity Batchmode `SaveCurrentModifiedScenesIfUserWantsTo()`**:
   - In `StarVanguardCoopCleanup.cs` line 71: `if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;`.
   - When Unity launches from the CLI in `-batchmode`, the initially active scene is clean and untitled; `SaveCurrentModifiedScenesIfUserWantsTo()` returns `true` immediately without showing a UI prompt. However, if Unity were invoked in an environment with an unsaved dirty scene, it could abort. Launching freshly via `-batchmode -quit` guarantees a clean state.
2. **Sequential Scene Modification**:
   - `MainMenuScene.unity` must NOT be touched concurrently by T-204a and T-204b workers. They must execute under the Single Scene Writer lock sequentially.
3. **Backup Check**:
   - `Star Vanguard/SceneBackups/MainMenuScene.unity.bak` exists as an emergency fallback. A Git commit should be recorded before executing scene modifications to provide clean revert points per AGENTS.md Rule 6.

---

## 4. Conclusion

1. **Readiness**: Task T-204a is 100% ready for execution. The tooling (`StarVanguardCoopCleanup.cs`) is already written, compiles cleanly, and correctly targets `CoopPanel`, `CoopCard`, and `CoopButton`.
2. **Zero Side Effects**: Exhaustive reference analysis confirmed that no external scripts, event listeners, or layouts depend on the destroyed objects. Removing them is completely safe and restores parity between serialized scene data and `MainMenuScreen.cs`.
3. **Execution Strategy**:
   - Use Unity batchmode `-executeMethod StarVanguard.EditorTools.StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave`.
   - Do NOT edit scene YAML directly.
   - Execute sequentially under the Single Scene Writer lock prior to or coordinated with T-204b.

---

## 5. Verification Method

### 5.1 Command to Execute Fix (For Worker)
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave -logFile -
```
Expected output in log:
`[Co-op Cleanup] Removed 2 legacy co-op object(s) from MainMenuScene and saved it.`

### 5.2 Independent Verification Steps
1. **Gate G3 (Static Audit)**:
   ```powershell
   python Tools/check_project.py
   ```
   - **Pass condition**: `[WARN] legacy_coop` is completely eliminated. Total WARN count decreases from 10 to 9. Total FAIL remains 0.
2. **Scene YAML Inspection**:
   Inspect `Star Vanguard/Assets/Scenes/MainMenuScene.unity`:
   - Verify `m_Name: CoopPanel`, `m_Name: CoopCard`, and `m_Name: CoopButton` are absent.
   - Verify `LeftColumn` (`&648767580`) `m_Children` no longer includes `&1283642854`.
   - Verify `Canvas` (`&134044670`) `m_Children` no longer includes `&1723830003`.
3. **Gate G1 (Compile Check)**:
   ```powershell
   dotnet build "Star Vanguard/Star Vanguard.sln"
   ```
   - **Pass condition**: 0 errors.
4. **Gate G2 (Regression / Functional Smoke Test)**:
   ```powershell
   "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile -
   ```
   - **Pass condition**: Exit code 0, all menu checks pass.
