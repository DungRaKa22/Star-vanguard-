# Survey Report: Scene Hierarchy & Cleanup Survey (Sprint 0)

## 1. Observation

### 1.1 Legacy Co-op UI in `MainMenuScene.unity` and `StarVanguardCoopCleanup.cs`
- **File**: `Star Vanguard/Assets/Editor/StarVanguardCoopCleanup.cs`
  - Line 24: `private static readonly string[] k_LegacyCoopNames = { "CoopPanel", "CoopCard", "CoopButton" };`
  - Lines 56-87: Method `RemoveFromMainMenuSceneAndSave()` bound to MenuItem `"Tools/Star Vanguard/1b. Remove Legacy Co-op UI from MainMenuScene and Save"`.
  - Lines 89-118: Method `removeFromScene(Scene i_Scene)` iterates all root GameObjects and all child transforms via `GetComponentsInChildren<Transform>(true)`. If `candidate.name` matches `k_LegacyCoopNames` and `!hasLegacyAncestor(candidate)`, it performs `Undo.DestroyObjectImmediate(candidate.gameObject)` and marks the scene dirty.
- **File**: `Star Vanguard/Assets/Scenes/MainMenuScene.unity`
  - Line 8003: `m_Name: CoopButton` on GameObject ID `1283642853` (parent RectTransform ID `648767580` inside `MenuRoot/ButtonGroup`, child ID `1433009940` Text TMP).
  - Line 10527: `m_Name: CoopPanel` on GameObject ID `1723830002` (parent RectTransform ID `134044670` Canvas, child ID `1951282684` CoopCard).
  - Line 11702: `m_Name: CoopCard` on GameObject ID `1951282683` (parent RectTransform ID `1723830003` CoopPanel, children IDs `1933019121`, `1800015116`, `1037428439`, `1521203888`).
- **Tool Output (`python Tools/check_project.py`)**:
  ```
  [WARN] legacy_coop: Scene con object Co-op (crash khong, nhung UI chet). Chay menu 'Tools/Star Vanguard/1b. Remove Legacy Co-op UI from MainMenuScene and Save'
         - Assets/Scenes/MainMenuScene.unity -> CoopPanel
         - Assets/Scenes/MainMenuScene.unity -> CoopCard
         - Assets/Scenes/MainMenuScene.unity -> CoopButton
  ```

### 1.2 EventSystem and `InputSystemUIInputModule` in `MainMenuScene.unity` and `GameplayScene.unity`
- **File**: `Star Vanguard/Assets/Scenes/MainMenuScene.unity`
  - Line 3255: `GameObject &501726004` named `EventSystem`.
  - Line 3261: Component `MonoBehaviour &501726005` with script GUID `01614664b831546d2ae94a42149d80ac` (`InputSystemUIInputModule`).
  - Line 3277: `m_ActionsAsset: {fileID: -944628639613478452, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}`
  - Lines 3278–3287: All 10 action reference fields (`m_PointAction`, `m_MoveAction`, `m_SubmitAction`, `m_CancelAction`, `m_LeftClickAction`, `m_MiddleClickAction`, `m_RightClickAction`, `m_ScrollWheelAction`, `m_TrackedDevicePositionAction`, `m_TrackedDeviceOrientationAction`) reference GUID `ca9f5fa95ffab41fb9a615ab714db018`.
- **File**: `Star Vanguard/Assets/Scenes/GameplayScene.unity`
  - Line 1655: `GameObject &501726004` named `EventSystem`.
  - Line 1661: Component `MonoBehaviour &501726005` with script GUID `01614664b831546d2ae94a42149d80ac` (`InputSystemUIInputModule`).
  - Line 1677: `m_ActionsAsset: {fileID: -944628639613478452, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}`
  - Lines 1678–1687: All 10 action reference fields reference GUID `ca9f5fa95ffab41fb9a615ab714db018`.
- **GUID Verification**:
  - Search for GUID `ca9f5fa95ffab41fb9a615ab714db018`: only found in `Tools/check_project.py:103` (whitelisted as a known built-in GUID) and inside the two scenes. No asset file exists on disk with this GUID.
  - File `Star Vanguard/Assets/InputSystem_Actions.inputactions.meta` line 2: `guid: 65b761383e570eb4390d8402a8c60d39`.
  - File `Star Vanguard/Assets/Settings/InputSystem_Actions.inputactions.meta` line 2: `guid: 2bcd2660ca9b64942af0de543d8d7100`.

