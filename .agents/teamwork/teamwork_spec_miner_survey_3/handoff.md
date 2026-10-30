# Specification Survey Report: Verification Gates & Tooling (Sprint 0)

## Features Discovered

| # | Category | Feature | Description | Inputs | Outputs | Error Behavior | Discovered Via |
|---|----------|---------|-------------|--------|---------|----------------|----------------|
| 1 | Gate G1: Compilation | `dotnet build` C# Compilation | Headless compilation of project C# solution containing runtime and editor assemblies. | `Star Vanguard/Star Vanguard.sln` | Build log, DLL artifacts (`Assembly-CSharp.dll`, `Assembly-CSharp-Editor.dll`). | Non-zero exit code, compiler errors `error CS...`. | Direct CLI execution (`dotnet build`) |
| 2 | Gate G1: Compilation | Unity AssetDatabase Auto-Refresh & Compile | Unity Editor file watcher detects script/asset changes on disk and triggers internal compiler pipeline. | Modified `.cs` or `.meta` files | Domain reload, recompiled script assemblies. | Safe Mode boot, compiler errors logged to `Editor.log`. | `Star Vanguard/Logs/Editor.log` inspection |
| 3 | Gate G1: Compilation | Unity Project Editor Log Monitoring | Real-time tracking of Unity compilation and runtime errors in dedicated project log file. | Unity internal logger | Log stream at `Star Vanguard/Logs/Editor.log`. | Records `[ScriptCompilation]`, `CompileScripts: ... ms`, errors. | `Star Vanguard/Logs/Editor.log` |
| 4 | Gate G2: Regression | `StarVanguardValidator.ValidateAll` | Headless wiring validator for prefabs, missing scripts, build settings, and gameplay scene. | Unity Editor state, assets | Exit code 0 on pass, exit code 1 on fail; console logs `[STARVANGUARD-CHECK]`. | Exits process with code 1 via `EditorApplication.Exit(1)`. | `Star Vanguard/Assets/Editor/StarVanguardValidator.cs` |
| 5 | Gate G2: Regression | `StarVanguardValidator.CheckPlayerPrefab` | Asserts existence and configuration of Player prefab components and WeaponHeatController parameters. | `Assets/Prefabs/Player_Prefab.prefab` | Assert logs for 6 core player components, tag, and 4 heat parameters. | Increments `s_Failed`, logs FAIL with expected vs actual values. | `StarVanguardValidator.cs:43-81` |
| 6 | Gate G2: Regression | `StarVanguardValidator.CheckBossPrefab` | Asserts existence of Boss prefab and core boss components (`BossController`, `BossShooting`, `BossMover`). | `Assets/Prefabs/Boss_Prefab.prefab` | Assert logs for boss components. | Increments `s_Failed`, logs FAIL if missing. | `StarVanguardValidator.cs:83-91` |
| 7 | Gate G2: Regression | `StarVanguardValidator.CheckMissingScripts` | Scans all prefabs in `Assets/Prefabs` to detect any broken MonoScript references (`null` MonoBehaviour). | `Assets/Prefabs/*.prefab` | Assert logs: prefab count and broken count. | Logs `FAIL missing script reference inside {path}`, increments `s_Failed`. | `StarVanguardValidator.cs:93-120` |
| 8 | Gate G2: Regression | `StarVanguardValidator.CheckBuildSettings` | Validates scene count and ordering in `EditorBuildSettings.scenes`. | `EditorBuildSettings.scenes` | Assert logs for scene count and paths. | Increments `s_Failed`, logs FAIL if order mismatch. *(Note: currently expects index 0 = MainMenuScene)*. | `StarVanguardValidator.cs:122-134` |
| 9 | Gate G2: Regression | `StarVanguardValidator.CheckGameplayScene` | Opens `GameplayScene.unity` and asserts manager singletons, WaveDirector references, and PoolManager size. | `Assets/Scenes/GameplayScene.unity` | Assert logs for managers, WaveDirector fields, PoolManager size >= 40. | Increments `s_Failed`, logs FAIL if singletons or pool size deficient. | `StarVanguardValidator.cs:136-165` |
| 10 | Gate G2: Regression | `StarVanguardPlayModeSmoke.Run` | Headless Play Mode smoke test simulating full player loop: Bootstrap -> MainMenu -> UI navigation -> Gameplay -> Wave spawn -> Weapon heat. | Boot from `BootstrapScene.unity` in Play Mode | Step logs `[STARVANGUARD-SMOKE]`, dumps hierarchy, asserts runtime behavior. | Exit code 1 on failure, exit code 2 on watchdog timeout (120s). | `Star Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs` |
| 11 | Gate G2: Regression | UI Raycast & RaycastTarget Audit | In PlayMode smoke, asserts that UI buttons (`LaunchButton`, `BackButton`) are top-most clickable raycast targets without occlusion. | GraphicRaycaster, PointerEventData | RaycastResult list check. | `Assert(IsTopMostClickable(btn))` fails if occluded by overlays. | `StarVanguardPlayModeSmoke.cs:390-409` |
| 12 | Gate G2: Regression | Weapon Overheat Runtime Jam Simulation | Fires 40 consecutive simulated shots during Play Mode to verify jamming, heat gauge cap (>= 0.999f), and ResetHeat recovery. | `WeaponHeatController.TryFire(1)` | Heat gauge assertions and refusal to fire while overheated. | Fails if weapon fires while overheated or fails to reset. | `StarVanguardPlayModeSmoke.cs:534-551` |
| 13 | Gate G3: Static Audit | `Tools/check_project.py` CLI Runner | Standalone Python 3 static auditor inspecting meta integrity, GUIDs, scenes, scripts, and settings without requiring Unity. | Project directory path (`--project`), optional `--json`, `--quiet` | Console report: `[ OK ]`, `[WARN]`, `[FAIL]`, summary line; optional JSON export. | Exit code 0 if 0 FAIL; exit code 1 if >= 1 FAIL; exit code 2 if directory invalid. | `Tools/check_project.py` |
| 14 | Gate G3: Static Audit | `meta_missing` & `meta_orphan` Checks | Detects assets without `.meta` files, or orphan `.meta` files without underlying assets. | `Assets/` directory tree | Lists missing or orphan files. | Reports level `FAIL` if count > 0. | `check_project.py:150-179` |
| 15 | Gate G3: Static Audit | `guid_duplicate` Check | Detects duplicate GUIDs across all `.meta` files in `Assets/`. | Parsed GUID map | Duplicate GUID mapping. | Reports level `FAIL` if duplicates found. | `check_project.py:180-195` |
| 16 | Gate G3: Static Audit | `missing_script` YAML Reference Check | Scans `.unity` and `.prefab` YAML files for unresolvable `m_Script` GUIDs (excluding known package GUIDs). | YAML asset text | Broken script GUID -> referencing files. | Reports level `FAIL` if missing scripts found. | `check_project.py:199-241` |
| 17 | Gate G3: Static Audit | `class_file_name` Rule Check | Ensures every `MonoBehaviour`, `Singleton<T>`, `NetworkBehaviour`, or `ScriptableObject` class name matches its `.cs` file name. | C# source files | Mismatch list. | Reports level `FAIL` if mismatch found. | `check_project.py:243-274` |
| 18 | Gate G3: Static Audit | `mirror_package` & `mirror_define` Rules | Detects leftover `#if MIRROR` code and verifies whether Mirror package and compiler symbols are configured. | `Packages/manifest.json`, C# source, `.asmdef`, `.rsp` | Guarded file list. | Reports `FAIL` if `#if MIRROR` exists without Mirror package; `WARN` if define missing. | `check_project.py:276-308` |
| 19 | Gate G3: Static Audit | `asmdef` & `unit_tests` Discovery | Audits existence of `.asmdef` files and NUnit `[Test]`/`[UnityTest]` test fixtures in `Assets/`. | `.asmdef` files, C# test annotations | Asmdef count and NUnit presence. | Reports `WARN` if 0 `.asmdef` or 0 NUnit tests. | `check_project.py:330-357` |
| 20 | Gate G3: Static Audit | `build_scenes` Ordering Verification | Validates `EditorBuildSettings.asset` existence, valid scene paths, and asserts scene index 0 contains "Bootstrap". | `ProjectSettings/EditorBuildSettings.asset` | Build scene order string. | Reports `FAIL` if index 0 is not Bootstrap or scene files missing. | `check_project.py:359-382` |
| 21 | Gate G3: Static Audit | `tags` & `layers` Runtime Safety Check | Scans C# code for `CompareTag` and `NameToLayer` calls and verifies against `TagManager.asset`. | `ProjectSettings/TagManager.asset`, C# code | List of validated tags and layers. | Reports `FAIL` if an undeclared tag or layer is referenced in code. | `check_project.py:384-418` |
| 22 | Gate G3: Static Audit | `input` Handler Mode Verification | Checks `activeInputHandler` in `ProjectSettings.asset` and flags legacy `Input.Get*` vs New Input System calls. | `ProjectSettings.asset`, C# code | Call counts. | Reports `WARN` if legacy calls exist without Input System adoption. | `check_project.py:420-446` |
| 23 | Gate G3: Static Audit | `hud_duplication` IMGUI Overheat Check | Flags if `UIManager.cs` dynamically adds `OverheatGaugeUI` while `OverheatGaugeUI` renders procedural IMGUI HUD (`OnGUI`). | `UIManager.cs`, `OverheatGaugeUI.cs` | Duplicate HUD warning. | Reports `WARN` if procedural IMGUI HUD is active. | `check_project.py:448-467` |
| 24 | Gate G3: Static Audit | `hot_path` Frame-Loop Performance Check | Scans `Update`, `FixedUpdate`, `LateUpdate`, `OnGUI` for allocations (`new[]`, `$"`) or expensive lookups (`Find*`, `Camera.main`). | C# methods | List of offending methods and operations. | Reports `WARN` if expensive calls detected in hot loops. | `check_project.py:469-507` |
| 25 | Gate G3: Static Audit | `legacy_coop` Scene Artifact Check | Scans scene YAML files for leftover LAN co-op GameObjects (`CoopPanel`, `CoopCard`, `CoopButton`). | `.unity` files | Offending scene and object names. | Reports `WARN` with remediation menu instruction. | `check_project.py:567-586` |
| 26 | Gate G3: Static Audit | `perf_settings` & `scene_size` Checks | Audits `QualitySettings.asset` vSync settings, missing target frame rate, and YAML scene size (> 3000 lines). | Project settings, `.unity` line counts | Offending scenes and vSync configurations. | Reports `WARN` if unconstrained framerate or bloated scenes. | `check_project.py:588-618` |
| 27 | Gate G3: Static Audit | `ci_summary.py` GitHub Action Formatter | Converts `check_project.py --json` output into GitHub Actions Step Summary Markdown format with collapsible details. | JSON audit report path | Formatted Markdown output. | Emits `❌ có lỗi cần sửa` or `✅ sạch` with summary tables; exit 0. | `Tools/ci_summary.py` |
| 28 | Editor Maintenance | `StarVanguardCoopCleanup.cs` Menu Tools | Editor menu tool to remove dead Co-op UI objects (`CoopPanel`, `CoopCard`, `CoopButton`) safely via Editor Undo API. | Open scenes or `MainMenuScene.unity` | Objects deleted, scenes marked dirty/saved. | Safe/idempotent; warns if run in Play Mode. | `Star Vanguard/Assets/Editor/StarVanguardCoopCleanup.cs` |
| 29 | Editor Maintenance | `StarVanguardBootstrapSetup.cs` | Establishes dedicated `BootstrapScene.unity` for persistent managers and strips duplicates from other scenes. | `MainMenuScene.unity`, `GameplayScene.unity` | Creates `BootstrapScene.unity`, updates `EditorBuildSettings`. | Overwrites existing `BootstrapScene.unity`. | `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs` |
| 30 | Editor Maintenance | `StarVanguardUIAudit.cs` | Traverses UI hierarchies in `MainMenuScene` or `GameplayScene` to log sizes, anchors, raycast blockers, and canvas groups. | Scene path (`MainMenuScene`, `GameplayScene`) | Console hierarchy dump. | Logs "no Canvas found" if canvas missing. | `Star Vanguard/Assets/Editor/StarVanguardUIAudit.cs` |

