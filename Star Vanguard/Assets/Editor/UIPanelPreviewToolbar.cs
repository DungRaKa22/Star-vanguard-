using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarVanguard.Editor
{
    /// <summary>
    /// Interactive Editor Toolbar to switch and visually edit any UI panel in Scene View without overlapping.
    /// </summary>
    [InitializeOnLoad]
    public static class UIPanelPreviewToolbar
    {
        private static readonly string[] k_AllPanels =
        {
            "MenuRoot",
            "SettingsPanel",
            "ControlsPanel",
            "LeaderboardPanel",
            "PausePanel",
            "GameOverPanel",
            "NewHighScorePanel"
        };

        static UIPanelPreviewToolbar()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange i_State)
        {
            if (i_State == PlayModeStateChange.ExitingEditMode)
            {
                // Ensure only the default boot panel is active before entering Play Mode
                ResetToBootState();
            }
        }

        [MenuItem("Star Vanguard/UI/Preview Panel/1. Home (Menu Chinh) #&1")]
        public static void ShowHome() => SwitchToPanel("MenuRoot");

        [MenuItem("Star Vanguard/UI/Preview Panel/2. Settings (Cai Dat) #&2")]
        public static void ShowSettings() => SwitchToPanel("SettingsPanel");

        [MenuItem("Star Vanguard/UI/Preview Panel/3. Controls (Huong Dan) #&3")]
        public static void ShowControls() => SwitchToPanel("ControlsPanel");

        // The Co-op panel was deleted in ADR-001 (docs/ADR-001-coop.md); its preview entry is gone too.
        [MenuItem("Star Vanguard/UI/Preview Panel/4. Reset To Boot State #&0")]
        public static void ResetToBootState()
        {
            SwitchToPanel("MenuRoot");
        }

        public static void SwitchToPanel(string i_TargetPanelName)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform canvasTrans = canvas.transform;
            bool targetFound = false;

            foreach (string panelName in k_AllPanels)
            {
                Transform panel = canvasTrans.Find(panelName);
                if (panel != null)
                {
                    bool shouldBeActive = (panelName == i_TargetPanelName);
                    if (shouldBeActive) targetFound = true;

                    if (panel.gameObject.activeSelf != shouldBeActive)
                    {
                        Undo.RecordObject(panel.gameObject, $"Switch UI Preview to {i_TargetPanelName}");
                        panel.gameObject.SetActive(shouldBeActive);
                        EditorUtility.SetDirty(panel.gameObject);
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

            if (targetFound)
            {
                Debug.Log($"[UIPanelPreview] Dang xem truoc va can chinh: '{i_TargetPanelName}'. Cac panel khac da duoc tam an.");
            }
        }

        public static void DrawPreviewToolbar(Canvas i_Canvas)
        {
            if (i_Canvas == null) return;

            Transform canvasTrans = i_Canvas.transform;
            string currentVisible = "Khong ro";

            foreach (string name in k_AllPanels)
            {
                Transform p = canvasTrans.Find(name);
                if (p != null && p.gameObject.activeSelf)
                {
                    currentVisible = name;
                    break;
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("🎨 XEM TRƯỚC & CĂN CHỈNH TỪNG GIAO DIỆN (PANEL PREVIEW):", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"Đang hiển thị trên Scene View: [{currentVisible}]\n" +
                "Bấm các nút dưới đây để chuyển đổi panel bạn muốn căn chỉnh. " +
                "Hệ thống sẽ chỉ hiển thị duy nhất panel đó để tránh đè chồng!",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = currentVisible == "MenuRoot" ? new Color(0.2f, 1f, 0.4f) : Color.white;
            if (GUILayout.Button("🏠 Menu Chính", GUILayout.Height(30))) SwitchToPanel("MenuRoot");

            GUI.backgroundColor = currentVisible == "SettingsPanel" ? new Color(0.2f, 0.8f, 1f) : Color.white;
            if (GUILayout.Button("⚙️ Cài Đặt (Settings)", GUILayout.Height(30))) SwitchToPanel("SettingsPanel");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = currentVisible == "ControlsPanel" ? new Color(1f, 0.8f, 0.2f) : Color.white;
            if (GUILayout.Button("📖 Hướng Dẫn (Controls)", GUILayout.Height(30))) SwitchToPanel("ControlsPanel");
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("🔄 Đặt Lại Trạng Thái Chạy Game (Chỉ hiện Menu Chính)", GUILayout.Height(24)))
            {
                ResetToBootState();
            }

            EditorGUILayout.Space(4f);
        }
    }
}