### 1.3 `BootstrapScene.unity` Objects and Historical Origin
- **File**: `Star Vanguard/Assets/Scenes/BootstrapScene.unity` (945 lines total)
  - Line 136: `GameObject &519420028` named `Main Camera` (Tag: `MainCamera`).
    - Components:
      1. `Transform &519420032` (line 245)
      2. `Camera &519420031` (line 194)
      3. `AudioListener &519420029` (line 142)
      4. `UniversalAdditionalCameraData &519420030` (line 150, GUID `a79441f348de89743a2939f4d699eac1`)
      5. `FixedAspectCamera &519420033` (line 260, GUID `a12a5cad668220944bb31561fd870019`)
  - Line 328: `GameObject &619394800` named `Global Light 2D`.
    - Components:
      1. `Transform &619394802`
      2. `Light2D &619394801` (line 334, GUID `073797afb82c5a1438f328866b10b3f0`)
  - Other objects in scene:
    - Line 284: `BootstrapController &582285520` with `BootstrapLoader` (`Assets/Scripts/BootstrapLoader.cs`).
    - Persistent Managers: `SettingsManager &932644265`, `Managers &1081734898`, `PoolManager &1282297925`, `LeaderboardManager &1391629851`, `AudioManager &1481977717`, `GameManager &2082260714`.
- **File**: `Star Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs`
  - Lines 33–53: `k_NonBootstrapObjects` whitelist destroyed during bootstrap setup omitted `"Main Camera"` and `"Global Light 2D"`. They were copied over from the backup of `MainMenuScene` and left behind.
- **File**: `Star Vanguard/Assets/Scripts/BootstrapLoader.cs`
  - Lines 38–48: `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] private static void EnsureBootstrapInEditor()` loads `BootstrapScene` additively (`LoadSceneMode.Additive`) whenever Play Mode is entered directly from `MainMenuScene` or `GameplayScene`.

### 1.4 `Assets/InputSystem_Actions.inputactions` Action Map
- **File**: `Star Vanguard/Assets/InputSystem_Actions.inputactions`
  - Action map `"UI"` (lines 479–572):
    - `Navigate` (Vector2, lines 483–490)
    - `Submit` (Button, lines 492–499)
    - `Cancel` (Button, lines 501–508)
    - `Point` (Vector2, lines 510–517)
    - `Click` (Button, lines 519–526)
    - `RightClick` (Button, lines 528–535)
    - `MiddleClick` (Button, lines 537–544)
    - `ScrollWheel` (Vector2, lines 546–553)
    - `TrackedDevicePosition` (Vector3, lines 555–562)
    - `TrackedDeviceOrientation` (Quaternion, lines 564–571)
  - These names and types exactly match the parameters required by `InputSystemUIInputModuleEditor.ReassignActions(InputSystemUIInputModule, InputActionAsset)` in `com.unity.inputsystem`.

### 1.5 AGENTS.md Scene Manipulation Rules
- **File**: `AGENTS.md`
  - Rule 1 (Luật cứng 1): "KHÔNG sửa trực tiếp YAML của `.unity`, `.prefab`, `.asset` bằng tay hoặc regex. Dùng MCP (`manage_scene`, `manage_gameobject`, `manage_components`, `manage_prefabs`) hoặc Editor script."
  - Rule 8: Task modifying scene structure requires review and plan.
  - Single Scene Writer Lock requirement (`ORIGINAL_REQUEST.md` R2): Only one agent may mutate scene files at a time.

---

## 2. Logic Chain

1. **Legacy Co-op UI Cleanup (R1 / T-204a)**:
   - Observation 1.1 shows that `MainMenuScene.unity` currently contains `CoopPanel`, `CoopCard`, and `CoopButton`.
   - Observation 1.1 confirms that `StarVanguardCoopCleanup.cs` is already implemented and contains `RemoveFromMainMenuSceneAndSave()`.
   - In `StarVanguardCoopCleanup`, destroying `CoopPanel` automatically cascades to `CoopCard` via `hasLegacyAncestor`, while `CoopButton` is destroyed directly. The method marks the scene dirty and saves it.
   - Therefore, running `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave()` will eliminate the 3 dead objects and clear the `[WARN] legacy_coop` warning in `check_project.py`.

