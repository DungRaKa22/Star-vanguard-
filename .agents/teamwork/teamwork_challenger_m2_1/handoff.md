# Empirical Challenge & Verification Report — Milestone 2 (T-101)

**Verdict**: `APPROVE`  
**Challenger**: Challenger M2-1 (Build Pipeline, Asmdef Boundaries & Static Checks)  
**Date**: 2026-10-30T09:18:30Z  

---

## Challenge Summary

**Overall risk assessment**: LOW

Empirical stress-testing of Milestone 2 confirmed that the assembly definitions, architectural decoupling, and build pipeline are fully sound and strictly enforced.
1. `dotnet build "Star Vanguard\Star Vanguard.sln"` cleanly restores and compiles all four projects (`StarVanguard.Runtime`, `StarVanguard.UI`, `StarVanguard.Editor`, `StarVanguard.Tests`) with **0 errors**.
2. Static project audit (`python Tools/check_project.py`) yields **0 FAIL, 6 WARN, 17 OK**, confirming `[ OK ] asmdef: 4 asmdef` and `[ OK ] unit_tests: Co test NUnit`.
3. Assembly boundary independence is absolute: all 49 runtime scripts contain **0 references** to `StarVanguard.UI` or any of its types.
4. Adversarial injection of an illegal UI reference in `StarVanguard.Runtime` caused compilation to fail with `error CS0234: The type or namespace name 'UI' does not exist in the namespace 'StarVanguard'`, proving that the compiler and assembly boundaries rigorously prevent boundary leaks.
5. In-engine verification: Unity batchmode script compilation succeeded with ExitCode 0; NUnit EditMode tests passed 5/5 (100%); `StarVanguardValidator` passed 37/37 (100%); `StarVanguardPlayModeSmoke` passed 55/55 (100%).

---

## 1. Observation

### 1.1 CLI MSBuild Pipeline (`dotnet build`)
- **Command executed**:
  ```powershell
  dotnet build "Star Vanguard\Star Vanguard.sln"
  ```
- **Verbatim Result**:
  ```
  Determining projects to restore...
  Nothing to do. None of the projects specified contain packages to restore.
  StarVanguard.Runtime -> C:\Documents\UNITY 2026\Star Vanguard\Temp\bin\Debug\StarVanguard.Runtime.dll
  StarVanguard.UI -> C:\Documents\UNITY 2026\Star Vanguard\Temp\bin\Debug\StarVanguard.UI.dll
  ...
  4 Warning(s)
  0 Error(s)
  Time Elapsed 00:00:04.57
  ```
- **Project-by-project verification**:
  - `StarVanguard.Runtime.csproj`: 0 Warnings, 0 Errors.
  - `StarVanguard.UI.csproj`: 0 Warnings, 0 Errors.
  - `StarVanguard.Editor.csproj`: 2 Warnings (MSB3277 unification on `System.Numerics.Vectors`), 0 Errors.
  - `StarVanguard.Tests.csproj`: 2 Warnings (MSB3277 unification on `System.Numerics.Vectors`), 0 Errors.
  - Full rebuild (`dotnet build "Star Vanguard\Star Vanguard.sln" /t:Rebuild`): 0 Errors, exited with code 0.

### 1.2 Gate G3 Static Analysis (`python Tools/check_project.py`)
- **Command executed**:
  ```powershell
  python Tools/check_project.py
  ```
