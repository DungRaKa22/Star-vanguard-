# Handoff Report — Reviewer M1-2: Code Quality & Gate Verification

## 1. Observation

Direct, independent observations of the Milestone 1 artifacts:

### 1.1 Source Code Quality & Standards Compliance
1. `Star Vanguard/Assets/Editor/StarVanguardBootstrapCleanup.cs`:
   - Namespace: `StarVanguard.EditorTools` (line 7).
   - Constants: `k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity"` (line 23), `k_DuplicateObjectNames = { "Main Camera", "Global Light 2D" }` (line 24).
   - Parameter naming: `i_Scene` (line 69).
   - GUI/Batchmode safety: Checks `EditorApplication.isPlayingOrWillChangePlaymode` (line 29), uses `!Application.isBatchMode` before prompting `EditorUtility.DisplayDialog` (line 31) and `EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()` (line 48).
   - Modification: Uses `Undo.DestroyObjectImmediate` (line 87), `EditorSceneManager.MarkSceneDirty` (line 59), and `EditorSceneManager.SaveScene` (line 60).
   - Idempotency: Returns 0 and logs `Nothing to save` when duplicate roots are absent.

2. `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs`:
   - Added `"Main Camera"` and `"Global Light 2D"` to `k_NonBootstrapObjects` (lines 53–54).
   - `k_NonBootstrapObjects` is consumed only in `bootstrapScene` cleanup (lines 77–84). Persistent managers stripping in `GameplayScene` and `MainMenuScene` operates exclusively on `k_PersistentManagerNames` (lines 104, 116), leaving their local cameras and lights intact.

3. `Star Vanguard/Assets/Editor/StarVanguardInputWiring.cs`:
   - Namespace: `StarVanguard.EditorTools` (line 11).
   - Canonical GUID validation: `AssetDatabase.AssetPathToGUID(k_CanonicalActionsAssetPath)` compared against `65b761383e570eb4390d8402a8c60d39` (lines 108–113).
   - Component queries: Uses `UnityEngine.Object.FindObjectsByType<InputSystemUIInputModule>(FindObjectsInactive.Include)` (lines 118–119).
   - Serialization persistence: Mutates runtime properties and synchronizes serialized properties via `SerializedObject` and `SetObjectReferenceProperty` across all 11 action references (`m_ActionsAsset`, `m_PointAction`, `m_MoveAction`, `m_SubmitAction`, `m_CancelAction`, `m_LeftClickAction`, `m_MiddleClickAction`, `m_RightClickAction`, `m_ScrollWheelAction`, `m_TrackedDevicePositionAction`, `m_TrackedDeviceOrientationAction`) followed by `so.ApplyModifiedProperties()`, `EditorUtility.SetDirty()`, and `EditorSceneManager.SaveScene()` (lines 161–200).
   - Parameter naming: Uses standard camelCase (`scenePath`, `so`, `propertyName`, `target`) rather than `i_` prefix.

4. `Star Vanguard/Assets/Scripts/UIManager.cs`:
   - Lines 331–337 & 344–350: Replaced null-conditional `UIPanelRouter.Instance?.Back()` with explicit `if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Controls)` and `if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Settings)` before invoking `Back()`.

### 1.2 Build & Static Gate Verification
1. **Gate G1 (Compile)**:
   - Command executed:
     ```powershell
     dotnet build "Star Vanguard\Star Vanguard.sln"
     ```
   - Verbatim result:
     ```
     Assembly-CSharp -> C:\Documents\UNITY 2026\Star Vanguard\Temp\bin\Debug\Assembly-CSharp.dll
     18 Warning(s)
     0 Error(s)
     Time Elapsed 00:00:02.33
     ```
     Exit code: `0`. 0 errors.

2. **Gate G3 (Static Project Audit)**:
   - Command executed:
     ```powershell
     python Tools/check_project.py
     ```
   - Verbatim result:
     ```
     [ OK ] legacy_coop: Khong con object Co-op trong scene
     Tong ket: 0 FAIL, 8 WARN, 15 OK
     ```
     Exit code: `0`. 0 failures. `legacy_coop` is eliminated (was previously reported as `[WARN]`). Total warnings decreased from 10 to 8.

3. **Dangling GUID Elimination (`ca9f5fa95ffab41fb9a615ab714db018`)**:
   - Ripgrep search across `Star Vanguard/Assets`:
     Result: **0 matches** found.
   - Ripgrep search across `Star Vanguard/SceneBackups`:
     Result: **0 matches** found.

