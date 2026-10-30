using UnityEditor;
using UnityEngine;
using StarVanguard.UI;

namespace StarVanguard.Editor
{
    [CustomEditor(typeof(UIPanelRouter))]
    public class UIPanelRouterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UIPanelRouter router = (UIPanelRouter)target;
            Canvas canvas = router.GetComponentInParent<Canvas>();
            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                UIPanelPreviewToolbar.DrawPreviewToolbar(canvas);
            }
        }
    }
}
