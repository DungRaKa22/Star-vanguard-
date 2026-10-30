# Handoff Report — Worker M1: Milestone 1 Completion (T-204a, T-204b, T-204c)

## 1. Observation

### 1.1 T-204c: Redundant Camera and Light Removal from `BootstrapScene.unity`
- **Tool Created**: `Star Vanguard/Assets/Editor/StarVanguardBootstrapCleanup.cs` (and its `.meta` file generated cleanly by Unity).
  - Target scene: `Assets/Scenes/BootstrapScene.unity`.
  - Targets: `Main Camera` (GameObject ID `519420028`) and `Global Light 2D` (GameObject ID `619394800`).
  - Menu item: `Tools > Star Vanguard > 1c. Remove Duplicate Camera and Light from BootstrapScene and Save`.
  - Batchmode entry: `StarVanguard.EditorTools.StarVanguardBootstrapCleanup.RemoveDuplicatesAndSave`.
- **Tool Updated**: `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs` lines 50–54:
  Added `"Main Camera"` and `"Global Light 2D"` to `k_NonBootstrapObjects` to prevent regression upon re-running bootstrap setup.
- **Batch Execution Result**:
  - Command:
    ```powershell
    Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath `"c:\Documents\UNITY 2026\Star Vanguard`" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardBootstrapCleanup.RemoveDuplicatesAndSave -logFile `"c:\Documents\UNITY 2026\Star Vanguard\bootstrap_cleanup.log`"" -Wait
    ```
  - Verbatim log output (`Star Vanguard/bootstrap_cleanup.log`):
    ```
    [Bootstrap Cleanup] Destroying 'Main Camera' in scene 'BootstrapScene'.
    [Bootstrap Cleanup] Destroying 'Global Light 2D' in scene 'BootstrapScene'.
    [Bootstrap Cleanup] Removed 2 duplicate object(s) from BootstrapScene and saved it.
    Exiting batchmode successfully now!
    ```
  - Diff: `Star Vanguard/Assets/Scenes/BootstrapScene.unity` reduced by 254 lines; `SceneRoots` now contains strictly `{fileID: 760890376}` (`Managers`) and `{fileID: 582285522}` (`BootstrapController`).

### 1.2 T-204a: Legacy Co-op UI Cleanup on `MainMenuScene.unity`
- **Tool Executed**: `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave()` (MenuItem `Tools > Star Vanguard > 1b. Remove Legacy Co-op UI from MainMenuScene and Save`).
- **Batch Execution Result**:
  - Command:
    ```powershell
    Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath `"c:\Documents\UNITY 2026\Star Vanguard`" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave -logFile `"c:\Documents\UNITY 2026\Star Vanguard\coop_cleanup.log`"" -Wait
    ```
  - Verbatim log output (`Star Vanguard/coop_cleanup.log`):
    ```
    [Co-op Cleanup] Destroying 'CoopButton' in scene 'MainMenuScene'.
    [Co-op Cleanup] Destroying 'CoopPanel' in scene 'MainMenuScene'.
    [Co-op Cleanup] Removed 2 legacy co-op object(s) from MainMenuScene and saved it.
    Exiting batchmode successfully now!
    ```
  - Static audit before: `[WARN] legacy_coop` reported `CoopPanel`, `CoopCard`, `CoopButton` (10 WARN total).
  - Static audit after (`python Tools/check_project.py`):
    ```
    [ OK ] legacy_coop: Khong con object Co-op trong scene
    Tong ket: 0 FAIL, 8 WARN, 15 OK
    ```
  - Result: `MainMenuScene.unity` dropped 1,763 lines of dead co-op subtrees (from 12,808 to 11,045 lines).

### 1.3 T-204b: EventSystem Input Actions Wiring
- **Tool Created**: `Star Vanguard/Assets/Editor/StarVanguardInputWiring.cs` (and `.meta` file).
  - Constants: `k_CanonicalActionsAssetPath = "Assets/InputSystem_Actions.inputactions"`, `k_CanonicalActionsAssetGuid = "65b761383e570eb4390d8402a8c60d39"`.
  - Targets: `MainMenuScene.unity` and `GameplayScene.unity`.
  - Menu item: `Tools > Star Vanguard > 2. Wire All EventSystem Input Actions and Save`.
  - Batchmode entry: `StarVanguard.EditorTools.StarVanguardInputWiring.WireAllScenesAndSave`.