- **Verbatim Result**:
  ```
  [ OK ] meta_missing: Moi asset deu co .meta
  [ OK ] meta_orphan: Khong co .meta mo coi
  [ OK ] guid_duplicate: Khong co GUID trung lap
  [ OK ] missing_script: Khong co component bi Missing Script
  [ OK ] class_file_name: 41 class MonoBehaviour/ScriptableObject deu trung ten file
  [ OK ] duplicate_type: Khong co type trung ten
  [ OK ] asmdef: 4 asmdef
  [ OK ] unit_tests: Co test NUnit
  [ OK ] build_scenes: Build order dung: BootstrapScene.unity -> MainMenuScene.unity -> GameplayScene.unity
  [WARN] template_scene: Con sot scene template URP trong Assets/Settings/Scenes (nen xoa)
  [ OK ] tags: Tag dung: ['Player']
  [ OK ] layers: Layer dung: ['Enemy', 'EnemyBullet', 'Player']
  [ OK ] input: activeInputHandler = Both; legacy=15, InputSystem=25
  [WARN] hud_duplication: UIManager tu AddComponent OverheatGaugeUI va OverheatGaugeUI ve HUD bang OnGUI voi m_EnableProceduralHUD = true -> co the ve trung HUD uGUI (score/lives/bombs) va cap phat GC moi frame
  [WARN] hot_path: 4 ham chay moi frame co thao tac ton kem
  [ OK ] camera_main: 23 lan goi Camera.main trong 11 file (nen cache trong 1 service)
  [ OK ] legacy_coop: Khong con object Co-op trong scene
  [WARN] perf_settings: vSyncCount = ['0', '0', '1', '1', '1', '1']
  [ OK ] perf_settings: Khong dat Application.targetFrameRate
  [WARN] scene_size: Scene qua lon
  [ OK ] dead_code: Khong co script chet ro rang
  [WARN] docs_cleanup: Khong co README.md o root
  [ OK ] code_noise: 2 Debug.Log, 0 TODO/FIXME trong Assets/Scripts

  Tong ket: 0 FAIL, 6 WARN, 17 OK
  ```

### 1.3 Assembly Independence & Code Separation
- **UI Type Extraction & Runtime Scan**:
  - Identified types in `Star Vanguard/Assets/Scripts/UI/*.cs`: `ControlsScreen`, `InputMap`, `MainMenuScreen`, `Row`, `SettingsScreen`, `UIPanelId`, `UIPanelRouter`, `UITheme`.
  - Scanned all 49 `.cs` files in `Star Vanguard/Assets/Scripts/*.cs` (outside `UI/`).
  - Result: **0 matches** found. No namespace references to `StarVanguard.UI`, and no references to any UI types.
- **Asmdef configuration verification**:
  - `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`: References only Unity packages (`Unity.TextMeshPro`, `UnityEngine.UI`, `Unity.InputSystem`, `Unity.RenderPipelines.Universal.Runtime`). No references to `StarVanguard.UI`.
  - `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`: References `StarVanguard.Runtime`. `autoReferenced` is set to `false`.
  - `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef`: References `StarVanguard.Runtime` and `StarVanguard.UI`. Platform restricted to `["Editor"]`.
  - `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`: References `StarVanguard.Runtime` and `StarVanguard.UI`. Platform restricted to `["Editor"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, `overrideReferences: true`, precompiled reference `nunit.framework.dll`.

### 1.4 Adversarial Injection Stress Test
- **Negative Test Scenario**:
  - Injected an illegal dependency `using StarVanguard.UI;` into `Star Vanguard/Assets/Scripts/CanvasAspectFitter.cs` (part of `StarVanguard.Runtime`).
  - Executed `dotnet build "Star Vanguard\StarVanguard.Runtime.csproj"`.
- **Verbatim Result**:
  ```
  C:\Documents\UNITY 2026\Star Vanguard\Assets\Scripts\CanvasAspectFitter.cs(2,20): error CS0234: The type or namespace name 'UI' does not exist in the namespace 'StarVanguard' (are you missing an assembly reference?) [C:\Documents\UNITY 2026\Star Vanguard\StarVanguard.Runtime.csproj]

  Build FAILED.
      0 Warning(s)
      1 Error(s)
  ```
  - Reverted edit cleanly immediately after verification; git working copy returned to 100% clean state.

### 1.5 Unity Engine Batchmode Compilation & Execution
- **Batchmode Script Compilation Log (`Temp/challenger_editmode.log`)**:
  - `DisplayProgressbar: Compiling Scripts`
  - `ExitCode: 0 Duration: 1s393ms`
  - `CompileScripts: 2426.278ms`
- **NUnit EditMode Test Suite (`Temp/challenger_editmode_results.xml`)**:
  - `<test-run id="2" testcasecount="5" result="Passed" total="5" passed="5" failed="0">`
  - Tests passing:
    1. `ArchitecturalConstraint_RuntimeDoesNotReferenceUI`: Passed
    2. `ArchitecturalConstraint_UIDependsOnRuntime`: Passed
    3. `EnumsAndConstants_AreConsistent`: Passed
    4. `RuntimeAssembly_LoadsExpectedCoreTypes`: Passed
    5. `UIAssembly_LoadsExpectedUITypes`: Passed
- **Project Validator (`StarVanguardValidator.ValidateAll`)**:
  - Log: `Temp/validator_cmd.log`
  - Result: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`
