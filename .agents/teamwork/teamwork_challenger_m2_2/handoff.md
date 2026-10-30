# Handoff Report — Challenger M2-2: Regression & Smoke Rigor Challenge

**Verdict**: `APPROVE`

---

## 1. Observation

### 1.1 Project Validator Execution (`StarVanguardValidator.ValidateAll`)
- **Execution Command**:
  ```powershell
  Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-quit", "-executeMethod", "StarVanguard.EditorTools.StarVanguardValidator.ValidateAll", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\validator_challenger.log`"" -Wait -PassThru
  ```
- **Exit Code**: `0`
- **Verbatim Output Log (`Star Vanguard\Temp\validator_challenger.log`)**:
  ```text
  [STARVANGUARD-CHECK] ===== BEGIN VALIDATION =====
  [STARVANGUARD-CHECK] PASS Player_Prefab loads
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerController
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerShooting
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerStats
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerHitBox
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerRespawn
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerBombing
  [STARVANGUARD-CHECK] PASS Player_Prefab is tagged Player
  [STARVANGUARD-CHECK] PASS Player_Prefab has WeaponHeatController (overheat system wired)
  [STARVANGUARD-CHECK] PASS heat per shot = 0.045 (actual=0,045)
  [STARVANGUARD-CHECK] PASS heat per level = 0.005 (actual=0,005)
  [STARVANGUARD-CHECK] PASS cooling rate = 0.35/s (actual=0,35)
  [STARVANGUARD-CHECK] PASS cool down delay = 0.25s (actual=0,25)
  [STARVANGUARD-CHECK] PASS Player_Prefab has PlayerShooting (for heat reference)
  [STARVANGUARD-CHECK] PASS PlayerShooting.m_HeatController is assigned
  [STARVANGUARD-CHECK] PASS PlayerShooting.m_HeatController points at the same component
  [STARVANGUARD-CHECK] PASS Boss_Prefab loads
  [STARVANGUARD-CHECK] PASS Boss_Prefab has BossController
  [STARVANGUARD-CHECK] PASS Boss_Prefab has BossShooting
  [STARVANGUARD-CHECK] PASS Boss_Prefab has BossMover
  [STARVANGUARD-CHECK] PASS prefab scan found prefabs to check (n=18)
  [STARVANGUARD-CHECK] PASS no missing/broken MonoScript in prefabs (broken=0)
  [STARVANGUARD-CHECK] PASS build settings contain >= 3 scenes (n=3)
  [STARVANGUARD-CHECK] PASS build index 0 is BootstrapScene (Assets/Scenes/BootstrapScene.unity)
  [STARVANGUARD-CHECK] PASS build index 1 is MainMenuScene (Assets/Scenes/MainMenuScene.unity)
  [STARVANGUARD-CHECK] PASS build index 2 is GameplayScene (Assets/Scenes/GameplayScene.unity)
  [STARVANGUARD-CHECK] PASS BootstrapScene opens
  [STARVANGUARD-CHECK] PASS GameplayScene opens additively
  [STARVANGUARD-CHECK] PASS GameplayScene has GameManager
  [STARVANGUARD-CHECK] PASS GameplayScene has UIManager
  [STARVANGUARD-CHECK] PASS GameplayScene has AudioManager
  [STARVANGUARD-CHECK] PASS GameplayScene has WaveDirector
  [STARVANGUARD-CHECK] PASS WaveDirector.m_EnemyPrefab assigned
  [STARVANGUARD-CHECK] PASS WaveDirector.m_BossPrefab assigned
  [STARVANGUARD-CHECK] PASS WaveDirector.m_Player assigned
  [STARVANGUARD-CHECK] PASS GameplayScene has PoolManager
  [STARVANGUARD-CHECK] PASS enemy pool >= 40 so Wave_Line (8x5) is never skipped (m_EnemyPoolSize=50)
  [STARVANGUARD-CHECK] SUMMARY passed=37 failed=0
  ```
- **Result**: Exactly **37/37 passed, 0 failed**.

---

### 1.2 PlayMode Smoke Test (`StarVanguardPlayModeSmoke.Run`)
- **Execution Command**:
  ```powershell
  Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-executeMethod", "StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\playmode_smoke_challenger.log`"" -Wait -PassThru
  ```