---

## Edge Cases

| # | Feature | Input | Observed Behavior |
|---|---------|-------|-------------------|
| 1 | `StarVanguardValidator.CheckBuildSettings` | `EditorBuildSettings.asset` with `BootstrapScene` at index 0, `MainMenuScene` at index 1, `GameplayScene` at index 2. | **Validation Failure**: Validator asserts `scenes[0].path.EndsWith("MainMenuScene.unity")` and `scenes[1].path.EndsWith("GameplayScene.unity")`. Because `BootstrapScene` was inserted at index 0, this check fails 2 assertions unless updated. |
| 2 | `StarVanguardValidator.ValidateAll` | Execution via MCP `execute_menu_item StarVanguard.EditorTools.StarVanguardValidator.ValidateAll` | **Execution Failure**: `StarVanguardValidator.cs` contains NO `[MenuItem]` attribute on `ValidateAll()`. `execute_menu_item` fails to find any matching menu item. It can only be executed via `-executeMethod` CLI. |
| 3 | Unity Batchmode Execution | Running `Unity.exe -batchmode` when Unity Editor GUI (PID 9280) is already open | **File Lock Collision**: Unity locks `Temp/UnityLockfile`. A second Unity process cannot open the project simultaneously. Batchmode runs require closing the GUI Editor or running commands through Editor tools/eval/CLI. |
| 4 | Unity CLI (`unity status`) | Running `unity status` against running Editor (PID 9280) | **Missing Package**: Returns `STATUS_NO_INSTANCES` with message "No Unity Editor instances found with the Pipeline package installed" because `com.unity.pipeline` is not in `manifest.json`. |
| 5 | `dotnet build` C# Compilation | Compiling `Star Vanguard.sln` on modern .NET SDK | Emits 18 framework version warnings (MSB3274 / MSB3275: nunit.framework v4.7.2 vs v4.7.1), but outputs 0 errors and succeeds in ~7.6 seconds. |
| 6 | `check_project.py` Scene 0 Rule | `EditorBuildSettings.asset` scene index 0 | Checks `if "Bootstrap" not in os.path.basename(scenes[0])`. Fails with `[FAIL]` if `BootstrapScene` is not first. This directly contrasts with `StarVanguardValidator`'s hardcoded check for `MainMenuScene`. |
| 7 | `check_project.py` Baseline | Running `python Tools/check_project.py` on current repository state | Output: `0 FAIL, 10 WARN, 13 OK`. Exit code 0. Exactly 10 specific warnings exist and constitute the safety net baseline. |
| 8 | `StarVanguardPlayModeSmoke` Batchmode Repaint | Running Play Mode smoke test in `-batchmode` | In headless batchmode, `EditorApplication.isPlaying` does not repaint windows automatically, freezing `Time.time`. Handled gracefully by explicit `EditorApplication.isPaused = true; EditorApplication.Step();` calls in `Tick()`. |
| 9 | `StarVanguardCoopCleanup` Play Mode Protection | Clicking menu item while `EditorApplication.isPlayingOrWillChangePlaymode` is true | Shows dialog "Exit Play Mode first" and aborts cleanly without corrupting scenes. |
| 10 | `check_project.py` Missing Meta Detection | Any file in `Assets/` without a `.meta` file | Immediately flags `meta_missing` as `[FAIL]` and exits with code 1. |

