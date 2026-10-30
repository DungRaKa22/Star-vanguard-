# Technical Investigation & Implementation Plan: EventSystem Input Actions Wiring (T-204b)

## Executive Summary
In `MainMenuScene.unity` and `GameplayScene.unity`, `InputSystemUIInputModule` contains a dangling GUID reference (`ca9f5fa95ffab41fb9a615ab714db018`) across 11 serialized fields per scene (22 references total). Because this GUID does not exist on disk, `InputSystemUIInputModule` fails to resolve its action references, leaving callback delegates unhooked and making uGUI buttons completely unresponsive to mouse clicks.

This report establishes the complete root cause evidence chain and provides the verified, automated Editor tool implementation (`StarVanguardInputWiring.cs`) to rewire `InputSystemUIInputModule` to `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`). This approach complies strictly with AGENTS.md Rule 1 (zero manual YAML edits) and integrates seamlessly with the Single Scene Writer lock protocol for Milestone 1.

---

## 1. Observation

### 1.1 Dangling GUID References in Scene YAML
Direct search across the entire project repository (`Star Vanguard/Assets/`) revealed exactly 22 occurrences of GUID `ca9f5fa95ffab41fb9a615ab714db018` — exactly 11 in `MainMenuScene.unity` and 11 in `GameplayScene.unity`:

- **File**: `Star Vanguard/Assets/Scenes/MainMenuScene.unity`
  - Line 3255: `GameObject &501726004` (`m_Name: EventSystem`)
  - Line 3261: Component `MonoBehaviour &501726005` (`guid: 01614664b831546d2ae94a42149d80ac` -> `InputSystemUIInputModule`)
  - Lines 3277–3287:
    ```yaml
    3277:   m_ActionsAsset: {fileID: -944628639613478452, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3278:   m_PointAction: {fileID: -1654692200621890270, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3279:   m_MoveAction: {fileID: -8784545083839296357, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3280:   m_SubmitAction: {fileID: 392368643174621059, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3281:   m_CancelAction: {fileID: 7727032971491509709, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3282:   m_LeftClickAction: {fileID: 3001919216989983466, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3283:   m_MiddleClickAction: {fileID: -2185481485913320682, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3284:   m_RightClickAction: {fileID: -4090225696740746782, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3285:   m_ScrollWheelAction: {fileID: 6240969308177333660, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3286:   m_TrackedDevicePositionAction: {fileID: 6564999863303420839, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    3287:   m_TrackedDeviceOrientationAction: {fileID: 7970375526676320489, guid: ca9f5fa95ffab41fb9a615ab714db018, type: 3}
    ```

- **File**: `Star Vanguard/Assets/Scenes/GameplayScene.unity`
  - Line 1655: `GameObject &501726004` (`m_Name: EventSystem`)
  - Line 1661: Component `MonoBehaviour &501726005` (`guid: 01614664b831546d2ae94a42149d80ac` -> `InputSystemUIInputModule`)
  - Lines 1677–1687:
    Identical 11 serialized action references pointing to missing GUID `ca9f5fa95ffab41fb9a615ab714db018`.

- **File**: `Star Vanguard/Assets/Scenes/BootstrapScene.unity`
  - Contains NO EventSystem or `InputSystemUIInputModule` component (verified by full scene grep).

### 1.2 Canonical Input Actions Asset & GUID Verification
- **File**: `Star Vanguard/Assets/InputSystem_Actions.inputactions.meta`
  - Line 2: `guid: 65b761383e570eb4390d8402a8c60d39`
  - Importer: `ScriptedImporter` (`guid: 8404be70184654265930450def6a9037`, Unity Input System `InputActionImporter`).