4. **Canonical GUID Wiring (`65b761383e570eb4390d8402a8c60d39`)**:
   - Ripgrep search across `Star Vanguard/Assets/Scenes`:
     Result: Exactly 22 occurrences (11 in `MainMenuScene.unity:3002-3012`, 11 in `GameplayScene.unity:1677-1687`).

5. **Duplicate Camera & Light in `BootstrapScene.unity`**:
   - Ripgrep search in `BootstrapScene.unity` for `Main Camera`: **0 matches**.
   - Ripgrep search in `BootstrapScene.unity` for `Global Light 2D`: **0 matches**.
   - Line 686–691: `SceneRoots` contains strictly `{fileID: 760890376}` (`Managers`) and `{fileID: 582285522}` (`BootstrapController`).

6. **Automated PlayMode Smoke Verification (Gate G2)**:
   - `Star Vanguard/smoke_test.log` line 1474:
     ```
     [STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0
     ```

---

## 2. Logic Chain

1. **Integrity and Anti-Cheating Verification**:
   - The implementations in `StarVanguardBootstrapCleanup.cs`, `StarVanguardBootstrapSetup.cs`, and `StarVanguardInputWiring.cs` were inspected line by line.
   - All logic performs genuine object graph traversal, component inspection, and Unity serialization operations.
   - No hardcoded test stubs, facade implementations, or bypassed verification outputs exist. The attestation outputs match actual filesystem state.
   - **Integrity Status**: PASS.

2. **AGENTS.md Rule 1 Compliance (Zero Direct YAML Edits)**:
   - Git diff inspection confirms that all modifications to `BootstrapScene.unity`, `MainMenuScene.unity`, and `GameplayScene.unity` were written by the Unity engine via `EditorSceneManager.SaveScene` following batchmode execution of the Editor tools.
   - Diff statistics (`5 files changed, 541 insertions(+), 2552 deletions(-)`) reflect clean removal of obsolete YAML blocks (1,763 lines of legacy co-op objects and 254 lines of duplicate camera/light) without dangling references or syntax errors.

3. **Regression Safety of Bootstrap Setup Modification**:
   - Updating `k_NonBootstrapObjects` in `StarVanguardBootstrapSetup.cs` guarantees that if an engineer re-runs `Setup Bootstrap Scene` via menu or script, the newly cloned `BootstrapScene` will have `Main Camera` and `Global Light 2D` stripped immediately.
   - Because `GameplayScene` and `MainMenuScene` are handled via `k_PersistentManagerNames`, their respective cameras and lights remain unaffected.

4. **Defensive UI Routing in `UIManager.cs`**:
   - `UIPanelRouter.Back()` pops the top element of `m_Stack`. If `HideControlsPanel()` or `HideSettingsPanel()` was called when those panels were not active (or called redundantly), it would pop `UIPanelId.Home`, leaving the router at `UIPanelId.None` and disabling all UI interaction.
   - The defensive checks `UIPanelRouter.Instance.Current == UIPanelId.Controls` and `== UIPanelId.Settings` directly prevent this failure mode.
   - The change also replaced `UIPanelRouter.Instance?.Back()` with explicit `!= null` comparison, conforming to AGENTS.md Rule 4 (Unity fake-null safety).

5. **Verification Gates Fully Met**:
   - Gate G1 compiles cleanly with 0 errors via `dotnet build`.
   - Gate G2 passes 55/55 assertions in `StarVanguardPlayModeSmoke`.
   - Gate G3 passes with 0 FAIL and reduced warnings (8 WARN, down from 10) in `check_project.py`.

---

## 3. Caveats

1. **Interactive Scene Dirty Guard in `StarVanguardInputWiring.cs`**:
   `StarVanguardInputWiring.cs:WireScene` opens scenes directly using `EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)`. In batchmode this is completely safe, but in interactive Editor GUI mode, standard practice is to prompt `if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;` to avoid discarding unsaved changes in an already open scene.
2. **Parameter Naming Convention**:
   Methods in `StarVanguardInputWiring.cs` use standard camelCase parameter names (`scenePath`, `so`, `propertyName`, `target`) rather than the `i_Name` prefix specified in `AGENTS.md` Rule 4. This is a minor stylistic deviation and does not affect runtime or compilation.