- **Exit Code**: `0`
- **Verbatim Output Log (`Star Vanguard\Temp\playmode_smoke_challenger.log`)**:
  ```text
  [STARVANGUARD-SMOKE] ===== BEGIN PLAY MODE SMOKE =====
  [STARVANGUARD-SMOKE] opened Assets/Scenes/BootstrapScene.unity
  [STARVANGUARD-SMOKE] PASS transitioned from BootstrapScene to MainMenuScene
  [STARVANGUARD-SMOKE] diag menu timeScale=1 time=1,02 panels(settings)=False hud=False waveDirector=False
  [STARVANGUARD-SMOKE] PASS MainMenuScreen runs in MainMenuScene
  [STARVANGUARD-SMOKE] PASS menu hierarchy (MenuRoot) was built
  [STARVANGUARD-SMOKE] PASS menu is visible on boot
  [STARVANGUARD-SMOKE] diag canvases=1
  [STARVANGUARD-SMOKE] diag canvas 'Canvas' root=Canvas mode=ScreenSpaceOverlay order=0 scale=0,385 size=1663x1247 scaler=ScaleWithScreenSize ref=(1920.00, 1080.00) match=0,50
  [STARVANGUARD-SMOKE] diag cameras=1 screen=640x480
  [STARVANGUARD-SMOKE] diag camera 'Main Camera' depth=-1,0 clear=SolidColor ortho=5,00 rect=(0,00,0,00,1,00,1,00) active=True
  [STARVANGUARD-SMOKE] diag MenuRoot#1 path='Canvas/MenuRoot' active=True size=1663x1247 lossyScale=(0.38, 0.38, 0.38) corners=(0,0)-(640,480) alpha=1,00
  [STARVANGUARD-SMOKE] PASS the menu hierarchy exists exactly once (actual=1)
  [STARVANGUARD-SMOKE] PASS canvas exists for the leftover check
  [STARVANGUARD-SMOKE] PASS no leftover UI is drawn on the menu canvas (found: )
  [STARVANGUARD-SMOKE] PASS HUD is not in the menu scene
  [STARVANGUARD-SMOKE] PASS WaveDirector is not in the menu scene
  [STARVANGUARD-SMOKE] PASS player ship is not in the menu scene
  [STARVANGUARD-SMOKE] PASS SettingsPanel starts hidden
  [STARVANGUARD-SMOKE] PASS LeaderboardPanel starts hidden
  [STARVANGUARD-SMOKE] PASS MainMenuScene contains NO local copies of persistent managers
  [STARVANGUARD-SMOKE] PASS menu has exactly one Canvas
  [STARVANGUARD-SMOKE] PASS menu has exactly one EventSystem
  [STARVANGUARD-SMOKE] PASS exactly one menu panel is visible (actual=1)
  [STARVANGUARD-SMOKE] PASS router reports Home as the current panel
  [STARVANGUARD-SMOKE] PASS LAUNCH MISSION button is the top-most raycast target
  [STARVANGUARD-SMOKE] PASS difficulty selection (ACE) reaches GameManager
  [STARVANGUARD-SMOKE] PASS CONTROLS panel opened through the router
  [STARVANGUARD-SMOKE] PASS controls card exists
  [STARVANGUARD-SMOKE] diag controls card=980x720
  [STARVANGUARD-SMOKE] PASS controls card fits a 1080p screen (actual=980x720)
  [STARVANGUARD-SMOKE] PASS BACK button exists
  [STARVANGUARD-SMOKE] PASS BACK button is not covered by another graphic
  [STARVANGUARD-SMOKE] PASS BACK returns to the main menu
  [STARVANGUARD-SMOKE] PASS SETTINGS panel opened through the router
  [STARVANGUARD-SMOKE] PASS SettingsPanel found
  [STARVANGUARD-SMOKE] PASS SettingsPanel fits the screen (height=847)
  [STARVANGUARD-SMOKE] PASS settings BACK button exists
  [STARVANGUARD-SMOKE] PASS settings BACK is not covered by another graphic
  [STARVANGUARD-SMOKE] diag settings back listeners=0  routerNull=False before=Settings depth=2
  [STARVANGUARD-SMOKE] diag settings back after invoke: current=Home depth=1
  [STARVANGUARD-SMOKE] PASS settings BACK returns to the main menu
  [STARVANGUARD-SMOKE] loading GameplayScene (same call as the menu button)
  [STARVANGUARD-SMOKE] PASS GameManager.Instance survives the scene load
  [STARVANGUARD-SMOKE] PASS GameState is Playing after loading GameplayScene (actual=Playing)
  [STARVANGUARD-SMOKE] PASS player ship spawned in GameplayScene
  [STARVANGUARD-SMOKE] PASS WaveDirector present in GameplayScene
  [STARVANGUARD-SMOKE] PASS ParallaxManager exists after loading GameplayScene
  [STARVANGUARD-SMOKE] diag parallax scene='GameplayScene' count=1
  [STARVANGUARD-SMOKE] PASS ParallaxManager belongs to the gameplay scene (actual='GameplayScene')
  [STARVANGUARD-SMOKE] PASS exactly one ParallaxManager survives the menu to gameplay transition (actual=1)
  [STARVANGUARD-SMOKE] PASS UIManager instance exists after loading GameplayScene
  [STARVANGUARD-SMOKE] PASS UIManager belongs to the gameplay scene (actual='GameplayScene')
  [STARVANGUARD-SMOKE] PASS WaveDirector instance exists after loading GameplayScene
  [STARVANGUARD-SMOKE] PASS WaveDirector belongs to the gameplay scene (actual='GameplayScene')
  [STARVANGUARD-SMOKE] PASS GameManager instance exists
  [STARVANGUARD-SMOKE] PASS GameManager lives in DontDestroyOnLoad (from BootstrapScene)
  [STARVANGUARD-SMOKE] PASS PoolManager instance exists
  [STARVANGUARD-SMOKE] PASS PoolManager lives in DontDestroyOnLoad (from BootstrapScene)
  [STARVANGUARD-SMOKE] PASS GameplayScene contains NO local copies of persistent managers
  [STARVANGUARD-SMOKE] diag timeScale=1 time=8,90 delta=0,0200 cameraMain=Main Camera cameras=1
  [STARVANGUARD-SMOKE] diag director.camera=ok director.player=ok pool.available=10
  [STARVANGUARD-SMOKE] PASS WaveDirector.Instance available
  [STARVANGUARD-SMOKE] PASS wave coroutine is running (mission started)
  [STARVANGUARD-SMOKE] PASS wave 1 spawned enemies (active=40, includingInactive=50)
  [STARVANGUARD-SMOKE] PASS WeaponHeatController exists on the live player ship
  [STARVANGUARD-SMOKE] PASS weapon overheats after 40 sustained shots
  [STARVANGUARD-SMOKE] PASS heat gauge reached maximum (actual=1,000)
  [STARVANGUARD-SMOKE] PASS weapon refuses to fire while overheated (jam works)
  [STARVANGUARD-SMOKE] PASS ResetHeat unlocks the weapon again
  [STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0
  ```
