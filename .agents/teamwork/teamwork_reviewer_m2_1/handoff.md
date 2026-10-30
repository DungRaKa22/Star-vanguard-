# Quality & Adversarial Review Report — Milestone 2: Asmdef & Architecture Integrity Review

**Reviewer**: Reviewer M2-1 (`teamwork_reviewer_m2_1`)  
**Target Work Product**: Milestone 2 (T-101a, T-101b, T-101c) implemented by `teamwork_worker_m2`  
**Verdict**: **APPROVE**  
**Overall Risk Assessment**: **LOW**

---

## 1. Observation

### 1.1 Assembly Definition Schema & File Layout Verification
Direct inspection of the four `.asmdef` files and their accompanying `.meta` files confirmed:

1. `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef` (GUID `273affc558c670e4789758f11050689c`):
   ```json
   {
       "name": "StarVanguard.Runtime",
       "rootNamespace": "",
       "references": [
           "Unity.TextMeshPro",
           "UnityEngine.UI",
           "Unity.InputSystem",
           "Unity.RenderPipelines.Universal.Runtime"
       ],
       "includePlatforms": [],
       "excludePlatforms": [],
       "allowUnsafeCode": false,
       "overrideReferences": false,
       "precompiledReferences": [],
       "autoReferenced": true,
       "defineConstraints": [],
       "versionDefines": [],
       "noEngineReferences": false
   }
   ```
   - **Internal project references**: None (`[]`). Isolates core gameplay runtime.

2. `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef` (GUID `2e575fc2c38056243b20d958dbcd97c9`):
   ```json
   {
       "name": "StarVanguard.UI",
       "rootNamespace": "StarVanguard.UI",
       "references": [
           "StarVanguard.Runtime",
           "UnityEngine.UI",
           "Unity.TextMeshPro",
           "Unity.InputSystem"
       ],
       "includePlatforms": [],
       "excludePlatforms": [],
       "allowUnsafeCode": false,
       "overrideReferences": false,
       "precompiledReferences": [],
       "autoReferenced": false,
       "defineConstraints": [],
       "versionDefines": [],
       "noEngineReferences": false
   }
   ```
   - Reference flows one-way: `StarVanguard.UI` -> `StarVanguard.Runtime`.
   - `autoReferenced` is explicitly `false`, preventing implicit assembly exposure.

3. `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef` (GUID `86c70e980808e6142aa59fd76726c1a6`):
   - References: `StarVanguard.Runtime`, `StarVanguard.UI`, Unity Editor and package dependencies.
   - `includePlatforms`: `["Editor"]`.

4. `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef` (GUID `26476cab263ebe54c87edff4eb213ba4`):
   - References: `StarVanguard.Runtime`, `StarVanguard.UI`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, Unity package dependencies.
   - `includePlatforms`: `["Editor"]`.
   - `precompiledReferences`: `["nunit.framework.dll"]`, `overrideReferences`: `true`.
   - `defineConstraints`: `["UNITY_INCLUDE_TESTS"]`.

All 4 `.asmdef` files have matching `.meta` files and are recognized by Unity's CompilationPipeline.

### 1.2 Decoupling of `UIManager.cs` and Runtime Scripts
Direct inspection of `Star Vanguard/Assets/Scripts/UIManager.cs`:
- Removed `using StarVanguard.UI;` completely (lines 1-4).
- Removed direct calls to `UIPanelRouter.Instance` and references to `UIPanelId`.
- Declared 7 C# navigation events (lines 13-21):
  ```csharp
  #region UI Navigation Events
  public static event Action OnShowSettingsRequested;
  public static event Action OnHideSettingsRequested;
  public static event Action OnShowControlsRequested;
  public static event Action OnHideControlsRequested;
  public static event Action OnShowAudioSettingsRequested;
  public static event Action OnHideSubPanelsRequested;
  public static event Action OnHideAllPanelsRequested;
  #endregion
  ```
- Public API preserved for backward compatibility (lines 311-347), delegating to event invocations with null-conditional operator `?.Invoke()`.