3. **Legacy `StarVanguardValidator.cs`**:
   `StarVanguardValidator.cs` predates the introduction of `BootstrapScene.unity` and expects `MainMenuScene.unity` at build index 0 and local `GameManager` in `GameplayScene.unity`. It should eventually be refreshed or superseded by `StarVanguardPlayModeSmoke.cs`, which correctly validates the modern bootstrap architecture.

---

## 4. Conclusion & Review Summary

### **Verdict: APPROVE**

Milestone 1 successfully delivers all objectives (T-204a, T-204b, T-204c):
1. **T-204a (Legacy Co-op UI Cleanup)**: Completely purged from `MainMenuScene.unity`; static audit check `legacy_coop` reports `[ OK ]`.
2. **T-204b (InputSystem EventSystem Wiring)**: Canonical `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) is correctly wired across all 11 action references in both `MainMenuScene.unity` and `GameplayScene.unity`. Dangling GUID `ca9f5fa95ffab41fb9a615ab714db018` is completely gone.
3. **T-204c (BootstrapScene Cleanup & Setup Update)**: Duplicate `Main Camera` and `Global Light 2D` are completely removed from `BootstrapScene.unity`. `StarVanguardBootstrapSetup.cs` is updated to prevent regression on subsequent runs.
4. **Gates G1, G2, G3**: All passed cleanly with 0 compilation errors, 100% smoke test pass rate, and 0 static check failures.

### Findings

#### [Minor] Finding 1: Parameter Naming Convention in `StarVanguardInputWiring.cs`
- **What**: Method parameters do not use the `i_` prefix (e.g. `scenePath` instead of `i_ScenePath`, `so` instead of `i_SerializedObject`).
- **Where**: `Star Vanguard/Assets/Editor/StarVanguardInputWiring.cs:73, 91, 203, 216, 222`.
- **Why**: Minor divergence from `AGENTS.md` Rule 4 (`tham số: i_Name`).
- **Suggestion**: Rename parameters to follow `i_Parameter` convention during next cleanup pass.

#### [Minor] Finding 2: Unsaved Scene Prompt in GUI Mode for `StarVanguardInputWiring.cs`
- **What**: `WireScene` opens target scenes without checking `EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()`.
- **Where**: `Star Vanguard/Assets/Editor/StarVanguardInputWiring.cs:94`.
- **Why**: If invoked manually from the Editor menu with dirty scenes open, user modifications in the active scene could be discarded without prompt.
- **Suggestion**: Add `if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;` before opening scenes.

---

## 5. Verification Method

To independently reproduce and verify this review:

1. **Verify Solution Compilation (Gate G1)**:
   ```powershell
   dotnet build "Star Vanguard\Star Vanguard.sln"
   ```
   *Expected*: `0 Error(s)`.

2. **Verify Static Audit Gate (Gate G3)**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expected*: `0 FAIL, 8 WARN, 15 OK` with `[ OK ] legacy_coop`.

3. **Verify Elimination of Dangling GUID**:
   ```powershell
   python -c "text=open('Star Vanguard/Assets/Scenes/MainMenuScene.unity', 'r', encoding='utf-8').read(); print('ca9f in MainMenu:', 'ca9f5fa95ffab41fb9a615ab714db018' in text)"
   python -c "text=open('Star Vanguard/Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8').read(); print('ca9f in Gameplay:', 'ca9f5fa95ffab41fb9a615ab714db018' in text)"
   ```
   *Expected*: Both print `False`.

4. **Verify Wiring of Canonical GUID**:
   ```powershell
   python -c "import re; text=open('Star Vanguard/Assets/Scenes/MainMenuScene.unity', 'r', encoding='utf-8').read(); print('Canonical matches in MainMenu:', len(re.findall('65b761383e570eb4390d8402a8c60d39', text)))"
   python -c "import re; text=open('Star Vanguard/Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8').read(); print('Canonical matches in Gameplay:', len(re.findall('65b761383e570eb4390d8402a8c60d39', text)))"
   ```
   *Expected*: Both print `11`.

5. **Verify Duplicate Camera & Light Removal from BootstrapScene**:
   ```powershell
   python -c "text=open('Star Vanguard/Assets/Scenes/BootstrapScene.unity', 'r', encoding='utf-8').read(); print('Main Camera in Bootstrap:', 'Main Camera' in text); print('Global Light 2D in Bootstrap:', 'Global Light 2D' in text)"
   ```
   *Expected*: Both print `False`.