- **Result**: Exactly **55/55 passed, 0 failed, exitCode=0**.

---

### 1.3 NUnit EditMode Test Suite (`Star Vanguard/Assets/Tests/AssemblySmokeTests.cs`)
- **Execution Command**:
  ```powershell
  Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-runTests", "-testPlatform", "EditMode", "-testResults", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\challenger_nunit_results.xml`"", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\challenger_nunit.log`"" -Wait -PassThru
  ```
- **Exit Code**: `0`
- **Verbatim XML Results (`Star Vanguard\Temp\challenger_nunit_results.xml`)**:
  ```xml
  <test-run id="2" testcasecount="5" result="Passed" total="5" passed="5" failed="0" inconclusive="0" skipped="0" asserts="0">
    <test-suite type="TestFixture" id="1001" name="AssemblySmokeTests" fullname="StarVanguard.Tests.AssemblySmokeTests" result="Passed">
      <test-case id="1004" name="ArchitecturalConstraint_RuntimeDoesNotReferenceUI" result="Passed" />
      <test-case id="1005" name="ArchitecturalConstraint_UIDependsOnRuntime" result="Passed" />
      <test-case id="1006" name="EnumsAndConstants_AreConsistent" result="Passed" />
      <test-case id="1002" name="RuntimeAssembly_LoadsExpectedCoreTypes" result="Passed" />
      <test-case id="1003" name="UIAssembly_LoadsExpectedUITypes" result="Passed" />
    </test-suite>
  </test-run>
  ```
