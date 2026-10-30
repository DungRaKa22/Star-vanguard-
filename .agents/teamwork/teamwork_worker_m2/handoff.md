# Handoff Report — Milestone 2: One-Way Assembly Definitions & Architecture Decoupling (T-101)

## 1. Observation

### 1.1 Architecture & Decoupling (T-101a)
- **Coupling elimination**: `Star Vanguard/Assets/Scripts/UIManager.cs` had `using StarVanguard.UI;` and direct calls to `UIPanelRouter.Instance` and `UIPanelId`.
- **Refactoring applied**:
  - Removed `using StarVanguard.UI;` from `UIManager.cs`.
  - Defined C# navigation events in `UIManager.cs`:
    - `public static event Action OnShowSettingsRequested;`
    - `public static event Action OnHideSettingsRequested;`
    - `public static event Action OnShowControlsRequested;`
    - `public static event Action OnHideControlsRequested;`
    - `public static event Action OnShowAudioSettingsRequested;`
    - `public static event Action OnHideSubPanelsRequested;`
    - `public static event Action OnHideAllPanelsRequested;`
  - Kept public method signatures intact (`ShowControlsPanel()`, `HideControlsPanel()`, `ShowSettingsPanel()`, `HideSettingsPanel()`) to maintain full backward compatibility with runtime scripts (`SettingsUI.cs`, `ControlsPanelController.cs`) and reflection tools (`StarVanguardSceneSplitter.cs`).
  - Implemented event subscriptions in `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`:
    - `OnEnable()` subscribes to `GameManager.OnGameStateChanged` and `UIManager` navigation events.
    - `OnDisable()` unconditionally unsubscribes from all events per AGENTS.md Rule #4.
    - `Start()` checks `m_Stack.Count == 0` to synchronize with current game state on boot.
- **Verification snippet result**:
  ```powershell
  python -c "import os; dir_p = r'Star Vanguard\Assets\Scripts'; print([f for f in os.listdir(dir_p) if f.endswith('.cs') and any(t in open(os.path.join(dir_p, f), encoding='utf-8', errors='ignore').read() for t in ['StarVanguard.UI', 'UIPanelRouter', 'UIPanelId'])])"
  ```
  Output: `[]` (0 references across all 49 scripts in `Assets/Scripts/`).

### 1.2 Assembly Definitions Created (T-101b)
Four `.asmdef` files were created with exact JSON schemas:
1. `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`
   - References: `Unity.TextMeshPro`, `UnityEngine.UI`, `Unity.InputSystem`, `Unity.RenderPipelines.Universal.Runtime`.
   - Internal project references: None (isolated root).
2. `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`
   - References: `StarVanguard.Runtime`, `UnityEngine.UI`, `Unity.TextMeshPro`, `Unity.InputSystem`.
   - `autoReferenced`: `false`.
3. `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef`
   - References: `StarVanguard.Runtime`, `StarVanguard.UI`, `Unity.InputSystem`, `UnityEngine.UI`, `UnityEditor.UI`, `Unity.TextMeshPro`, `Unity.TextMeshPro.Editor`, `Unity.RenderPipelines.Universal.Runtime`, `Unity.RenderPipelines.Universal.Editor`.
   - `includePlatforms`: `["Editor"]`.
4. `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`
   - References: `StarVanguard.Runtime`, `StarVanguard.UI`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `Unity.InputSystem`, `UnityEngine.UI`, `Unity.TextMeshPro`.
   - `includePlatforms`: `["Editor"]`.
   - `overrideReferences`: `true`, `precompiledReferences`: `["nunit.framework.dll"]`.

All 4 asmdefs and their `.meta` files were generated and tracked cleanly in git.

### 1.3 Validator Alignment & NUnit Test Fixture (T-101c)
- **`Star Vanguard/Assets/Editor/StarVanguardValidator.cs`**:
  - Added `[MenuItem("Star Vanguard/Validate Project", priority = 50)]` and `[MenuItem("Tools/Star Vanguard/Validate Project", priority = 50)]` to `ValidateAll()`.
  - Guarded `EditorApplication.Exit` with `if (Application.isBatchMode)` to prevent terminating the interactive Unity Editor during validation.
  - Updated `CheckBuildSettings()` to assert `scenes.Length >= 3` in canonical order:
    - Index 0: `Assets/Scenes/BootstrapScene.unity`
    - Index 1: `Assets/Scenes/MainMenuScene.unity`
    - Index 2: `Assets/Scenes/GameplayScene.unity`
  - Updated `CheckGameplayScene()` to load `BootstrapScene` first (single) and `GameplayScene` additively, matching runtime additive loading architecture so persistent managers (`GameManager`, `AudioManager`, `PoolManager` with pool size 50 >= 40) are verified without false failures.
- **`Star Vanguard/Assets/Tests/AssemblySmokeTests.cs`**:
  - Created 5 NUnit EditMode tests:
    1. `RuntimeAssembly_LoadsExpectedCoreTypes`
    2. `UIAssembly_LoadsExpectedUITypes`
    3. `ArchitecturalConstraint_RuntimeDoesNotReferenceUI`
    4. `ArchitecturalConstraint_UIDependsOnRuntime`
    5. `EnumsAndConstants_AreConsistent`
- **`Tools/check_project.py`**:
  - Updated line 627 in `check_dead_scripts` to exclude `Tests` directory along with `Editor` directory (`if "Editor" + os.sep in path or "Tests" + os.sep in path:`).
