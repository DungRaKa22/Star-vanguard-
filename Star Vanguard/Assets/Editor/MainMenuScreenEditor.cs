using UnityEditor;
using UnityEngine;
using StarVanguard.UI;

namespace StarVanguard.Editor
{
    [CustomEditor(typeof(MainMenuScreen))]
    public class MainMenuScreenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MainMenuScreen screen = (MainMenuScreen)target;
            Canvas canvas = screen.GetComponentInParent<Canvas>();
            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();

            Transform menuRoot = canvas != null ? canvas.transform.Find("MenuRoot") : null;

            EditorGUILayout.Space(12f);

            if (menuRoot == null)
            {
                EditorGUILayout.HelpBox(
                    "GIAO DIỆN CHƯA ĐƯỢC TẠO TRONG SCENE:\n" +
                    "Hiện tại Canvas đang trống (trắng tinh ở Edit Mode) do menu được tạo bằng code lúc chạy.\n\n" +
                    "Hãy bấm nút dưới đây để TẠO SẴN toàn bộ cây đối tượng (MenuRoot, các nút bấm, logo, thẻ điểm...) " +
                    "trực tiếp vào Scene để bạn có thể tự do kéo thả và căn chỉnh trực quan!",
                    MessageType.Warning);

                GUI.backgroundColor = new Color(0.2f, 0.8f, 1f);
                if (GUILayout.Button("Bake Menu to Scene (Tạo UI vào Scene để căn chỉnh)", GUILayout.Height(38)))
                {
                    screen.BakeHierarchy();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "GIAO DIỆN ĐÃ ĐƯỢC LƯU TRONG SCENE:\n" +
                    "Toàn bộ đối tượng MenuRoot đang nằm trong Hierarchy.\n" +
                    "Bạn có thể click chọn từng nút (LaunchButton, SettingsButton...), kéo thả vị trí, đổi kích thước, " +
                    "đổi font chữ hoặc màu sắc trực tiếp trên Scene View và Inspector.\n\n" +
                    "Mọi chỉnh sửa của bạn sẽ được giữ nguyên khi chạy game!",
                    MessageType.Info);

                if (GUILayout.Button("Re-bake Menu to Scene (Khôi phục bố cục mặc định)", GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Khôi phục Menu?",
                        "Hành động này sẽ tạo lại các nút bấm về vị trí chuẩn ban đầu. Bạn có muốn tiếp tục?",
                        "Đồng ý", "Huỷ"))
                    {
                        screen.BakeHierarchy();
                    }
                }
            }

            if (canvas != null)
            {
                UIPanelPreviewToolbar.DrawPreviewToolbar(canvas);
            }
        }
    }
}