- **PlayMode Smoke Suite (`StarVanguardPlayModeSmoke.Run`)**:
  - Log: `Temp/challenger_smoke.log`
  - Result: `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`

---

## 2. Logic Chain

1. **Step 1 (Clean Pipeline Build)**: Observation 1.1 proved that the .NET solution builds cleanly across all targets with 0 errors. The only compiler diagnostic is a minor MSB3277 version unification for `System.Numerics.Vectors` between Unity reference assemblies and .NET standard facades, which does not impede build success.
2. **Step 2 (Static Gate G3 Compliance)**: Observation 1.2 confirmed that `python Tools/check_project.py` reports 0 FAIL, 6 WARN, and 17 OK. The two key checks (`[ OK ] asmdef: 4 asmdef` and `[ OK ] unit_tests: Co test NUnit`) passed cleanly.
3. **Step 3 (Strict Decoupling Verification)**: Observation 1.3 showed that static scanning across all 49 runtime scripts detected zero imports or usages of UI types or the `StarVanguard.UI` namespace. The decoupling via `UIManager` static events is complete.
4. **Step 4 (Adversarial Negative Proof)**: Observation 1.4 demonstrated that any attempt by a runtime script to reference `StarVanguard.UI` immediately halts compilation with `CS0234`. This proves that the boundary is not merely cosmetic or conventional, but enforced by the compiler.
5. **Step 5 (Full Runtime & Engine Verification)**: Observation 1.5 demonstrated that within the Unity 6000.5.7f1 engine runtime, compilation exited cleanly (ExitCode 0), NUnit EditMode tests passed 5/5, project validator passed 37/37, and PlayMode smoke tests passed 55/55.
6. **Step 6 (Synthesis)**: All requirements R4 (T-101a, T-101b, T-101c) of Sprint 0 are satisfied under empirical scrutiny.

---

## 3. Caveats

- **No Caveats**: All checks were directly and independently run in the local Windows environment. Negative testing was verified and restored without leaving dirty files.

---

## 4. Conclusion & Verdict

**VERDICT: `APPROVE`**

Milestone 2 (T-101: One-Way Assembly Definitions & Architecture Decoupling) satisfies all architectural, compilation, and safety requirements:
- The assembly definitions partition the codebase into strict one-way layers (`Runtime` -> independent; `UI` -> `Runtime`; `Editor` -> `Runtime`, `UI`; `Tests` -> `Runtime`, `UI`).
- CLI MSBuild, Unity batchmode, NUnit EditMode tests, and project validators all pass 100% with zero regressions.

---

## 5. Verification Method

To independently reproduce and verify this report:

1. **CLI Solution Build**:
   ```powershell
   dotnet build "Star Vanguard\Star Vanguard.sln"
   ```
   *Expected*: `0 Error(s)`, Build Succeeded.

2. **Static Project Audit**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expected*: `0 FAIL, 6 WARN, 17 OK`, with `[ OK ] asmdef` and `[ OK ] unit_tests`.

3. **Negative Boundary Test**:
   Inject `using StarVanguard.UI;` into any script in `Star Vanguard/Assets/Scripts/` (e.g. `CanvasAspectFitter.cs`) and run:
   ```powershell
   dotnet build "Star Vanguard\StarVanguard.Runtime.csproj"
   ```
   *Expected*: Fails compilation with `error CS0234`. Revert changes.

4. **NUnit EditMode Tests**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-runTests", "-testPlatform", "EditMode", "-testResults", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\challenger_editmode_results.xml`"", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\challenger_editmode.log`"" -Wait
   Get-Content "Star Vanguard\Temp\challenger_editmode_results.xml" | Select-String -Pattern "testcasecount"
   ```
   *Expected*: `testcasecount="5" result="Passed" total="5" passed="5" failed="0"`.

5. **StarVanguardValidator**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-quit", "-executeMethod", "StarVanguard.EditorTools.StarVanguardValidator.ValidateAll", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\validator_cmd.log`"" -Wait
   Get-Content "Star Vanguard\Temp\validator_cmd.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expected*: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`.