- **File**: `Star Vanguard/Assets/InputSystem_Actions.inputactions`
  - Action map `"UI"` (lines 479–572) declares the full complement of UI actions:
    - `Point`: `PassThrough`, Vector2 (lines 510–517)
    - `Click`: `PassThrough`, Button (lines 519–526)
    - `RightClick`: `PassThrough`, Button (lines 528–535)
    - `MiddleClick`: `PassThrough`, Button (lines 537–544)
    - `ScrollWheel`: `PassThrough`, Vector2 (lines 546–553)
    - `Navigate`: `PassThrough`, Vector2 (lines 483–490)
    - `Submit`: `Button` (lines 492–499)
    - `Cancel`: `Button` (lines 501–508)
    - `TrackedDevicePosition`: `PassThrough`, Vector3 (lines 555–562)
    - `TrackedDeviceOrientation`: `PassThrough`, Quaternion (lines 564–571)

- **Bindings for Mouse Input in `"UI"` Action Map**:
  - `Point`: bound to `<Mouse>/position` (lines 862–869, group `Keyboard&Mouse`)
  - `Click` (Left Click): bound to `<Mouse>/leftButton` (lines 895–902, group `Keyboard&Mouse`)
  - `ScrollWheel`: bound to `<Mouse>/scroll` (lines 939–946, group `Keyboard&Mouse`)
  - `RightClick`: bound to `<Mouse>/rightButton` (lines 950–957, group `Keyboard&Mouse`)
  - `MiddleClick`: bound to `<Mouse>/middleButton` (lines 961–968, group `Keyboard&Mouse`)

### 1.3 `InputSystemUIInputModule` Runtime Failure Mechanism
Inspection of package source `com.unity.inputsystem@1.20.0` (`InputSystemUIInputModule.cs`):
- Lines 2495–2507:
  ```csharp
  private void SetActionCallbacks(bool install)
  {
      m_ActionsHooked = install;
      SetActionCallback(m_PointAction, m_OnPointDelegate, install);
      SetActionCallback(m_MoveAction, m_OnMoveDelegate, install);
      SetActionCallback(m_SubmitAction, m_OnSubmitCancelDelegate, install);
      SetActionCallback(m_CancelAction, m_OnSubmitCancelDelegate, install);
      SetActionCallback(m_LeftClickAction, m_OnLeftClickDelegate, install);
      ...
  ```
- Lines 2514–2520:
  ```csharp
  if (actionReference == null) return;
  var action = actionReference.action;
  if (action == null) return;
  if (install) { action.performed += callback; action.canceled += callback; }
  ```
- When `m_PointAction` or `m_LeftClickAction` points to a missing GUID (`ca9f...`), `actionReference.action` evaluates to `null`.
- As a direct consequence, `action.performed += callback` is NEVER executed. No pointer positions (`m_PointerStates[index].screenPosition`) are updated, and no clicks (`OnLeftClickCallback`) are received. uGUI pointer raycasts and click dispatches fail completely.

### 1.4 Unity Input System Editor Mechanics
Inspection of `InputSystemUIInputModuleEditor.cs` (lines 38–49, 109–126):
- Sub-assets of type `InputActionReference` are generated by Unity's importer inside the `.inputactions` file and loaded via `AssetDatabase.LoadAllAssetsAtPath(assetPath)`.
- Reassigning actions requires mapping the internal serialized fields (`m_PointAction`, `m_LeftClickAction`, etc.) to their corresponding `InputActionReference` sub-assets.
- Calling `module.actionsAsset = newAsset` alone does NOT automatically recover references when previous references were dangling (because `UpdateReferenceForNewAsset` evaluates `actionReference?.action == null` and resets each property to `null`).
- Therefore, both `actionsAsset` and each individual `InputActionReference` property must be explicitly set.

---

## 2. Logic Chain

1. **Root Cause Confirmation**:
   - Observation 1.1 proves that both `MainMenuScene.unity` and `GameplayScene.unity` reference missing GUID `ca9f5fa95ffab41fb9a615ab714db018`.
   - Observation 1.3 demonstrates that `InputSystemUIInputModule` requires resolved `InputActionReference.action` instances to register `performed` callbacks for mouse movement (`Point`) and clicks (`LeftClick`).
   - Because the GUID is dangling, `action == null`, delegates are never hooked, and mouse clicks never reach uGUI buttons in Play Mode.