---

## 5-Component Handoff Report

### 1. Observation

1. **Gate G1 (Compilation):**
   - The solution `Star Vanguard/Star Vanguard.sln` contains `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj`.
   - Execution command: `dotnet build "Star Vanguard\Star Vanguard.sln"`:
     - Result: `Time Elapsed 00:00:07.63`, `18 Warning(s)`, `0 Error(s)`.
   - The Unity Editor is running (PID 9280, Unity 6000.5.7f1) on `C:\Documents\UNITY 2026\Star Vanguard`.
   - Unity writes its active log to `Star Vanguard/Logs/Editor.log`.
   - Inspection of `Star Vanguard/Logs/Editor.log`:
     - Line 267: `[ScriptCompilation] Requested script compilation because: AssetDatabase observed changes...`
     - Line 353: `Finished compiling graph: 1136 nodes, 22991 flattened edges...`
     - Line 381: `AssetDatabase: script compilation time: 23.274160s`
     - Line 432: `CompileScripts: 23278.062ms`
     - Zero compilation errors present.

2. **Gate G2 (Regression & Validator):**
   - File: `Star Vanguard/Assets/Editor/StarVanguardValidator.cs`.
     - Static class: `StarVanguard.EditorTools.StarVanguardValidator`.
     - Entry method: `public static void ValidateAll()`.
     - Exit mechanism (Line 38): `EditorApplication.Exit(s_Failed == 0 ? 0 : 1);`.
     - Checked methods:
       - `CheckPlayerPrefab()`: Prefab load, 6 components, "Player" tag, `WeaponHeatController` parameters (`m_BaseHeatPerShot = 0.045f`, `m_HeatPerLevel = 0.005f`, `m_CoolingRate = 0.35f`, `m_CoolDownDelay = 0.25f`), and `PlayerShooting.m_HeatController` assignment.
       - `CheckBossPrefab()`: Prefab load, `BossController`, `BossShooting`, `BossMover`.
       - `CheckMissingScripts()`: All prefabs in `Assets/Prefabs`, scans for `null` MonoBehaviour components.
       - `CheckBuildSettings()`: Asserts `scenes.Length >= 2`, `scenes[0]` is `MainMenuScene.unity`, `scenes[1]` is `GameplayScene.unity`.
       - `CheckGameplayScene()`: Opens `Assets/Scenes/GameplayScene.unity`, asserts `GameManager`, `UIManager`, `AudioManager`, `WaveDirector` (with valid references), and `PoolManager` (`m_EnemyPoolSize >= 40`).
     - **Discrepancy 1**: Lines 23-24 have no `[MenuItem]` attribute. MCP `execute_menu_item` will fail.
     - **Discrepancy 2**: Lines 128-133 assert `scenes[0]` is `MainMenuScene.unity`. However, `Star Vanguard/ProjectSettings/EditorBuildSettings.asset` currently has:
       - Scene 0: `Assets/Scenes/BootstrapScene.unity`
       - Scene 1: `Assets/Scenes/MainMenuScene.unity`
       - Scene 2: `Assets/Scenes/GameplayScene.unity`
       Running `StarVanguardValidator.ValidateAll()` against the current project will fail on `CheckBuildSettings`.
   - File: `Star Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs`:
     - Method: `public static void Run()`. Exits 0 on success, 1 on failure, 2 on 120s watchdog timeout.
     - Simulates full loop: `BootstrapScene` -> `MainMenuScene` -> UI panels (`Controls`, `Settings`, `Home`) -> `GameplayScene` -> live wave spawning -> weapon overheating (40 shots) and cooling reset.
   - NUnit tests:
     - Search in `Star Vanguard/Assets/` found 0 test files (`Assets/Tests/` does not exist yet).

