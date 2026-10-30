using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// One-click cleanup for the co-op UI that ADR-001 removed from the codebase
    /// (docs/ADR-001-coop.md).
    ///
    /// Why this exists instead of editing MainMenuScene.unity by hand: Unity scene files are
    /// YAML with fileIDs and GUIDs, and a hand edit that leaves a dangling reference corrupts
    /// the scene. This tool deletes the leftover objects through the editor API, which lets
    /// Unity rewrite the YAML correctly (and drop the now-orphaned serialized fields).
    ///
    /// Idempotent: running it twice is safe, and running it on a clean scene just reports zero.
    /// </summary>
    public static class StarVanguardCoopCleanup
    {
        private const string k_MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";

        // Objects created by the LAN co-op feature that no longer exists in code.
        private static readonly string[] k_LegacyCoopNames = { "CoopPanel", "CoopCard", "CoopButton" };

        [MenuItem("Tools/Star Vanguard/1. Remove Legacy Co-op UI (ADR-001)", priority = 1)]
        public static void RemoveFromOpenScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Exit Play Mode first",
                    "Stop Play Mode before cleaning the scenes.",
                    "OK");
                return;
            }

            int removed = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                removed += removeFromScene(scene);
            }

            if (removed == 0)
            {
                Debug.Log("[Co-op Cleanup] No legacy co-op objects found in the open scenes. Nothing to do.");
            }
            else
            {
                Debug.Log($"[Co-op Cleanup] Removed {removed} legacy co-op object(s). Save your scenes to persist the change.");
            }
        }

        [MenuItem("Tools/Star Vanguard/1b. Remove Legacy Co-op UI from MainMenuScene and Save", priority = 2)]
        public static void RemoveFromMainMenuSceneAndSave()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Exit Play Mode first",
                    "Stop Play Mode before cleaning MainMenuScene.",
                    "OK");
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.path != k_MainMenuScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                active = EditorSceneManager.OpenScene(k_MainMenuScenePath, OpenSceneMode.Single);
            }

            int removed = removeFromScene(active);

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(active);
                EditorSceneManager.SaveScene(active);
                Debug.Log($"[Co-op Cleanup] Removed {removed} legacy co-op object(s) from MainMenuScene and saved it.");
            }
            else
            {
                Debug.Log("[Co-op Cleanup] MainMenuScene has no legacy co-op objects. Nothing to save.");
            }
        }

        private static int removeFromScene(Scene i_Scene)
        {
            int removed = 0;

            foreach (GameObject root in i_Scene.GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate == null) continue;

                    bool isLegacyName = System.Array.IndexOf(k_LegacyCoopNames, candidate.name) >= 0;
                    if (!isLegacyName) continue;

                    // Skip a child whose parent is already in the removal list: destroying the
                    // parent takes care of it, and touching it here would throw an invalid object.
                    if (hasLegacyAncestor(candidate)) continue;

                    Debug.Log($"[Co-op Cleanup] Destroying '{candidate.name}' in scene '{i_Scene.name}'.");
                    Undo.DestroyObjectImmediate(candidate.gameObject);
                    removed++;
                }
            }

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(i_Scene);
            }

            return removed;
        }

        private static bool hasLegacyAncestor(Transform i_Transform)
        {
            for (Transform parent = i_Transform.parent; parent != null; parent = parent.parent)
            {
                if (System.Array.IndexOf(k_LegacyCoopNames, parent.name) >= 0) return true;
            }

            return false;
        }
    }
}
