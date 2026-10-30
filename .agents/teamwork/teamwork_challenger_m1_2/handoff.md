# Handoff Report — Challenger M1-2: Scene Interaction & Smoke Rigor Verification

## 1. Observation

### 1.1 PlayMode Smoke Test (`StarVanguardPlayModeSmoke`) Execution
- **Command Executed**:
  ```powershell
  $proc = Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath `"c:\Documents\UNITY 2026\Star Vanguard`" -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile `"c:\Documents\UNITY 2026\Star Vanguard\smoke_test_challenger.log`"" -Wait -PassThru
  ```
- **Exit Code**: `0`
- **Verbatim Log Output** (`Star Vanguard/smoke_test_challenger.log` line 1417):
  ```
  [STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0
  ```
- **Verified Sub-checks (Selected Verbatim Assertions)**:
  - Line 81: `[STARVANGUARD-SMOKE] PASS transitioned from BootstrapScene to MainMenuScene`
  - Line 183: `[STARVANGUARD-SMOKE] PASS MainMenuScene contains NO local copies of persistent managers`
  - Line 185: `[STARVANGUARD-SMOKE] PASS menu has exactly one Canvas`
  - Line 187: `[STARVANGUARD-SMOKE] PASS menu has exactly one EventSystem`
  - Line 205: `[STARVANGUARD-SMOKE] PASS LAUNCH MISSION button is the top-most raycast target`
  - Line 238: `[STARVANGUARD-SMOKE] PASS BACK button is not covered by another graphic`
  - Line 240: `[STARVANGUARD-SMOKE] PASS BACK returns to the main menu`
  - Line 264: `[STARVANGUARD-SMOKE] PASS settings BACK is not covered by another graphic`
  - Line 287: `[STARVANGUARD-SMOKE] PASS settings BACK returns to the main menu`
  - Line 425: `[STARVANGUARD-SMOKE] PASS GameState is Playing after loading GameplayScene (actual=Playing)`
  - Line 468: `[STARVANGUARD-SMOKE] PASS GameManager lives in DontDestroyOnLoad (from BootstrapScene)`
  - Line 474: `[STARVANGUARD-SMOKE] PASS PoolManager lives in DontDestroyOnLoad (from BootstrapScene)`
  - Line 492: `[STARVANGUARD-SMOKE] PASS GameplayScene contains NO local copies of persistent managers`
  - Line 530: `[STARVANGUARD-SMOKE] PASS wave 1 spawned enemies (active=40, includingInactive=50)`
  - Line 545: `[STARVANGUARD-SMOKE] PASS weapon overheats after 40 sustained shots`
  - Line 550: `[STARVANGUARD-SMOKE] PASS ResetHeat unlocks the weapon again`

### 1.2 `BootstrapScene.unity` Camera, Light, and Persistent Managers Audit
- **File Checked**: `Star Vanguard/Assets/Scenes/BootstrapScene.unity` (691 lines, 28 YAML documents).
- **Camera and Light Absence**:
  - Search for `Main Camera`: **0 matches** (GameObject ID `519420028` is absent).
  - Search for `Global Light 2D`: **0 matches** (GameObject ID `619394800` is absent).
  - Search for tag `20` (`Camera` component): **0 matches**.
  - Search for URP 2D Light `Light2D` components: **0 matches**.
- **Scene Hierarchy & Roots** (`BootstrapScene.unity` lines 685–691):
  ```yaml
  --- !u!1660057539 &9223372036854775807
  SceneRoots:
    m_ObjectHideFlags: 0
    m_Roots:
    - {fileID: 760890376}
    - {fileID: 582285522}
  ```
  - Root 1 (`{fileID: 760890376}`): Transform of GameObject `Managers` (`{fileID: 760890375}`), containing 5 persistent managers as children:
    1. `GameManager` (`{fileID: 2080025536}`) with `GameManager` script (`{fileID: 2080025537}`) & `ItemDropManager` (`{fileID: 2080025539}`)
    2. `AudioManager` (`{fileID: 1941854439}`) with `AudioManager` script (`{fileID: 1941854440}`), SFX AudioSource (`{fileID: 1941854441}`), Music AudioSource (`{fileID: 1941854443}`)
    3. `PoolManager` (`{fileID: 1518732596}`) with `PoolManager` script (`{fileID: 1518732599}`)
    4. `LeaderboardManager` (`{fileID: 1900088710}`) with `LeaderboardManager` script (`{fileID: 1900088711}`)
    5. `SettingsManager` (`{fileID: 638974996}`) with `SettingsManager` script (`{fileID: 638974997}`)
  - Root 2 (`{fileID: 582285522}`): Transform of GameObject `BootstrapController` (`{fileID: 582285520}`) with `BootstrapLoader` script (`{fileID: 582285521}`).
- **Setup Tool Hardening**:
  - `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs` lines 53–54: `"Main Camera"` and `"Global Light 2D"` are hard-coded in `k_NonBootstrapObjects` to prevent regression.

### 1.3 `MainMenuScene.unity` Button Navigation, Raycast Targets, and EventSystem Input
- **EventSystem Input Module Wiring**:
  - `MainMenuScene.unity` GameObject `EventSystem` (`&501726004`) has `InputSystemUIInputModule` (`&501726005`, guid `01614664b831546d2ae94a42149d80ac`):
    - `m_ActionsAsset: {fileID: -944628639613478452, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_PointAction: {fileID: -1654692200621890270, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_LeftClickAction: {fileID: 3001919216989983466, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_RightClickAction: {fileID: -4090225696740746782, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_MiddleClickAction: {fileID: -2185481485913320682, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_ScrollWheelAction: {fileID: 6240969308177333660, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_MoveAction: {fileID: -8784545083839296357, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_SubmitAction: {fileID: 392368643174621059, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_CancelAction: {fileID: 7727032971491509709, guid: 65b761383e570eb4390d8402a8c60d39, type: 3}`
    - `m_PointerBehavior: 0`
- **Canvas & Raycaster**:
  - GameObject `Canvas` (`&134044666`) carries `GraphicRaycaster` (`&134044667`, guid `dc42784cf147c0c48a680349fa168899`).
- **Button Selectables & Navigation**:
  - 10 UI Button components verified in `MainMenuScene.unity`:
    `ExitButton`, `ResetToDefaultButton`, `Difficulty_VETERAN`, `Difficulty_CADET`, `SettingsButton`, `Difficulty_ACE`, `BackButton` (x2), `LaunchButton`, `ControlsButton`.
  - All 10 buttons configure `m_Navigation.m_Mode = 3` (Automatic), with zero explicit dead pointers to removed objects (`Up=0, Down=0, Left=0, Right=0`).
  - All 10 buttons configure valid `m_TargetGraphic` pointing to existing Image components in the scene.
  - Legacy co-op elements (`CoopPanel`, `CoopCard`, `CoopButton`) are completely eliminated from `MainMenuScene.unity` (0 occurrences).
  - Runtime listener bindings in `MainMenuScreen.BindHome()`, `ControlsScreen.Bind()`, and `SettingsScreen.Bind()` cleanly bind clicks to `OnLaunchMission`, `OpenPanel(UIPanelId.Controls)`, `OpenPanel(UIPanelId.Settings)`, `OnExitGame`, and `UIPanelRouter.Instance.Back()`.

### 1.4 YAML Integrity and Orphaned fileID Oracle
- **Tool Executed**: `.agents/teamwork/teamwork_challenger_m1_2/scene_integrity_oracle.py`
- **Output Results**:
  - `BootstrapScene.unity`: 28 defined objects, 0 orphaned local fileIDs, 0 dangling GUIDs (`ca9f...`).
  - `MainMenuScene.unity`: 385 defined objects, 0 orphaned local fileIDs, 0 dangling GUIDs (`ca9f...`), 11 canonical GUIDs (`65b7...`).
  - `GameplayScene.unity`: 264 defined objects, 0 orphaned local fileIDs, 0 dangling GUIDs (`ca9f...`), 11 canonical GUIDs (`65b7...`).
- Total orphaned local fileIDs across all 3 modified scenes: **0**.

### 1.5 Static Project Audit & C# Solution Build
- **Command Executed**: `python Tools/check_project.py`
  - Output: `0 FAIL, 8 WARN, 15 OK`
  - Specific check: `[ OK ] legacy_coop: Khong con object Co-op trong scene`
- **Command Executed**: `dotnet build "Star Vanguard\Star Vanguard.sln"`
  - Output: `0 Error(s)`, 18 Warning(s).

---

## 2. Logic Chain

1. **Gate G2 (PlayMode Smoke Regression) Rigor**:
   - The test was executed independently by Challenger M1-2 via headless batchmode Unity.
   - The smoke test does not merely check component presence; it verifies actual pointer raycast reachability (`IsTopMostClickable`), drives button onClick events, invokes router panel switches, loads gameplay additively, and asserts wave spawning and weapon overheat mechanics.
   - The verified 55/55 checks and exit code 0 prove runtime functional correctness from `BootstrapScene` through `MainMenuScene` to `GameplayScene` (Observation 1.1).

2. **No Duplicate Camera/Lighting Conflict**:
   - Because `BootstrapScene.unity` contains strictly `Managers` and `BootstrapController` with no Camera or Light2D components (Observation 1.2), additive scene loading and boot sequencing from `BootstrapScene` cannot cause double-rendering or dual-listener conflicts with `MainMenuScene` or `GameplayScene`.

3. **Input Responsiveness & EventSystem Integrity**:
   - Unresponsive mouse input previously occurred due to a broken/dangling GUID (`ca9f...`) on `InputSystemUIInputModule`.
   - Inspection proves canonical GUID `65b761383e570eb4390d8402a8c60d39` is properly assigned across all mouse/pointer action references in both `MainMenuScene` and `GameplayScene` (Observation 1.3).
   - GraphicRaycaster is verified on the Canvas, and all button navigation modes are automatic with valid target graphics and active raycasts (Observation 1.3).

4. **Scene File & Single Scene Writer Safety**:
   - All scene modifications were completed using Unity's native Editor serialization APIs (`StarVanguardCoopCleanup`, `StarVanguardBootstrapCleanup`, `StarVanguardInputWiring`).
   - The oracle script cross-referenced every `{fileID: X}` in all 3 scenes against the set of defined objects, proving zero orphaned local fileIDs and zero YAML syntax corruption (Observation 1.4).

---

## 3. Caveats

1. **Legacy `StarVanguardValidator.cs` Incompatibility**:
   - Batchmode execution of `StarVanguardValidator.ValidateAll` reports `passed=29 failed=5`.
   - **Root Cause**: `StarVanguardValidator.cs` contains legacy assertions written before the Bootstrap architecture was adopted. Specifically, lines 128–133 assert that `MainMenuScene.unity` is at build index 0 (which violates `AGENTS.md` requiring `BootstrapScene.unity` at index 0), and lines 141–158 assert that `GameManager`, `AudioManager`, and `PoolManager` exist locally inside `GameplayScene.unity` (which violates the singleton architecture where persistent managers are centralized in `BootstrapScene` and kept in `DontDestroyOnLoad`).
   - `StarVanguardPlayModeSmoke.cs` is the authoritative regression test designed for the current architecture.
   - Recommended follow-up: Update `StarVanguardValidator.cs` in a future task to reflect the bootstrap scene hierarchy.
2. **Review Scope**:
   - Gamepad navigation via virtual cursor was not tested on physical hardware, as Sprint 0 focuses on mouse/keyboard uGUI input wiring and emergency stabilization.

---

## 4. Conclusion

- **Verdict**: **`APPROVE`**
- All 4 dispatch requirements and acceptance criteria for Milestone 1 are empirically satisfied:
  1. PlayMode smoke test (`StarVanguardPlayModeSmoke`) independently verified with 55/55 passed assertions and exit code 0.
  2. `BootstrapScene.unity` has zero duplicate cameras or 2D lights, with persistent managers intact.
  3. `MainMenuScene.unity` buttons have valid automatic navigation, valid raycast targets, and canonical mouse input wiring via `InputSystemUIInputModule`.
  4. All 3 modified scenes have 0 broken YAML blocks, 0 orphaned fileIDs, and 0 dangling GUIDs.

---

## 5. Verification Method

To independently reproduce this verification:

1. **Run PlayMode Smoke Test**:
   ```powershell
   $proc = Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath `"c:\Documents\UNITY 2026\Star Vanguard`" -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile `"c:\Documents\UNITY 2026\Star Vanguard\smoke_test_verify.log`"" -Wait -PassThru
   Write-Output "Exit Code: $($proc.ExitCode)"
   Select-String -Path "Star Vanguard\smoke_test_verify.log" -Pattern "SUMMARY passed="
   ```
   *Expected*: Exit Code 0, `SUMMARY passed=55 failed=0 exitCode=0`.

2. **Run Scene Integrity Oracle**:
   ```powershell
   python ".agents\teamwork\teamwork_challenger_m1_2\scene_integrity_oracle.py"
   ```
   *Expected*: `Orphaned local fileIDs found: 0` and `References to dangling GUID (ca9f...): 0` for all 3 scenes.

3. **Run Static Project Audit**:
   ```powershell
   python Tools/check_project.py
   ```
   *Expected*: `0 FAIL, 8 WARN, 15 OK` with `[ OK ] legacy_coop`.