- **Result**: Exactly **5/5 passed, 0 failed**.

---

### 1.4 Regression Invariance (Milestone 1 Protections)
1. **Bootstrap Duplicate Camera & Light (T-204c)**:
   - File inspected: `Star Vanguard/Assets/Scenes/BootstrapScene.unity`.
   - Grep search for `Camera` component: 0 occurrences.
   - Grep search for `Light` component: 0 occurrences (only global lighting settings).
   - Only persistent objects present: `BootstrapController`, `SettingsManager`, `Managers`, `PoolManager`, `LeaderboardManager`, `AudioManager`, `GameManager`.
2. **Legacy Co-op UI Cleanup (T-204a)**:
   - File inspected: `Star Vanguard/Assets/Scenes/MainMenuScene.unity`.
   - Grep search for `CoopPanel`, `CoopCard`, `CoopButton`: 0 occurrences.
   - `python Tools/check_project.py` reports: `[ OK ] legacy_coop: Khong con object Co-op trong scene`.
3. **EventSystem Mouse Input (T-204b)**:
   - `Star Vanguard/Assets/Scenes/MainMenuScene.unity` (line 3002):
     `m_ActionsAsset: {fileID: -944628639613478452, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
   - `Star Vanguard/Assets/Scenes/GameplayScene.unity` (line 1677):
     `m_ActionsAsset: {fileID: -944628639613478452, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
   - Actions mapped: Point, Move, Submit, Cancel, LeftClick, MiddleClick, RightClick, ScrollWheel, TrackedDevice.
   - Raycast hit verification: `StarVanguardPlayModeSmoke.CheckMenuScene()` proves `LaunchButton` is the top-most clickable element.

---

### 1.5 Decoupling & Boundary Verification (T-101)
- Scanned all 49 `.cs` files in `Star Vanguard/Assets/Scripts/`: 0 files contain references to `StarVanguard.UI`, `UIPanelRouter`, or `UIPanelId`.
- No `System.Reflection`, `Type.GetType`, or `SendMessage` workarounds found in `Assets/Scripts/`.
- `StarVanguard.UI.asmdef` specifies `"autoReferenced": false`, preventing any implicit dependency from unrelated assemblies.

---

## 2. Logic Chain

1. **Gate G1 (Compile & Build)**: Observation 1.1, 1.2, 1.3 show Unity compiles all 4 assemblies cleanly (`StarVanguard.Runtime`, `StarVanguard.UI`, `StarVanguard.Editor`, `StarVanguard.Tests`) without cyclic dependencies or missing references.
2. **Gate G2 (Regression & Smoke Rigor)**: Observation 1.1 confirms `StarVanguardValidator.ValidateAll` achieves 37/37 pass. Observation 1.2 confirms `StarVanguardPlayModeSmoke.Run` executes the complete boot, menu navigation, gameplay transition, pool initialization, wave spawning, and overheat mechanics, passing 55/55 checks with exit code 0. Observation 1.3 confirms NUnit EditMode executes 5/5 tests passing with 0 failures.
3. **Gate G3 (Static Project Audit)**: `python Tools/check_project.py` produces `0 FAIL, 6 WARN, 17 OK`, verifying clean asset metadata, proper build scenes order, 4 asmdefs, and no orphan meta files.
4. **Regression Invariance**: Observation 1.4 confirms that Milestone 1 fixes (no duplicate camera/light in Bootstrap, no co-op UI in MainMenu, EventSystem actions asset wired to GUID `65b761383e570eb4390d8402a8c60d39`) remain 100% intact.
5. **Architectural Decoupling**: Observation 1.5 confirms `StarVanguard.Runtime` contains zero direct or reflection-based references to `StarVanguard.UI`. Navigation flows upward exclusively through static C# events (`UIManager.OnShowSettingsRequested`, etc.) and downward through direct engine/gameplay methods.

---

## 3. Caveats

- **Pre-existing Known Technical Debt**:
  `check_project.py` reports 6 non-blocking warnings (`template_scene`, `hud_duplication`, `hot_path`, `perf_settings`, `scene_size`, `docs_cleanup`). These are tracked backlog items in `AGENTS.md` §5 (T-201, T-304, T-305, T-401) and were neither introduced nor worsened by Milestone 2.
- **Interactive UI Smoke Test Scope**:
  PlayMode tests run headlessly in batchmode with `EditorApplication.Step()` and simulated `GraphicRaycaster` screen-point hits. Full human tactile gameplay (controller/mouse feel) will be verified in Milestone 3 functional acceptance.

---

## 4. Conclusion

Milestone 2 has been thoroughly stress-tested and empirically verified.
- 37/37 checks pass in `StarVanguardValidator.ValidateAll`.
- 55/55 checks pass in `StarVanguardPlayModeSmoke.Run` with exit code 0.
- 5/5 NUnit EditMode tests pass.
- All Milestone 1 scene and input fixes remain 100% intact.
- Architectural decoupling is strictly enforced by compiler and runtime asmdef boundaries.

**Final Verdict**: `APPROVE`

---

## 5. Verification Method

To independently re-verify all empirical findings:

1. **Run Static Project Audit (Gate G3)**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expectation*: `Tong ket: 0 FAIL, 6 WARN, 17 OK`.

2. **Run Headless Wiring Validator (Gate G2)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-quit", "-executeMethod", "StarVanguard.EditorTools.StarVanguardValidator.ValidateAll", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\validator_run.log`"" -Wait
   Get-Content "Star Vanguard\Temp\validator_run.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expectation*: `[STARVANGUARD-CHECK] SUMMARY passed=37 failed=0`.

3. **Run Headless PlayMode Smoke Test (Gate G2)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-executeMethod", "StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\smoke_run.log`"" -Wait
   Get-Content "Star Vanguard\Temp\smoke_run.log" | Select-String -Pattern "SUMMARY"
   ```
   *Expectation*: `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`.

4. **Run NUnit EditMode Tests (Gate G2)**:
   ```powershell
   Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath", "`"C:\Documents\UNITY 2026\Star Vanguard`"", "-batchmode", "-runTests", "-testPlatform", "EditMode", "-testResults", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\nunit_results.xml`"", "-logFile", "`"C:\Documents\UNITY 2026\Star Vanguard\Temp\nunit_run.log`"" -Wait
   Get-Content "Star Vanguard\Temp\nunit_results.xml" | Select-String -Pattern "testcasecount"
   ```
   *Expectation*: `testcasecount="5" result="Passed" total="5" passed="5" failed="0"`.