### 1.3 Strict Zero-Reference Verification Across All Runtime Scripts
Executed an automated scan across all `.cs` files in `Star Vanguard/Assets/Scripts/` (excluding the `UI/` subdirectory) for banned tokens (`StarVanguard.UI`, `UIPanelRouter`, `UIPanelId`, `UITheme`, `ControlsScreen`, `InputMap`, `MainMenuScreen`, `SettingsScreen`):
```text
Total violations: 0 across all 49 scripts in Assets/Scripts/
```
Grep across the repository confirmed that `StarVanguard.UI`, `UIPanelRouter`, and `UIPanelId` are strictly encapsulated within `Star Vanguard/Assets/Scripts/UI/`, `Assets/Editor/`, and `Assets/Tests/`.

### 1.4 Event Subscription Lifecycle in `UIPanelRouter.cs`
Direct inspection of `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`:
- `OnEnable()` (lines 69-79) subscribes to `GameManager.OnGameStateChanged` and all 7 `UIManager` navigation events:
  ```csharp
  private void OnEnable()
  {
      GameManager.OnGameStateChanged += HandleGameStateChanged;
      UIManager.OnShowSettingsRequested += HandleShowSettings;
      UIManager.OnHideSettingsRequested += HandleHideSettings;
      UIManager.OnShowControlsRequested += HandleShowControls;
      UIManager.OnHideControlsRequested += HandleHideControls;
      UIManager.OnShowAudioSettingsRequested += HandleShowAudioSettings;
      UIManager.OnHideSubPanelsRequested += HandleHideSubPanels;
      UIManager.OnHideAllPanelsRequested += HandleHideAll;
  }
  ```
- `OnDisable()` (lines 81-91) unconditionally unsubscribes from all 8 events, strictly conforming to AGENTS.md Rule #4.
- `Start()` (lines 93-100) checks `if (GameManager.Instance != null && m_Stack.Count == 0)` and triggers `HandleGameStateChanged` to synchronize initial game state upon boot.

### 1.5 Independent Verification Command Executions
1. **Gate G1 (Compile & Build)**:
   - Command: `dotnet build "Star Vanguard\Star Vanguard.sln"`
   - Result: `4 Warning(s), 0 Error(s), Time Elapsed 00:00:15.10`.
   - Unity Roslyn Compilation: `*** Tundra build success (0.60 seconds), 1 items updated, 1131 evaluated`.
