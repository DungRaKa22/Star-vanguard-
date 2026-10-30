# Review & Adversarial Challenge Report — Milestone 2 (T-101)
**Reviewer**: Reviewer M2-2 (Code Quality, Test Fixture & Static Checks)  
**Date**: 2026-10-30T09:17:00Z  
**Verdict**: **`APPROVE`**  
**Overall Risk Assessment**: **LOW**

---

## 1. Observation

### 1.1 Code Quality & Decoupling Audit (`UIPanelRouter.cs` & `UIManager.cs`)
1. **Coupling Removal**:
   - `Star Vanguard/Assets/Scripts/UIManager.cs`:
     - Line 1: `using StarVanguard.UI;` removed.
     - Lines 14–20: Defined static navigation events:
       - `public static event Action OnShowSettingsRequested;`
       - `public static event Action OnHideSettingsRequested;`
       - `public static event Action OnShowControlsRequested;`
       - `public static event Action OnHideControlsRequested;`
       - `public static event Action OnShowAudioSettingsRequested;`
       - `public static event Action OnHideSubPanelsRequested;`
       - `public static event Action OnHideAllPanelsRequested;`
     - Line 78: Replaced `if (GameManager.HasInstance) GameManager.OnGameStateChanged -= showState;` with unconditional unsubscription:
       ```csharp
       GameManager.OnGameStateChanged -= showState;
       ```
       Directly conforms to `AGENTS.md` Rule #4.
     - Lines 311–347: Replaced direct router calls with event invocations (`OnShowControlsRequested?.Invoke()`, `OnShowSettingsRequested?.Invoke()`, etc.).
   - Exhaustive script check across `Star Vanguard/Assets/Scripts/` (49 scripts):
     ```powershell
     python -c "import os; dir_p = r'Star Vanguard\Assets\Scripts'; print([f for f in os.listdir(dir_p) if f.endswith('.cs') and any(t in open(os.path.join(dir_p, f), encoding='utf-8', errors='ignore').read() for t in ['StarVanguard.UI', 'UIPanelRouter', 'UIPanelId'])])"
     ```
     Result: `[]` (0 references). `StarVanguard.Runtime` has zero dependencies on `StarVanguard.UI`.
2. **Lifecycle & State Synchronization (`UIPanelRouter.cs`)**:
   - Lines 69–79 (`OnEnable()`): Subscribes to `GameManager.OnGameStateChanged` and all 7 `UIManager` navigation events.
   - Lines 81–91 (`OnDisable()`): Unsubscribes from all 8 events unconditionally.
   - Lines 93–100 (`Start()`):
     ```csharp
     if (GameManager.Instance != null && m_Stack.Count == 0)
     {
         HandleGameStateChanged(GameManager.Instance.GameState);
     }
     ```
     Correctly synchronizes UI state on initial boot if no panel is registered or open yet.
   - Lines 217–234 (`HandleGameStateChanged`):
     - `eGameState.Menu` => `Show(UIPanelId.Home)`
     - `eGameState.Paused` => `Show(UIPanelId.Pause)`
     - `eGameState.GameOver` => `Show(UIPanelId.GameOver)`
     - `eGameState.Playing` => `HideAll()`
   - Rule #4 Conformance:
     - All private fields follow `m_PascalCase` (`m_Panels`, `m_Groups`, `m_Fades`, `m_Stack`).
     - Parameters follow `i_PascalCase` (`i_Visible`, `i_Id`, `i_Panel`, `i_State`).
     - Object comparisons use `== null` / `!= null` (no fake-null `?.` on `UnityEngine.Object`).
     - Coroutines are explicitly tracked in `m_Fades` and stopped before state changes.

### 1.2 Validator Alignment (`StarVanguardValidator.cs`)
1. **Menu Item & Batchmode Protection**:
   - Lines 24–25: Added `[MenuItem("Star Vanguard/Validate Project", priority = 50)]` and `[MenuItem("Tools/Star Vanguard/Validate Project", priority = 50)]`.
   - Lines 41–44:
     ```csharp
     if (Application.isBatchMode)
     {
         EditorApplication.Exit(s_Failed == 0 ? 0 : 1);
     }
     ```
     Guards `EditorApplication.Exit` so developers can run validation inside the interactive Editor without crashing/quitting the Unity editor.
2. **Canonical Scene Order**:
   - Lines 128–142 (`CheckBuildSettings()`):
     - Asserts `scenes.Length >= 3`.
     - Index 0: `Assets/Scenes/BootstrapScene.unity`
     - Index 1: `Assets/Scenes/MainMenuScene.unity`
     - Index 2: `Assets/Scenes/GameplayScene.unity`
3. **Additive Scene Loading Mirror**:
   - Lines 144–176 (`CheckGameplayScene()`):
     - Opens `BootstrapScene.unity` as `OpenSceneMode.Single`.
     - Opens `GameplayScene.unity` as `OpenSceneMode.Additive`.
     - Asserts `GameManager`, `UIManager`, `AudioManager`, `WaveDirector` (with assigned prefabs), and `PoolManager` (`m_EnemyPoolSize == 50 >= 40`) are present and valid across the loaded scenes.