3. **Gate G3 (Static Audit):**
   - File: `Tools/check_project.py` (712 lines).
   - Execution command: `python Tools/check_project.py`.
   - Live execution output:
     `Star Vanguard audit -> C:\Documents\UNITY 2026\Star Vanguard`
     `Tong ket: 0 FAIL, 10 WARN, 13 OK`
     Exit code: 0.
   - The 10 baseline `WARN` items:
     1. `asmdef`: No Assembly Definition files exist in `Assets/`.
     2. `unit_tests`: `com.unity.test-framework` present but no NUnit tests in code.
     3. `template_scene`: `Assets/Settings/Scenes/URP2DSceneTemplate.unity` leftover template scene.
     4. `input`: `activeInputHandler = Both`; 15 legacy Input calls, 0 Input System calls.
     5. `hud_duplication`: `UIManager` adds `OverheatGaugeUI` and `OverheatGaugeUI` runs IMGUI `OnGUI` procedural HUD.
     6. `hot_path`: 4 hot frame methods with expensive operations (`CanvasAspectFitter`, `EnemyMover`, `OverheatGaugeUI`, `ParallaxLayer`).
     7. `legacy_coop`: MainMenuScene contains leftover Co-op objects (`CoopPanel`, `CoopCard`, `CoopButton`).
     8. `perf_settings`: `vSyncCount = ['0', '0', '1', '1', '1', '1']` and no `targetFrameRate`.
     9. `scene_size`: Large scene files (`GameplayScene.unity` 7,290 lines; `MainMenuScene.unity` 12,808 lines).
     10. `docs_cleanup`: Missing `README.md` at root.
   - File: `Tools/ci_summary.py` (65 lines). Formats JSON audit report into Markdown for GitHub Actions `$GITHUB_STEP_SUMMARY`.

