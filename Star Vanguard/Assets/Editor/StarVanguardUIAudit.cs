using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarVanguard.EditorTools
{
    /// <summary>
    /// Dumps the UI tree of a scene with the numbers that decide whether the player can
    /// click anything: rect size, anchors, active state, raycast targets and scroll rects.
    /// </summary>
    public static class StarVanguardUIAudit
    {
        private const string k_MenuScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string k_GameplayScenePath = "Assets/Scenes/GameplayScene.unity";
        private const int k_MaxDepth = 4;

        [MenuItem("Star Vanguard/Maintenance/Audit Menu UI")]
        public static void AuditMenu() => Audit(k_MenuScenePath, "MENU");

        [MenuItem("Star Vanguard/Maintenance/Audit Both Scenes")]
        public static void AuditAll()
        {
            Audit(k_MenuScenePath, "MENU");
            Audit(k_GameplayScenePath, "GAMEPLAY");
        }

        [MenuItem("Star Vanguard/Maintenance/Audit Gameplay UI")]
        public static void AuditGameplay() => Audit(k_GameplayScenePath, "GAMEPLAY");

        private static void Audit(string i_Path, string i_Tag)
        {
            Scene scene = EditorSceneManager.OpenScene(i_Path, OpenSceneMode.Single);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[UI-AUDIT] ===== {i_Tag} ({i_Path}) =====");

            GameObject canvas = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "Canvas") canvas = root;
            }

            if (canvas == null)
            {
                sb.AppendLine("[UI-AUDIT] no Canvas found");
                Debug.Log(sb.ToString());
                return;
            }

            foreach (Transform child in canvas.transform)
            {
                Dump(child, sb, 0);
            }

            Debug.Log(sb.ToString());
        }

        private static void Dump(Transform i_Node, StringBuilder io_Log, int i_Depth)
        {
            if (i_Depth > k_MaxDepth) return;

            RectTransform rect = i_Node as RectTransform;
            Image image = i_Node.GetComponent<Image>();
            ScrollRect scroll = i_Node.GetComponent<ScrollRect>();
            CanvasGroup group = i_Node.GetComponent<CanvasGroup>();

            string size = rect != null ? $"{rect.rect.width:F0}x{rect.rect.height:F0}" : "no-rect";
            string anchors = rect != null
                ? $"a({rect.anchorMin.x:F1},{rect.anchorMin.y:F1})-({rect.anchorMax.x:F1},{rect.anchorMax.y:F1})"
                : "-";
            string raycast = image != null ? (image.raycastTarget ? "RAYCAST" : "pass-through") : "-";
            string position = rect != null
                ? $"pos=({rect.anchoredPosition.x:F0},{rect.anchoredPosition.y:F0}) pivot=({rect.pivot.x:F1},{rect.pivot.y:F1})"
                : "-";
            string scrollInfo = scroll != null ? " SCROLLRECT" : string.Empty;
            string groupInfo = group != null
                ? $" cg(alpha={group.alpha:F2},blocks={group.blocksRaycasts})"
                : string.Empty;

            io_Log.AppendLine(
                $"[UI-AUDIT] {new string(' ', i_Depth * 2)}{i_Node.name} active={i_Node.gameObject.activeSelf} " +
                $"size={size} {anchors} {position} {raycast}{scrollInfo}{groupInfo}");

            foreach (Transform child in i_Node)
            {
                Dump(child, io_Log, i_Depth + 1);
            }
        }
    }
}