2. **Unresponsive Mouse Input on EventSystem (R2 / T-204b)**:
   - Observation 1.2 demonstrates that in both `MainMenuScene.unity` and `GameplayScene.unity`, `InputSystemUIInputModule` references missing GUID `ca9f5fa95ffab41fb9a615ab714db018`.
   - Observation 1.4 confirms that `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) contains the complete and valid `"UI"` action map.
   - Because `InputSystemUIInputModule` cannot resolve `m_ActionsAsset` or its sub-actions (`m_PointAction`, `m_LeftClickAction`), uGUI pointer raycasting and click events fail to register in Play Mode.
   - Per AGENTS.md Rule 1 (Observation 1.5), this fix must NOT be done via manual YAML edits or regex. It must be executed either via Unity MCP or via a dedicated Editor script that loads the scene, assigns the `InputActionAsset`, calls `InputSystemUIInputModuleEditor.ReassignActions(module, actionsAsset)` (or wires the serialized properties), and saves the scene.

3. **Duplicate Camera and Light in `BootstrapScene.unity` (R3 / T-204c)**:
   - Observation 1.3 shows that `BootstrapScene.unity` contains `Main Camera` (with an active `AudioListener` and `Camera`) and `Global Light 2D` (active `Light2D`).
   - When `BootstrapLoader.EnsureBootstrapInEditor()` runs additively before `MainMenuScene` or `GameplayScene`, the scene ends up with two cameras, two AudioListeners (causing Unity warnings), and double 2D lighting.
   - `BootstrapScene` is purely a persistent manager container that immediately switches scenes; it requires no camera, no audio listener, and no lighting.
   - Removing `Main Camera` and `Global Light 2D` from `BootstrapScene.unity` via an Editor script eliminates these duplicate conflicts without affecting persistent managers.

4. **Integration with Single Scene Writer Lock & Safety Gates**:
   - Because all three tasks (T-204a, T-204b, T-204c) touch scenes (`MainMenuScene.unity`, `GameplayScene.unity`, `BootstrapScene.unity`), they should be scheduled sequentially or executed under the Single Scene Writer lock to prevent concurrent modification collisions.
   - After these fixes, running `python Tools/check_project.py` will see `[WARN] legacy_coop` cleared, while G1 (`refresh_unity` / compile) and G2 (`StarVanguardPlayModeSmoke.Run`) will verify that mouse clicks on `LaunchButton` work.

---

## 3. Caveats

1. **`StarVanguardValidator.cs` Build Settings Assertion**:
   - `StarVanguardValidator.cs` line 129 asserts `scenes[0].path.EndsWith("MainMenuScene.unity")`. However, the project architecture set `BootstrapScene.unity` as index 0 (`EditorBuildSettings.asset`). If `StarVanguardValidator.ValidateAll` is executed as the G2 gate, that check and the check for `GameManager` in `GameplayScene` (which now lives in `BootstrapScene`) will fail unless `StarVanguardValidator.cs` is updated or `StarVanguardPlayModeSmoke.Run` is used as the primary G2 gate.
2. **Dual `.inputactions` Assets**:
   - There are two `.inputactions` files: `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) and `Assets/Settings/InputSystem_Actions.inputactions` (GUID `2bcd2660ca9b64942af0de543d8d7100`). Both have identical content, but `ORIGINAL_REQUEST.md` specifies GUID `65b761383e570eb4390d8402a8c60d39` (`Assets/InputSystem_Actions.inputactions`). Ensure the implementer uses the correct file path and GUID.
3. **No manual YAML editing**:
   - Even though changing a GUID in text might seem quick, AGENTS.md Rule 1 explicitly classifies manual YAML scene editing as an immediate revert violation. An Editor script must be used.

---

## 4. Conclusion

1. **R1 / T-204a**: Ready to execute immediately. `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave()` (`Assets/Editor/StarVanguardCoopCleanup.cs`) exists and directly targets `CoopPanel`, `CoopCard`, and `CoopButton`.
2. **R2 / T-204b**: The cause of unresponsive mouse input is fully identified: dangling ActionsAsset GUID `ca9f5fa95ffab41fb9a615ab714db018` on `InputSystemUIInputModule` in both `MainMenuScene.unity` and `GameplayScene.unity`. Reassigning to `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) via an Editor script will restore mouse and keyboard UI event dispatching.
3. **R3 / T-204c**: `Main Camera` and `Global Light 2D` in `BootstrapScene.unity` are confirmed redundant leftovers from the scene split. Removing them via an Editor script will resolve duplicate camera, dual audio listener, and lighting blowout bugs during additive loading.

---

## 5. Verification Method

### 5.1 Static Verification (Gate G3)
Run the project audit tool:
```powershell
python Tools/check_project.py
```
- **Expected before fix**: 0 FAIL, 10 WARN (including `[WARN] legacy_coop` with `CoopPanel`, `CoopCard`, `CoopButton`).
- **Expected after R1 fix**: `[WARN] legacy_coop` is eliminated; WARN count drops to 9.

### 5.2 Scene Inspection Verification
Inspect the YAML files (read-only):
- `MainMenuScene.unity`:
  - Verify absence of `m_Name: CoopPanel`, `m_Name: CoopCard`, `m_Name: CoopButton`.
  - Verify `InputSystemUIInputModule` (GameObject ID `501726004`) has `m_ActionsAsset` referencing GUID `65b761383e570eb4390d8402a8c60d39`.
- `GameplayScene.unity`:
  - Verify `InputSystemUIInputModule` (GameObject ID `501726004`) has `m_ActionsAsset` referencing GUID `65b761383e570eb4390d8402a8c60d39`.
- `BootstrapScene.unity`:
  - Verify absence of `m_Name: Main Camera` and `m_Name: Global Light 2D`.
  - Verify persistent managers (`GameManager`, `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager`) remain intact.

### 5.3 Play Mode & Functional Verification
Run the Play Mode smoke test in batchmode:
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile -
```
- Invalidation condition: If `InputSystemUIInputModule` still references `ca9f5fa95ffab41fb9a615ab714db018` or `BootstrapScene` still has duplicate AudioListeners, errors will be logged in the smoke test console output.