4. **Environment Infrastructure:**
   - Unity Hub and Unity Editor 6000.5.7f1 installed at `C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe`.
   - Local port 8080 (`TcpTestSucceeded : False`): Unity MCP server (HTTP bridge) is not currently running.
   - `Temp/UnityLockfile` is `True`, indicating the project is locked by the active GUI Unity instance (PID 9280).

---

### 2. Logic Chain

1. **Gate G1 Verification Path:**
   - Because `Temp/UnityLockfile` is held by PID 9280, invoking a second headless `Unity.exe -batchmode` process directly will trigger a lock collision.
   - However, `dotnet build "Star Vanguard/Star Vanguard.sln"` directly restores and compiles all `.cs` files into `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll` in ~7.6 seconds, giving instant, reliable compiler gate verification.
   - In addition, the running Editor's `Star Vanguard/Logs/Editor.log` automatically tracks asset pipeline refreshes (`RefreshV2`) and reports compilation errors (`error CS`).
   - Thus, G1 is fully operable via `dotnet build` and log verification.

2. **Gate G2 BuildSettings Conflict:**
   - In `StarVanguardBootstrapSetup.cs` (lines 124-135) and `check_project.py` (lines 372-375), `BootstrapScene.unity` is required at scene index 0 to guarantee persistent managers are created before any menu or gameplay scenes load.
   - `EditorBuildSettings.asset` reflects this: index 0 is `BootstrapScene.unity`, index 1 is `MainMenuScene.unity`, index 2 is `GameplayScene.unity`.
   - But `StarVanguardValidator.cs` (lines 128-133) contains an outdated assertion:
     `Assert(scenes[0].path.EndsWith("MainMenuScene.unity"))`
     `Assert(scenes[1].path.EndsWith("GameplayScene.unity"))`
   - Therefore, any run of `StarVanguardValidator.ValidateAll()` will fail on `CheckBuildSettings()` unless `StarVanguardValidator.cs` is synchronized to check `BootstrapScene` at index 0.

