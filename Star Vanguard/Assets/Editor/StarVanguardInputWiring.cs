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
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before modifying scenes.", "OK");
                }
                else
                {
                    Debug.LogError("[InputWiring] Cannot wire scenes while in Play Mode.");
                }
                return false;
            }

            Debug.Log("[InputWiring] ===== Beginning EventSystem Input Actions Wiring (T-204b) =====");

            bool menuSuccess = WireScene(k_MainMenuScenePath);
            bool gameplaySuccess = WireScene(k_GameplayScenePath);

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
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Exit Play Mode", "Stop Play Mode before modifying scenes.", "OK");
                }
                else
                {
                    Debug.LogError("[InputWiring] Cannot wire scene while in Play Mode.");
                }
                return false;
            }

            return WireScene(scenePath);
        }

        public static bool WireScene(string scenePath)
        {
            Debug.Log($"[InputWiring] Opening scene '{scenePath}'...");
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[InputWiring] Could not open scene: {scenePath}");
                return false;
            }

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
            Debug.Log($"[InputWiring] Loaded {actionRefs?.Length ?? 0} InputActionReference(s) for scene '{scenePath}'");

            InputSystemUIInputModule[] modules = UnityEngine.Object.FindObjectsByType<InputSystemUIInputModule>(
                FindObjectsInactive.Include);

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
            InputActionReference pointRef = FindActionReference(actionRefs, actionsAsset, "Point", "MousePosition", "Mouse Position");
            InputActionReference leftClickRef = FindActionReference(actionRefs, actionsAsset, "Click", "LeftClick", "Left Click");
            InputActionReference rightClickRef = FindActionReference(actionRefs, actionsAsset, "RightClick", "Right Click", "ContextClick");
            InputActionReference middleClickRef = FindActionReference(actionRefs, actionsAsset, "MiddleClick", "Middle Click");
            InputActionReference scrollWheelRef = FindActionReference(actionRefs, actionsAsset, "ScrollWheel", "Scroll Wheel", "Scroll", "Wheel");
            InputActionReference moveRef = FindActionReference(actionRefs, actionsAsset, "Navigate", "Move");
            InputActionReference submitRef = FindActionReference(actionRefs, actionsAsset, "Submit");
            InputActionReference cancelRef = FindActionReference(actionRefs, actionsAsset, "Cancel", "Esc", "Escape");
            InputActionReference trackPosRef = FindActionReference(actionRefs, actionsAsset, "TrackedDevicePosition", "Position");
            InputActionReference trackRotRef = FindActionReference(actionRefs, actionsAsset, "TrackedDeviceOrientation", "Orientation");

            // Verify critical mouse actions
            if (pointRef == null || leftClickRef == null)
            {
                Debug.LogError($"[InputWiring] Critical mouse action references could not be resolved! point={pointRef != null}, leftClick={leftClickRef != null}");
                if (actionRefs != null)
                {
                    foreach (var r in actionRefs)
                    {
                        if (r != null)
                        {
                            Debug.Log($"[InputWiring] Available ref: name='{r.name}', action='{(r.action != null ? r.action.name : "null")}'");
                        }
                    }
                }
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

        public static InputActionReference FindActionReference(InputActionReference[] references, InputActionAsset asset, params string[] candidateNames)
        {
            if (references != null)
            {
                foreach (string name in candidateNames)
                {
                    foreach (InputActionReference reference in references)
                    {
                        if (reference == null) continue;

                        if (reference.action != null && string.Equals(reference.action.name, name, StringComparison.OrdinalIgnoreCase))
                            return reference;

                        if (string.Equals(reference.name, name, StringComparison.OrdinalIgnoreCase))
                            return reference;

                        if (reference.name.EndsWith("/" + name, StringComparison.OrdinalIgnoreCase))
                            return reference;
                    }
                }
            }

            // Fallback: create from asset action directly if not in references
            if (asset != null)
            {
                foreach (string name in candidateNames)
                {
                    InputAction action = asset.FindAction("UI/" + name) ?? asset.FindAction(name);
                    if (action != null)
                    {
                        Debug.Log($"[InputWiring] Fallback: Created InputActionReference for '{name}' via asset.FindAction");
                        return InputActionReference.Create(action);
                    }
                }
            }

            return null;
        }
    }
}
