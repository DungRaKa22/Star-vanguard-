# Star Vanguard — Space Combat Arcade Shooter

Dự án game bắn máy bay không gian khoa học viễn tưởng 2D xây dựng trên **Unity 6 (6000.5.7f1)**, sử dụng Universal Render Pipeline (URP 2D) và kiến trúc uGUI Zero-GC chuẩn mực.

---

## 1. Yêu cầu & Kiến trúc Dự án

* **Unity Version**: Unity `6000.5.7f1`
* **Render Pipeline**: Universal Render Pipeline (URP 2D)
* **UI**: uGUI + TextMeshPro (CanvasScaler 1920x1080 Match 0.5)
* **Input**: Unity Input System (ActionsAsset) + Legacy fallback
* **Scene Hierarchy**:
  1. `Assets/Scenes/BootstrapScene.unity` (Build Index 0): Nơi khởi tạo các Persistent Managers (`GameManager`, `AudioManager`, `PoolManager`, `SettingsManager`).
  2. `Assets/Scenes/MainMenuScene.unity` (Build Index 1): Giao diện Menu chính dạng Prefab (`MenuRoot`, `SettingsPanel`, `ControlsPanel`).
  3. `Assets/Scenes/GameplayScene.unity` (Build Index 2): Màn chơi chiến dịch với cơ chế sinh quái tự động `WaveDirector`, HUD Zero-GC, và Boss 3 giai đoạn.

---

## 2. Cách Chạy Game & Thao Tác Điều Khiển

### Chạy trong Unity Editor:
1. Mở Unity Hub và mở thư mục dự án `Star Vanguard/`.
2. Mở Scene khởi đầu `Assets/Scenes/BootstrapScene.unity` (hoặc mở trực tiếp `MainMenuScene` nhờ hỗ trợ của `BootstrapLoader`).
3. Bấm nút **Play** trên thanh công cụ Unity.

### Phím Điều Khiển (Gameplay Controls):
* **Di chuyển**: Phím mũi tên / WASD hoặc Chuột (tuỳ chọn cài đặt)
* **Bắn pháo năng lượng**: Chuột trái / Phím Space
* **Phóng bom Plasma**: Chuột phải / Phím E
* **Đổi vũ khí**: Phím Q / Cuộn chuột
* **Tạm dừng**: Phím Escape

---

## 3. Ba Cổng Kiểm Chứng (Verification Gates)

Dự án duy trì 3 cổng kiểm chứng tự động nghiêm ngặt theo `AGENTS.md`:

```powershell
# Gate G1 — Compile Check
# Đảm bảo 0 lỗi biên dịch trên 4 assembly definitions (Runtime, UI, Editor, Tests)

# Gate G2 — Regression Tests & PlayMode Smoke
& "Unity.exe" -batchmode -quit -projectPath "Star Vanguard" -executeMethod StarVanguard.EditorTools.StarVanguardValidator.ValidateAll
& "Unity.exe" -batchmode -runTests -projectPath "Star Vanguard" -testPlatform EditMode
& "Unity.exe" -batchmode -projectPath "Star Vanguard" -executeMethod StarVanguard.EditorTools.StarVanguardPlayModeSmoke.Run

# Gate G3 — Static Project Audit
python Tools/check_project.py
```

---

## 4. Đóng Gói Bản Build Windows

```powershell
& "Unity.exe" -batchmode -quit -projectPath "Star Vanguard" -executeMethod StarVanguard.EditorTools.StarVanguardBuildPipeline.BuildWindowsRelease
```
Bản build độc lập sẽ được xuất ra tại thư mục `Build/Windows/StarVanguard.exe`.