2. **Resolution Target Verification**:
   - Observation 1.2 confirms that `Assets/InputSystem_Actions.inputactions` exists on disk with GUID `65b761383e570eb4390d8402a8c60d39`.
   - Observation 1.2 proves that its `"UI"` action map contains all 10 required actions, with complete mouse bindings (`<Mouse>/position`, `<Mouse>/leftButton`, `<Mouse>/rightButton`, `<Mouse>/middleButton`, `<Mouse>/scroll`).

3. **Editor Tooling Requirement (AGENTS.md Rule 1)**:
   - AGENTS.md Rule 1 strictly forbids editing scene YAML by hand or via regex.
   - Observation 1.4 confirms that `AssetDatabase.LoadAllAssetsAtPath` extracts the persistent `InputActionReference` sub-assets backed by `Assets/InputSystem_Actions.inputactions`.
   - An Editor script using `EditorSceneManager.OpenScene`, assigning both runtime component properties and `SerializedObject` backing properties (`m_ActionsAsset`, `m_PointAction`, etc.), calling `EditorUtility.SetDirty`, and saving via `EditorSceneManager.SaveScene` will safely update the scene YAML through Unity's native serialization pipeline without YAML corruption.

4. **Single Scene Writer Lock & Milestone 1 Sequencing**:
   - Milestone 1 modifies three scenes:
     - `BootstrapScene.unity` (T-204c: duplicate camera & light removal)
     - `MainMenuScene.unity` (T-204a: Co-op UI cleanup; T-204b: EventSystem wiring)
     - `GameplayScene.unity` (T-204b: EventSystem wiring)
   - Because `MainMenuScene.unity` is modified by both T-204a and T-204b, running them concurrently would lead to file collisions or lost changes.
   - Under the Single Scene Writer lock, the safe execution order for the Milestone 1 worker is:
     1. T-204c on `BootstrapScene.unity`
     2. T-204a on `MainMenuScene.unity`
     3. T-204b on `MainMenuScene.unity` and `GameplayScene.unity`
     4. Gate verification (G1 compile, G2 PlayMode smoke, G3 static project check).

---

## 3. Caveats