2. **Gate G2 (Regression & Validator)**:
   - Command: `Unity -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardValidator.ValidateAll -logFile Temp/validator_cmd.log`
   - Result: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`. All 37 validator assertions passed.
   - Command: `Unity -batchmode -runTests -testPlatform EditMode -testResults Temp/nunit_reviewer_results.xml -logFile Temp/nunit_reviewer.log`
   - Result: `<test-run id="2" testcasecount="5" result="Passed" total="5" passed="5" failed="0">`. All 5 NUnit EditMode tests passed.
   - Command: `Unity -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile Temp/playmode_smoke_challenger.log`
   - Result: `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`. All 55 PlayMode assertions passed.
3. **Gate G3 (Static Project Audit)**:
   - Command: `python Tools/check_project.py`
   - Result: `Tong ket: 0 FAIL, 6 WARN, 17 OK` (reduced from 8 WARN, 0 FAIL).
   - Validated: `[ OK ] asmdef: 4 asmdef`, `[ OK ] unit_tests: Co test NUnit`, `[ OK ] build_scenes: Build order dung`, `[ OK ] dead_code: Khong co script chet ro rang`.

---

## 2. Logic Chain

1. **Architecture Isolation (Obs 1.1, Obs 1.2, Obs 1.3)**: `StarVanguard.Runtime.asmdef` contains no references to `StarVanguard.UI`. Grep analysis proves zero source-level dependencies in `Assets/Scripts/`. `UIManager.cs` now depends only on C# language primitives (`Action`) for its event bus.
2. **Compiler-Enforced Boundary**: Because `StarVanguard.UI` has `autoReferenced: false`, no assembly can reference `StarVanguard.UI` without explicitly listing it. Any attempt by `StarVanguard.Runtime` to use UI types triggers Roslyn compiler error `CS0234` (verified empirically when negative test injection failed compilation as expected).
3. **Lifecycle Soundness (Obs 1.4)**: `UIPanelRouter` unsubscribes unconditionally in `OnDisable()`, eliminating memory leaks or dangling event invocations across scene changes.
4. **Integrity Verification (Obs 1.5)**:
   - No hardcoded test results: `AssemblySmokeTests.cs` dynamically examines reflection metadata (`assembly.GetReferencedAssemblies()`).
   - No facades or fake logic: Validator inspects real scenes, prefabs, pool sizes, and build settings.
   - All three safety gates (G1, G2, G3) were independently reproduced and confirmed passing 100%.

---

## 3. Adversarial Challenges & Stress Testing

### Challenge 1: Accidental Direct Reference Injection
- **Assumption**: Developers or future tasks might attempt to access `StarVanguard.UI` from runtime scripts.
- **Stress Test**: A temporary reference `public StarVanguard.UI.UIPanelRouter LeakedRouterReference;` in runtime code failed with:
  `Assets\Scripts\CanvasAspectFitter.cs(2,20): error CS0234: The type or namespace name 'UI' does not exist in the namespace 'StarVanguard'`
- **Conclusion**: The assembly definition boundary strictly prevents any illegal upward dependency at compile time.

### Challenge 2: Null Listeners & Out-of-Order Lifecycle
- **Assumption**: `UIManager` methods may be invoked when `UIPanelRouter` is not yet active or disabled.
- **Stress Test**: Analyzed all event calls in `UIManager.cs`. Every event invocation uses null-conditional `.Invoke()` (`OnShowControlsRequested?.Invoke()`). If no UI router is present, calls gracefully no-op without throwing `NullReferenceException`.
- **Conclusion**: Robust against additive scene loading and out-of-order execution.

### Challenge 3: Double-Navigation Invocations
- **Assumption**: `UIManager.HideControlsPanel()` calls both `OnHideControlsRequested?.Invoke()` and `OnHideSubPanelsRequested?.Invoke()`. Could this cause double-back popping?
- **Analysis**:
  - `HandleHideControls()` checks `if (Current == UIPanelId.Controls) Back();`. Popping `Controls` changes `Current` to `Home` (or `None`).
  - `HandleHideSubPanels()` then checks `if (Current == UIPanelId.Settings || Current == UIPanelId.Controls) Back();`. Because `Current` is now `Home`, the condition evaluates to `false`, preventing a secondary pop.
- **Conclusion**: Stack behavior remains safe and idempotent.

---

## 4. Caveats

- **No Caveats**: All tasks for Milestone 2 (T-101a, T-101b, T-101c) have been thoroughly investigated, independently compiled, and verified via automated CLI and Unity batchmode tools.

---

## 5. Conclusion & Verdict

**Verdict**: **APPROVE**

Milestone 2 implementation satisfies all requirements from `ORIGINAL_REQUEST.md` (§R4), `PROJECT.md`, and `AGENTS.md`:
- Strict one-way assembly definitions established with 0 cyclic dependencies.
- `UIManager.cs` decoupled via clean C# navigation events.
- 0 references to UI in runtime scripts.
- 100% pass across Gates G1, G2 (37/37 validator, 5/5 NUnit, 55/55 smoke), and G3 (0 FAIL, 6 WARN <= baseline).
- Zero integrity violations detected.

---

## 6. Verification Method

To independently reproduce this review:

1. **Verify Static Audit (Gate G3)**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expected:* `Tong ket: 0 FAIL, 6 WARN, 17 OK` with `[ OK ] asmdef: 4 asmdef`.

2. **Verify Solution Compilation (Gate G1)**:
   ```powershell
   dotnet build "Star Vanguard\Star Vanguard.sln"
   ```
   *Expected:* `0 Error(s)`.

3. **Verify Project Validator (Gate G2)**:
   ```powershell
   cmd.exe /c "`"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe`" -projectPath `"C:\Documents\UNITY 2026\Star Vanguard`" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardValidator.ValidateAll -logFile `"C:\Documents\UNITY 2026\Star Vanguard\Temp\validator_cmd.log`""
   Get-Content "Star Vanguard\Temp\validator_cmd.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expected:* `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`.

4. **Verify NUnit EditMode Tests (Gate G2)**:
   ```powershell
   cmd.exe /c "`"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe`" -projectPath `"C:\Documents\UNITY 2026\Star Vanguard`" -batchmode -runTests -testPlatform EditMode -testResults `"C:\Documents\UNITY 2026\Star Vanguard\Temp\nunit_reviewer_results.xml`" -logFile `"C:\Documents\UNITY 2026\Star Vanguard\Temp\nunit_reviewer.log`""
   Get-Content "Star Vanguard\Temp\nunit_reviewer_results.xml" | Select-String -Pattern "testcasecount"
   ```
   *Expected:* `testcasecount="5" result="Passed" total="5" passed="5" failed="0"`.
