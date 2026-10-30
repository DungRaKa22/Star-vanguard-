using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Editor cleanup tool for T-204c:
    /// Removes duplicate 'Main Camera' and 'Global Light 2D' from BootstrapScene.unity.
    ///
    /// These objects were leftover copies from MainMenuScene during initial bootstrap setup.
    /// When BootstrapScene is loaded additively in Editor, they cause:
    /// 1. "There are 2 audio listeners in the scene" warning.
    /// 2. Duplicate Main Cameras competing for rendering.
    /// 3. Double URP 2D Global Light intensity (200% blown-out lighting).
    ///
    /// Idempotent: safe to run multiple times.
    /// </summary>
    public static class StarVanguardBootstrapCleanup
    {
        private const string k_BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
        private static readonly string[] k_DuplicateObjectNames = { "Main Camera", "Global Light 2D" };

        [MenuItem("Tools/Star Vanguard/1c. Remove Duplicate Camera and Light from BootstrapScene and Save", priority = 3)]
        public static void RemoveDuplicatesAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Exit Play Mode first",
                        "Stop Play Mode before cleaning BootstrapScene.",
                        "OK");
                }
                else
                {
                    Debug.LogError("[Bootstrap Cleanup] Cannot clean scene while in Play Mode.");
                }
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.path != k_BootstrapScenePath)
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }
                active = EditorSceneManager.OpenScene(k_BootstrapScenePath, OpenSceneMode.Single);
            }

            int removed = removeFromScene(active);

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(active);
                EditorSceneManager.SaveScene(active);
                Debug.Log($"[Bootstrap Cleanup] Removed {removed} duplicate object(s) from BootstrapScene and saved it.");
            }
            else
            {
                Debug.Log("[Bootstrap Cleanup] BootstrapScene has no duplicate objects. Nothing to save.");
            }
        }

        private static int removeFromScene(Scene i_Scene)
        {
            int removed = 0;
            List<GameObject> toDestroy = new List<GameObject>();

            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                if (root == null) continue;

                if (System.Array.IndexOf(k_DuplicateObjectNames, root.name) >= 0)
                {
                    toDestroy.Add(root);
                }
            }

            foreach (GameObject go in toDestroy)
            {
                Debug.Log($"[Bootstrap Cleanup] Destroying '{go.name}' in scene '{i_Scene.name}'.");
                Undo.DestroyObjectImmediate(go);
                removed++;
            }

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(i_Scene);
            }

            return removed;
        }
    }
}