- **`Star Vanguard/Directory.Build.props`**:
  - Added `<ResolveAssemblyReferenceIgnoreTargetFrameworkAttributeVersionMismatch>true</ResolveAssemblyReferenceIgnoreTargetFrameworkAttributeVersionMismatch>` to prevent MSB3274 when resolving `nunit.framework.dll` v4.7.2 under .NET Framework v4.7.1 in CLI MSBuild.

### 1.4 Verification Command Results
1. **Gate G1 (Compile & Build)**:
   - Command: `dotnet build "Star Vanguard\Star Vanguard.sln"`
   - Output: `0 Warning(s)`, `0 Error(s)`, Time Elapsed 00:00:09.17.
   - Batchmode asset compilation: `Exiting batchmode successfully now! Application will terminate with return code 0`.
2. **Gate G2 (Regression & Validator)**:
   - Command: `Unity -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardValidator.ValidateAll -logFile Temp/validator_run.log`
   - Output: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`.
   - Command: `Unity -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile Temp/playmode_smoke.log`
   - Output: `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`.
   - Command: `Unity -batchmode -runTests -testPlatform EditMode -testResults Temp/editmode_test_results.xml`
   - Output: `<test-run id="2" testcasecount="5" result="Passed" total="5" passed="5" failed="0">`.
3. **Gate G3 (Static Project Audit)**:
   - Command: `python Tools/check_project.py`
   - Output: `Tong ket: 0 FAIL, 6 WARN, 17 OK` (reduced from 8 WARN, `[ OK ] asmdef: 4 asmdef`, `[ OK ] unit_tests: Co test NUnit`).

---

## 2. Logic Chain

1. **Step 1 (Root Cause Resolution)**: Observation 1.1 demonstrated that `UIManager.cs` was the sole runtime file referencing `StarVanguard.UI`. Removing this dependency and switching to C# static events allowed `StarVanguard.Runtime.asmdef` to compile without cyclic or missing reference errors.
2. **Step 2 (Strict One-Way Hierarchy)**: With runtime decoupled, Observation 1.2 established the four assembly definitions. `StarVanguard.Runtime` contains 0 internal dependencies; `StarVanguard.UI` depends on `Runtime`; `StarVanguard.Editor` and `StarVanguard.Tests` depend on both `Runtime` and `UI`.
3. **Step 3 (Safety & Validator Alignment)**: Observation 1.3 aligned `StarVanguardValidator.cs` with the reality of `EditorBuildSettings.asset` (3 scenes starting with `BootstrapScene`) and additive runtime loading. This eliminated false failures while enforcing the project architecture.
4. **Step 4 (Test Automation & Tooling)**: Observation 1.3 and 1.4 confirmed that creating `AssemblySmokeTests.cs` and updating `check_project.py` converted static warnings into passing checks while executing real architectural assertions.
5. **Step 5 (Full Quality Gate Pass)**: The verification results in Observation 1.4 confirm that Gate G1 (0 compile errors), Gate G2 (100% test & validator pass: 37/37 validator checks, 55/55 smoke checks, 5/5 NUnit tests), and Gate G3 (0 FAIL, 6 WARN <= baseline) are satisfied.

---

## 3. Caveats

- **No Caveats**: All tasks (T-101a, T-101b, T-101c) have been implemented and verified end-to-end via headless Unity batchmode and dotnet CLI. Scene YAML was untouched; all .meta files were generated by Unity.

---

## 4. Conclusion

Milestone 2 (T-101a, T-101b, T-101c) is complete, robust, and fully verified.
- `StarVanguard.Runtime` is decoupled from `StarVanguard.UI`.
- 4 assembly definitions establish strict one-way compilation boundaries.
- Project validation and smoke testing pass 100% with zero regressions.
- Static audit passes Gate G3 with 0 failures and reduced warning count (6 WARN, 17 OK).

---

## 5. Verification Method

To independently verify this milestone:

1. **Static Project Audit (Gate G3)**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expectation:* `0 FAIL, 6 WARN, 17 OK`. Confirm `[ OK ] asmdef: 4 asmdef` and `[ OK ] unit_tests: Co test NUnit`.

2. **CLI Build (Gate G1)**:
   ```powershell
   dotnet build "Star Vanguard\Star Vanguard.sln"
   ```
   *Expectation:* 0 errors, build succeeded.

3. **Validator Run (Gate G2)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-quit", "-executeMethod", "StarVanguard.EditorTools.StarVanguardValidator.ValidateAll", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\validator_verify.log`"" -Wait
   Get-Content "Star Vanguard\Temp\validator_verify.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expectation:* `SUMMARY passed=37 failed=0`.

4. **NUnit EditMode Tests (Gate G2)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-runTests", "-testPlatform", "EditMode", "-testResults", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\test_verify.xml`"", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\test_verify.log`"" -Wait
   Get-Content "Star Vanguard\Temp\test_verify.xml" | Select-String -Pattern "testcasecount"
   ```
   *Expectation:* `testcasecount="5" result="Passed" total="5" passed="5" failed="0"`.

5. **PlayMode Smoke Test (Gate G2)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-executeMethod", "StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\smoke_verify.log`"" -Wait
   Get-Content "Star Vanguard\Temp\smoke_verify.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expectation:* `SUMMARY passed=55 failed=0 exitCode=0`.