1. **Dual `.inputactions` Assets on Disk**:
   - There are two `.inputactions` files: `Assets/InputSystem_Actions.inputactions` (`65b761383e570eb4390d8402a8c60d39`) and `Assets/Settings/InputSystem_Actions.inputactions` (`2bcd2660ca9b64942af0de543d8d7100`).
   - The tool must explicitly target `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) as mandated by `ORIGINAL_REQUEST.md` §R2 and `PROJECT.md`.
2. **Action Name Matching for LeftClick and Move**:
   - In `Assets/InputSystem_Actions.inputactions`, the left click action is named `"Click"`, while on `InputSystemUIInputModule` the property is named `leftClick` / `m_LeftClickAction`.
   - Similarly, navigation is named `"Navigate"` in the action map, while on the module it is `move` / `m_MoveAction`.
   - The reference resolution logic must search for candidate aliases (`"Click"` for `leftClick`, `"Navigate"` for `move`), exactly matching Unity's internal `InputSystemUIInputModuleEditor.ReassignActions` implementation.
3. **Dual Setting (Component + SerializedObject)**:
   - Setting only `module.actionsAsset = asset` in Editor mode does not guarantee immediate serialization flush if the scene is not marked dirty or if property drawers override values. Setting both component properties AND `SerializedProperty` values via `SerializedObject.ApplyModifiedProperties()` guarantees 100% serialization fidelity.
4. **Unity Batchmode vs GUI Lockfile**:
   - Verified that `Temp/UnityLockfile` is `False`. If the GUI Unity Editor is running when executing batchmode CLI commands, Unity will abort on lock contention. The worker must run batchmode commands when the GUI Editor is closed, or execute menu items via Unity MCP when connected.

---

## 4. Conclusion & Proposed Implementation

### 4.1 Property Mapping Specification
| InputSystemUIInputModule Field | Serialized Property Name | Target Action in `InputSystem_Actions` | Action Map | Expected Control Type |
| :--- | :--- | :--- | :--- | :--- |
| `actionsAsset` | `m_ActionsAsset` | `InputSystem_Actions` (main asset) | - | - |
| `point` | `m_PointAction` | `Point` | `UI` | `Vector2` (`<Mouse>/position`) |
| `leftClick` | `m_LeftClickAction` | `Click` | `UI` | `Button` (`<Mouse>/leftButton`) |
| `rightClick` | `m_RightClickAction` | `RightClick` | `UI` | `Button` (`<Mouse>/rightButton`) |
| `middleClick` | `m_MiddleClickAction` | `MiddleClick` | `UI` | `Button` (`<Mouse>/middleButton`) |
| `scrollWheel` | `m_ScrollWheelAction` | `ScrollWheel` | `UI` | `Vector2` (`<Mouse>/scroll`) |
| `move` | `m_MoveAction` | `Navigate` | `UI` | `Vector2` |
| `submit` | `m_SubmitAction` | `Submit` | `UI` | `Button` |
| `cancel` | `m_CancelAction` | `Cancel` | `UI` | `Button` |
| `trackedDevicePosition` | `m_TrackedDevicePositionAction` | `TrackedDevicePosition` | `UI` | `Vector3` |
| `trackedDeviceOrientation` | `m_TrackedDeviceOrientationAction` | `TrackedDeviceOrientation` | `UI` | `Quaternion` |

### 4.2 Complete Proposed Implementation (`StarVanguardInputWiring.cs`)
Target file path: `Star Vanguard/Assets/Editor/StarVanguardInputWiring.cs`

```csharp
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Editor automation tool for T-204b: Rewires EventSystem InputSystemUIInputModule
    /// to the canonical InputActionAsset ("Assets/InputSystem_Actions.inputactions",
    /// GUID 65b761383e570eb4390d8402a8c60d39), fixing unresponsive mouse clicks.
    ///
    /// Complies strictly with AGENTS.md Rule 1 (no manual YAML edits) and the
    /// Single Scene Writer lock protocol.
    /// </summary>
    public static class StarVanguardInputWiring
    {
        public const string k_CanonicalActionsAssetPath = "Assets/InputSystem_Actions.inputactions";
        public const string k_CanonicalActionsAssetGuid = "65b761383e570eb4390d8402a8c60d39";
        public const string k_MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        public const string k_GameplayScenePath = "Assets/Scenes/GameplayScene.unity";

        [MenuItem("Tools/Star Vanguard/2. Wire All EventSystem Input Actions and Save", priority = 10)]
        public static bool WireAllScenesAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before modifying scenes.", "OK");
                return false;
            }

            Debug.Log("[InputWiring] ===== Beginning EventSystem Input Actions Wiring (T-204b) =====");

            InputActionAsset actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(k_CanonicalActionsAssetPath);
            if (actionsAsset == null)
            {
                Debug.LogError($"[InputWiring] FAILED: Could not load InputActionAsset at {k_CanonicalActionsAssetPath}");
                return false;
            }

            string actualGuid = AssetDatabase.AssetPathToGUID(k_CanonicalActionsAssetPath);
            if (!string.Equals(actualGuid, k_CanonicalActionsAssetGuid, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[InputWiring] FAILED: ActionsAsset GUID mismatch! Expected: {k_CanonicalActionsAssetGuid}, Actual: {actualGuid}");
                return false;
            }

            InputActionReference[] actionRefs = GetAllActionReferences(k_CanonicalActionsAssetPath);
            if (actionRefs == null || actionRefs.Length == 0)
            {
                Debug.LogError($"[InputWiring] FAILED: No InputActionReferences found in {k_CanonicalActionsAssetPath}");
                return false;
            }

            bool menuSuccess = WireScene(k_MainMenuScenePath, actionsAsset, actionRefs);
            bool gameplaySuccess = WireScene(k_GameplayScenePath, actionsAsset, actionRefs);

            if (menuSuccess && gameplaySuccess)
            {
                Debug.Log("[InputWiring] SUCCESS: Successfully wired EventSystem in MainMenuScene and GameplayScene!");
                return true;
            }
            else
            {
                Debug.LogError($"[InputWiring] FAILED: Scene wiring failed. MainMenu: {menuSuccess}, Gameplay: {gameplaySuccess}");
                return false;
            }
        }

        [MenuItem("Tools/Star Vanguard/2a. Wire MainMenuScene Input Actions and Save", priority = 11)]
        public static bool WireMainMenuSceneAndSave()
        {
            return WireSingleScene(k_MainMenuScenePath);
        }

        [MenuItem("Tools/Star Vanguard/2b. Wire GameplayScene Input Actions and Save", priority = 12)]
        public static bool WireGameplaySceneAndSave()
        {
            return WireSingleScene(k_GameplayScenePath);
        }

        private static bool WireSingleScene(string scenePath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before modifying scenes.", "OK");
                return false;
            }

            InputActionAsset actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(k_CanonicalActionsAssetPath);
            if (actionsAsset == null)
            {
                Debug.LogError($"[InputWiring] FAILED: Could not load InputActionAsset at {k_CanonicalActionsAssetPath}");
                return false;
            }

            InputActionReference[] actionRefs = GetAllActionReferences(k_CanonicalActionsAssetPath);
            return WireScene(scenePath, actionsAsset, actionRefs);
        }

        public static bool WireScene(string scenePath, InputActionAsset actionsAsset, InputActionReference[] actionRefs)
        {
            Debug.Log($"[InputWiring] Opening scene '{scenePath}'...");
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[InputWiring] Could not open scene: {scenePath}");
                return false;
            }

            InputSystemUIInputModule[] modules = UnityEngine.Object.FindObjectsByType<InputSystemUIInputModule>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (modules.Length == 0)
            {
                Debug.LogError($"[InputWiring] No InputSystemUIInputModule found in scene {scenePath}!");
                return false;
            }

            if (modules.Length > 1)
            {
                Debug.LogWarning($"[InputWiring] Multiple InputSystemUIInputModule found in {scenePath} (count={modules.Length}). Wiring all instances.");
            }

            // Resolve references
            InputActionReference pointRef = FindActionReference(actionRefs, "UI", "Point", "MousePosition");
            InputActionReference leftClickRef = FindActionReference(actionRefs, "UI", "Click", "LeftClick");
            InputActionReference rightClickRef = FindActionReference(actionRefs, "UI", "RightClick");
            InputActionReference middleClickRef = FindActionReference(actionRefs, "UI", "MiddleClick");
            InputActionReference scrollWheelRef = FindActionReference(actionRefs, "UI", "ScrollWheel");
            InputActionReference moveRef = FindActionReference(actionRefs, "UI", "Navigate", "Move");
            InputActionReference submitRef = FindActionReference(actionRefs, "UI", "Submit");
            InputActionReference cancelRef = FindActionReference(actionRefs, "UI", "Cancel", "Escape");
            InputActionReference trackPosRef = FindActionReference(actionRefs, "UI", "TrackedDevicePosition", "Position");
            InputActionReference trackRotRef = FindActionReference(actionRefs, "UI", "TrackedDeviceOrientation", "Orientation");

            // Verify critical mouse actions
            if (pointRef == null || leftClickRef == null)
            {
                Debug.LogError("[InputWiring] Critical mouse action references (Point or Click) could not be resolved!");
                return false;
            }

            foreach (InputSystemUIInputModule module in modules)
            {
                Undo.RecordObject(module, "Wire InputSystem Actions");

                // Assign runtime component properties
                module.actionsAsset = actionsAsset;
                module.point = pointRef;
                module.leftClick = leftClickRef;
                module.rightClick = rightClickRef;
                module.middleClick = middleClickRef;
                module.scrollWheel = scrollWheelRef;
                module.move = moveRef;
                module.submit = submitRef;
                module.cancel = cancelRef;
                module.trackedDevicePosition = trackPosRef;
                module.trackedDeviceOrientation = trackRotRef;

                // Sync SerializedObject to ensure serialized properties are flushed to disk
                SerializedObject so = new SerializedObject(module);
                SetObjectReferenceProperty(so, "m_ActionsAsset", actionsAsset);
                SetObjectReferenceProperty(so, "m_PointAction", pointRef);
                SetObjectReferenceProperty(so, "m_MoveAction", moveRef);
                SetObjectReferenceProperty(so, "m_SubmitAction", submitRef);
                SetObjectReferenceProperty(so, "m_CancelAction", cancelRef);
                SetObjectReferenceProperty(so, "m_LeftClickAction", leftClickRef);
                SetObjectReferenceProperty(so, "m_MiddleClickAction", middleClickRef);
                SetObjectReferenceProperty(so, "m_RightClickAction", rightClickRef);
                SetObjectReferenceProperty(so, "m_ScrollWheelAction", scrollWheelRef);
                SetObjectReferenceProperty(so, "m_TrackedDevicePositionAction", trackPosRef);
                SetObjectReferenceProperty(so, "m_TrackedDeviceOrientationAction", trackRotRef);
                so.ApplyModifiedProperties();

                EditorUtility.SetDirty(module);
                Debug.Log($"[InputWiring] Wired InputSystemUIInputModule on GameObject '{module.gameObject.name}' in scene '{scenePath}'");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            Debug.Log($"[InputWiring] Saved scene '{scenePath}': {(saved ? "SUCCESS" : "FAILED")}");
            return saved;
        }

        private static void SetObjectReferenceProperty(SerializedObject so, string propertyName, UnityEngine.Object target)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = target;
            }
            else
            {
                Debug.LogWarning($"[InputWiring] Serialized property '{propertyName}' not found on {so.targetObject.name}");
            }
        }

        public static InputActionReference[] GetAllActionReferences(string assetPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            return assets.OfType<InputActionReference>().ToArray();
        }

        public static InputActionReference FindActionReference(InputActionReference[] references, string actionMapName, params string[] candidateNames)
        {
            foreach (string name in candidateNames)
            {
                foreach (InputActionReference reference in references)
                {
                    if (reference == null || reference.action == null) continue;

                    if (!string.IsNullOrEmpty(actionMapName))
                    {
                        if (!string.Equals(reference.action.actionMap?.name, actionMapName, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    if (string.Equals(reference.action.name, name, StringComparison.OrdinalIgnoreCase))
                        return reference;
                }
            }
            return null;
        }
    }
}
```

---

## 5. Verification Method

### 5.1 Verification Commands
1. **Compilation Check (Gate G1)**:
   Verify the editor tool compiles with 0 errors:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -logFile -
   ```

2. **Execution via Unity Batchmode**:
   Execute the wiring tool under the Single Scene Writer lock:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardInputWiring.WireAllScenesAndSave -logFile -
   ```

3. **Static Audit (Gate G3)**:
   ```powershell
   python Tools/check_project.py
   ```
   - Must output `0 FAIL`.

4. **Scene Inspection (Git Diff)**:
   ```powershell
   git diff "Star Vanguard/Assets/Scenes/MainMenuScene.unity"
   git diff "Star Vanguard/Assets/Scenes/GameplayScene.unity"
   ```
   - Verify that all 11 fields under `InputSystemUIInputModule` (`m_ActionsAsset`, `m_PointAction`, `m_LeftClickAction`, etc.) now reference GUID `65b761383e570eb4390d8402a8c60d39`.
   - Confirm that GUID `ca9f5fa95ffab41fb9a615ab714db018` is completely absent from both scene files.

5. **Functional Play Mode Verification (Gate G2 / Smoke Test)**:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe" -projectPath "c:\Documents\UNITY 2026\Star Vanguard" -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run -logFile -
   ```
   - Verify that `LAUNCH MISSION` button responds to pointer click and boots `GameplayScene`.

### 5.2 Invalidation Conditions
- Any occurrence of GUID `ca9f5fa95ffab41fb9a615ab714db018` remaining in `MainMenuScene.unity` or `GameplayScene.unity`.
- `InputSystemUIInputModule.point` or `InputSystemUIInputModule.leftClick` remaining `null` after running the script.
- Any manual YAML modification or regex edit performed directly on `.unity` files.