3. **Gate G2 Menu Item Discrepancy:**
   - `AGENTS.md` and `docs/ANTIGRAVITY_MCP_PLAYBOOK.md` refer to running `execute_menu_item StarVanguard.EditorTools.StarVanguardValidator.ValidateAll`.
   - Inspection of `StarVanguardValidator.cs` proves no `[MenuItem]` attribute is declared on `ValidateAll()`.
   - If an agent or test runner calls `execute_menu_item`, it will fail unless a `[MenuItem("Tools/Star Vanguard/Validate All")]` is added, or it is run via CLI `-executeMethod`.

4. **Gate G3 Baseline & Impact of Sprint 0:**
   - Running `check_project.py` yields 0 FAIL and 10 WARN.
   - Executing Sprint 0 tasks will immediately reduce the warning count:
     - T-204a (legacy co-op cleanup): removes `legacy_coop` warning (reduces WARN from 10 to 9).
     - T-101 (assembly definitions): adds 4 `.asmdef` files, removing `asmdef` warning (reduces WARN from 9 to 8).
   - Any commit that introduces a new FAIL or increases WARN above 10 will violate Gate G3.

---

### 3. Caveats

- **Unity MCP Server**: Port 8080 is currently not responding because the CoplayDev UnityMCP python bridge is not active. MCP tools such as `manage_scene` or `read_console` will fail unless the server is started or tasks rely on CLI/editor scripts.
- **Batchmode Concurrency**: Do not attempt to run `Unity.exe -batchmode` simultaneously while Unity Editor GUI (PID 9280) is open, as Unity will abort on `Temp/UnityLockfile`. All batchmode commands (`ValidateAll`, `PlayModeSmoke`) require either quitting the GUI Editor first or executing them inside the editor via C# menu items.
- **NUnit Tests**: Currently no unit test suite exists in `Assets/Tests/`. Regression testing in Sprint 0 is governed primarily by `StarVanguardValidator` and `check_project.py`.