- **Batch Execution Result**:
  - Command:
    ```powershell
    Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath `"c:\Documents\UNITY 2026\Star Vanguard`" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardInputWiring.WireAllScenesAndSave -logFile `"c:\Documents\UNITY 2026\Star Vanguard\input_wiring.log`"" -Wait
    ```
  - Verbatim log output (`Star Vanguard/input_wiring.log`):
    ```
    [InputWiring] Loaded 38 InputActionReference(s) for scene 'Assets/Scenes/MainMenuScene.unity'
    [InputWiring] Wired InputSystemUIInputModule on GameObject 'EventSystem' in scene 'Assets/Scenes/MainMenuScene.unity'
    [InputWiring] Saved scene 'Assets/Scenes/MainMenuScene.unity': SUCCESS
    [InputWiring] Loaded 38 InputActionReference(s) for scene 'Assets/Scenes/GameplayScene.unity'
    [InputWiring] Wired InputSystemUIInputModule on GameObject 'EventSystem' in scene 'Assets/Scenes/GameplayScene.unity'
    [InputWiring] Saved scene 'Assets/Scenes/GameplayScene.unity': SUCCESS
    [InputWiring] SUCCESS: Successfully wired EventSystem in MainMenuScene and GameplayScene!
    Exiting batchmode successfully now!
    ```
  - Dangling GUID check:
    - Search for `ca9f5fa95ffab41fb9a615ab714db018` across `Assets/`: **0 matches** (was 22 matches previously).
    - Search for `65b761383e570eb4390d8402a8c60d39` across `Assets/Scenes/`: Exactly **22 references** wired across `MainMenuScene.unity` (lines 3002–3012) and `GameplayScene.unity` (lines 1677–1687): `m_ActionsAsset`, `m_PointAction`, `m_MoveAction`, `m_SubmitAction`, `m_CancelAction`, `m_LeftClickAction`, `m_MiddleClickAction`, `m_RightClickAction`, `m_ScrollWheelAction`, `m_TrackedDevicePositionAction`, `m_TrackedDeviceOrientationAction`.

### 1.4 Smoke Test & Runtime Safety Guard
- **File Updated**: `Star Vanguard/Assets/Scripts/UIManager.cs` lines 331–346:
  - Guarded `HideControlsPanel()` with `if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Controls) UIPanelRouter.Instance.Back();`.
  - Guarded `HideSettingsPanel()` with `if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Settings) UIPanelRouter.Instance.Back();`.
  - Prevents redundant calls (e.g. from `SettingsUI.CloseSettings()`) from popping past `UIPanelId.Home` into `UIPanelId.None`.
- **PlayMode Smoke Test Execution**:
  - Command:
    ```powershell
    Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -ArgumentList "-projectPath `"c:\Documents\UNITY 2026\Star Vanguard`" -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile `"c:\Documents\UNITY 2026\Star Vanguard\smoke_test.log`"" -Wait
    ```
  - Verbatim log output (`Star Vanguard/smoke_test.log` line 1474):
    ```
    [STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0
    ```
  - Result: 100% pass across all 55 assertions (BootstrapScene -> MainMenuScene -> ControlsPanel -> SettingsPanel -> GameplayScene -> WaveDirector -> WeaponHeatController).

---

## 2. Logic Chain

1. **Adherence to AGENTS.md Rule 1 (Zero Direct YAML Edits)**:
   - All scene modifications (`BootstrapScene.unity`, `MainMenuScene.unity`, `GameplayScene.unity`) were performed exclusively through Unity's native Editor API (`Undo.DestroyObjectImmediate`, `EditorUtility.SetDirty`, `SerializedObject.ApplyModifiedProperties`, `EditorSceneManager.SaveScene`) executed via headless Unity batchmode.
   - This guaranteed no YAML corruption, correct fileID de-referencing, and proper Unity asset tracking.

2. **Sequential Execution Under Single Scene Writer Lock**:
   - As established in the explorer handoffs, scenes were locked and modified sequentially:
     - T-204c operated on `BootstrapScene.unity`.
     - T-204a operated on `MainMenuScene.unity`.
     - T-204b operated on `MainMenuScene.unity` and `GameplayScene.unity`.
   - No scene collisions occurred.

3. **Input Wiring Post-Scene Load Resolution**:
   - During initial wiring execution, calling `AssetDatabase.LoadAllAssetsAtPath` before `EditorSceneManager.OpenScene` caused Unity's scene transition memory cleanup to unload the action references.
   - Refactoring `StarVanguardInputWiring.cs` to open the scene first, then load canonical `InputActionAsset` and `InputActionReference` instances, and bind both runtime properties and serialized properties resolved the issue completely.

4. **Complete Elimination of Root Causes**:
   - `ca9f5fa95ffab41fb9a615ab714db018` is completely eradicated from disk (Observation 1.3).
   - In `MainMenuScene` and `GameplayScene`, `InputSystemUIInputModule` delegates hook directly to valid actions from `Assets/InputSystem_Actions.inputactions`.
   - In `BootstrapScene`, duplicate AudioListener and dual-camera rendering conflicts are permanently eliminated (Observation 1.1, smoke test confirms `cameras=1`).

---

## 3. Caveats

1. **PlayMode Smoke Test Execution Mode**:
   - `StarVanguardPlayModeSmoke.Run` manages the player loop through `EditorApplication.Step()` and calls `EditorApplication.Exit()` upon completion. When running it via batchmode CLI, do NOT pass `-quit`, as `-quit` forces Unity to shut down immediately before entering Play Mode.
2. **Git Status of Working Copy**:
   - Changes are staged/verified in working directory. As instructed by AGENTS.md Rule 6, commits should be performed per sprint/milestone guidelines.

---

## 4. Conclusion

1. **Milestone 1 Objectives 100% Achieved**:
   - **T-204a**: Legacy co-op UI elements (`CoopPanel`, `CoopCard`, `CoopButton`) are completely removed from `MainMenuScene.unity`. Static check warning `legacy_coop` is eliminated.
   - **T-204b**: EventSystem in both `MainMenuScene.unity` and `GameplayScene.unity` is properly wired to canonical `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`). Dangling GUID is gone.
   - **T-204c**: Duplicate `Main Camera` and `Global Light 2D` are removed from `BootstrapScene.unity`. `StarVanguardBootstrapSetup.cs` is updated.
2. **All Three Verification Gates Passed**:
   - **Gate G1 (Compile)**: `dotnet build "Star Vanguard\Star Vanguard.sln"` passes with 0 errors.
   - **Gate G2 (Regression / Functional)**: `StarVanguardPlayModeSmoke.Run` passes 55/55 checks (0 failures).
   - **Gate G3 (Static Project Audit)**: `python Tools/check_project.py` reports 0 FAIL, 8 WARN, 15 OK.

---

## 5. Verification Method

To independently reproduce and verify this milestone:

1. **Verify Dangling GUID Elimination**:
   ```powershell
   python -c "import os; text=open('Star Vanguard/Assets/Scenes/MainMenuScene.unity', 'r', encoding='utf-8').read(); print('ca9f found in MainMenu:', 'ca9f5fa95ffab41fb9a615ab714db018' in text)"
   python -c "import os; text=open('Star Vanguard/Assets/Scenes/GameplayScene.unity', 'r', encoding='utf-8').read(); print('ca9f found in Gameplay:', 'ca9f5fa95ffab41fb9a615ab714db018' in text)"
   ```
   Expected: Both output `False`.

2. **Verify Static Audit Gate (Gate G3)**:
   ```powershell
   python Tools/check_project.py
   ```
   Expected: `0 FAIL, 8 WARN, 15 OK`. Verify `[ OK ] legacy_coop` is reported.

3. **Verify Solution Compilation (Gate G1)**:
   ```powershell
   dotnet build "Star Vanguard/Star Vanguard.sln"
   ```
   Expected: `0 Error(s)`.

4. **Verify Play Mode Regression / Functional Test (Gate G2)**:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile -
   ```
   Expected: Output logs `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0` and terminates with exit code 0.