### 1.3 NUnit Test Fixture (`Assets/Tests/AssemblySmokeTests.cs`)
- Located in `Star Vanguard/Assets/Tests/AssemblySmokeTests.cs` under `StarVanguard.Tests.asmdef`.
- 5 comprehensive EditMode tests:
  1. `RuntimeAssembly_LoadsExpectedCoreTypes`: Verifies resolution of `GameManager`, `PlayerController`, `PoolManager`, `AudioManager`, `BootstrapLoader`.
  2. `UIAssembly_LoadsExpectedUITypes`: Verifies resolution of `UIPanelRouter`, `MainMenuScreen`, `SettingsScreen`, `ControlsScreen`.
  3. `ArchitecturalConstraint_RuntimeDoesNotReferenceUI`: Uses CLR reflection (`typeof(GameManager).Assembly.GetReferencedAssemblies()`) to assert that `StarVanguard.Runtime` contains zero assembly references to `StarVanguard.UI`.
  4. `ArchitecturalConstraint_UIDependsOnRuntime`: Verifies that `StarVanguard.UI` has an explicit reference to `StarVanguard.Runtime`.
  5. `EnumsAndConstants_AreConsistent`: Verifies enum integrity for `eGameState` and `UIPanelId`.

### 1.4 Independent Gate Verification Results
1. **Gate G1 (Compile & Build)**:
   - Command: `dotnet build "Star Vanguard\Star Vanguard.sln"`
   - Output: `0 Warning(s)`, `0 Error(s)`, exit code 0.