---

### 4. Conclusion

All verification tooling for Sprint 0 has been thoroughly investigated, reverse-engineered, and documented:
1. **Gate G1 (Compile)**: Verified via `dotnet build "Star Vanguard/Star Vanguard.sln"` (0 errors, 18 MSBuild warnings, 7.6s) and `Star Vanguard/Logs/Editor.log`.
2. **Gate G2 (Regression)**: `StarVanguardValidator.ValidateAll` and `StarVanguardPlayModeSmoke.Run` provide comprehensive structural and behavioral checks. Two critical bugs were identified in `StarVanguardValidator.cs`:
   - Outdated `CheckBuildSettings()` asserting `MainMenuScene` at index 0 instead of `BootstrapScene`.
   - Missing `[MenuItem]` attribute required by MCP `execute_menu_item`.
3. **Gate G3 (Static Check)**: `python Tools/check_project.py` is fully operational with a verified baseline of **0 FAIL, 10 WARN, 13 OK**.
4. The team can proceed to Sprint 0 execution with precise awareness of all gate procedures and the identified validator discrepancies.

---

### 5. Verification Method

To independently reproduce and verify these findings:

1. **Verify Gate G1 (Compilation):**
   ```powershell
   dotnet build "Star Vanguard\Star Vanguard.sln"
   ```
   *Expected*: `0 Error(s)`, exit code 0.

2. **Verify Gate G3 (Static Audit & Baseline):**
   ```powershell
   python Tools/check_project.py
   ```
   *Expected*: `Tong ket: 0 FAIL, 10 WARN, 13 OK`, exit code 0.

   To generate JSON and CI summary:
   ```powershell
   python Tools/check_project.py --json audit-report.json
   python Tools/ci_summary.py audit-report.json
   ```

3. **Inspect Gate G2 Discrepancies:**
   - Inspect `Star Vanguard/Assets/Editor/StarVanguardValidator.cs` lines 122-134 vs `Star Vanguard/ProjectSettings/EditorBuildSettings.asset` lines 7-16.
   - Note `BootstrapScene` is index 0 in build settings, but `StarVanguardValidator` checks for `MainMenuScene` at index 0.