2. **Gate G2 (Regression & Test Suites)**:
   - **Validator Execution**:
     - Command: `Unity -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardValidator.ValidateAll -logFile Temp/validator_cmd.log`
     - Output: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`, exit code 0.
   - **NUnit Test Execution**:
     - Command: `Unity -batchmode -runTests -testPlatform EditMode -testResults Temp/challenger_nunit_results.xml -logFile Temp/challenger_nunit.log`
     - Output XML: `<test-run id="2" testcasecount="5" result="Passed" total="5" passed="5" failed="0">`. All 5 tests passed.
   - **PlayMode Smoke Execution**:
     - Command: `Unity -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile Temp/playmode_smoke_challenger.log`
     - Output: `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`. All 55 checks passed.
3. **Gate G3 (Static Project Audit)**:
   - Command: `python Tools/check_project.py`
   - Output: `Tong ket: 0 FAIL, 6 WARN, 17 OK`, exit code 0.
   - Verified items:
     - `[ OK ] asmdef: 4 asmdef`
     - `[ OK ] unit_tests: Co test NUnit`
     - `[ OK ] build_scenes: Build order dung: BootstrapScene.unity -> MainMenuScene.unity -> GameplayScene.unity`
     - `[ OK ] meta_missing: Moi asset deu co .meta`
     - `[ OK ] meta_orphan: Khong co .meta mo coi`

---

## 2. Logic Chain

1. **Premise 1 (R4 Strict Decoupling)**: Requirement R4 demands one-way assembly dependencies where `StarVanguard.Runtime` cannot depend on `StarVanguard.UI`. Removing direct `using StarVanguard.UI;` and switching to static C# events in `UIManager` cleanly breaks the circularity. Observation 1.1 proved that 0 references to UI exist in `Assets/Scripts/`.
2. **Premise 2 (Lifecycle & Memory Leak Prevention)**: `UIPanelRouter` subscribes in `OnEnable()` and unconditionally unsubscribes in `OnDisable()`. In Unity, `OnDisable()` is guaranteed to execute before `OnDestroy()` or upon game object deactivation. This eliminates dangling event delegate memory leaks.
3. **Premise 3 (Validator Correctness & Safety)**: In Milestone 1, persistent singletons were centralized into `BootstrapScene.unity`. Running `StarVanguardValidator` additively matches the production game loop (`BootstrapLoader` loads `BootstrapScene` before `GameplayScene`). Guarding `EditorApplication.Exit` with `Application.isBatchMode` ensures the validator is safe for Editor menus and CLI CI.
4. **Premise 4 (NUnit Test Rigor & Integrity)**: `AssemblySmokeTests.cs` performs genuine reflection checks on assembly metadata and type loaders. It directly verifies the architectural constraints at runtime.
5. **Premise 5 (Static Verification Invariance)**: Excluding `Tests` from `check_dead_scripts` in `Tools/check_project.py` prevents false positives on NUnit test classes while maintaining full asset and script tracking.
6. **Deduction**: All acceptance criteria for Milestone 2 (T-101) are satisfied, and all three quality gates (G1, G2, G3) pass cleanly.

---

## 3. Adversarial Challenges & Integrity Verification

### 3.1 Integrity Violation Checks
- **Hardcoded test results**: None detected. Tests dynamically reflect on assemblies and scan actual serialized properties.
- **Dummy/facade implementations**: None detected. `UIPanelRouter` manages actual CanvasGroups, raycast blocks, and coroutine animations; `UIManager` triggers real events.
- **Shortcuts bypassing task**: None detected. Four `.asmdef` files were created with correct schemas and compile boundaries.
- **Fabricated verification logs**: Independently reproduced all runs (MSBuild, NUnit XML, Validator log, PlayMode smoke log, python check).

### 3.2 Adversarial Failure Mode Challenges

#### Challenge 1: Event Double-Emission in UIManager Sub-Panel Dismissal
- **Observation**: In `UIManager.cs`:
  ```csharp
  public void HideControlsPanel()
  {
      OnHideControlsRequested?.Invoke();
      OnHideSubPanelsRequested?.Invoke();
  }
  ```
- **Stress-Test Scenario**: `HideControlsPanel` invokes both `OnHideControlsRequested` and `OnHideSubPanelsRequested`. Could this cause the router stack to pop twice and exit the Main Menu?
- **Analysis**:
  In `UIPanelRouter.cs`:
  - `HandleHideControls()` checks `if (Current == UIPanelId.Controls) Back();`. Popping `Controls` changes `Current` to `Home`.
  - Next, `HandleHideSubPanels()` checks `if (Current == UIPanelId.Settings || Current == UIPanelId.Controls) Back();`. Since `Current` is now `Home`, the check evaluates to `false` and `Back()` is NOT invoked.
- **Result**: PASS (Stack safety preserved by current panel guard).
- **Recommendation (Minor)**: For future refactoring (T-201), unify sub-panel hide events to avoid redundant event dispatches.

#### Challenge 2: Startup Race Condition between GameManager and UIPanelRouter
- **Observation**: `BootstrapLoader` loads `BootstrapScene` and then `MainMenuScene`.
- **Stress-Test Scenario**: What if `GameManager` enters `eGameState.Menu` before `UIPanelRouter` initializes in `MainMenuScene`?
- **Analysis**:
  - `UIPanelRouter.Start()` explicitly inspects:
    ```csharp
    if (GameManager.Instance != null && m_Stack.Count == 0)
    {
        HandleGameStateChanged(GameManager.Instance.GameState);
    }
    ```
  - If `GameManager` has already transitioned to `Menu`, `Start()` synchronizes and shows `Home`.
  - If `MainMenuScreen.Start()` runs first, it registers `Home` and shows it, setting `m_Stack.Count == 1`, which prevents redundant re-initialization in `UIPanelRouter.Start()`.
- **Result**: PASS.

#### Challenge 3: Inadvertent Editor BatchMode Termination
- **Observation**: `StarVanguardValidator.ValidateAll()` previously called `EditorApplication.Exit` unconditionally.
- **Stress-Test Scenario**: A developer clicks `Star Vanguard > Validate Project` inside the Unity Editor GUI.
- **Analysis**: The new guard `if (Application.isBatchMode) EditorApplication.Exit(...)` prevents the Editor window from abruptly closing, logging results to the Console instead.
- **Result**: PASS.

---

## 4. Caveats

- **No Caveats**: All required deliverables (T-101a decoupling, T-101b asmdefs, T-101c validator & tests, static checks) have been inspected, tested, and validated.

---

## 5. Conclusion

**Verdict: `APPROVE`**

Milestone 2 meets all technical and architectural requirements:
- `StarVanguard.Runtime` is completely decoupled from `StarVanguard.UI`.
- 4 assembly definition files establish strict one-way compile boundaries.
- `StarVanguardValidator` correctly validates canonical scene order and additive gameplay loading.
- `AssemblySmokeTests` provides robust NUnit test coverage for architectural constraints.
- All three gates (G1: Compile, G2: Regression/Validator/Tests, G3: Static Check) pass 100%.

---

## 6. Verification Method

To independently reproduce this verification:

1. **Gate G3 (Static Check)**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expected*: `0 FAIL, 6 WARN, 17 OK` with `[ OK ] asmdef: 4 asmdef` and `[ OK ] unit_tests: Co test NUnit`.

2. **Gate G1 (Solution Compilation)**:
   ```powershell
   dotnet build "Star Vanguard\Star Vanguard.sln"
   ```
   *Expected*: 0 Errors, build succeeded.

3. **Gate G2 (StarVanguardValidator)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-quit", "-executeMethod", "StarVanguard.EditorTools.StarVanguardValidator.ValidateAll", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\validator_run.log`"" -Wait
   Get-Content "Star Vanguard\Temp\validator_run.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expected*: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`.

4. **Gate G2 (NUnit EditMode Tests)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-runTests", "-testPlatform", "EditMode", "-testResults", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\editmode_results.xml`"", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\editmode.log`"" -Wait
   Get-Content "Star Vanguard\Temp\editmode_results.xml" | Select-String -Pattern "testcasecount"
   ```
   *Expected*: `testcasecount="5" result="Passed" total="5" passed="5" failed="0"`.

5. **Gate G2 (PlayMode Smoke)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-executeMethod", "StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\smoke_run.log`"" -Wait
   Get-Content "Star Vanguard\Temp\smoke_run.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expected*: `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`.
