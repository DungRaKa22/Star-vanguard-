# AGENTS.md — Luật làm việc cho AI agent trên Star Vanguard

> File này được Antigravity (và các agent khác) đọc tự động ở root workspace.
> Đọc kèm: `docs/AUDIT_2026-10-30.md` (đánh giá), `docs/ANTIGRAVITY_MCP_PLAYBOOK.md` (cách dùng MCP),
> `docs/ANTIGRAVITY_BACKLOG.md` (task), `unity_6_ai_agent_rules_guardrails.md` (luật Unity chi tiết).

## 0. Bối cảnh dự án

* Unity **6000.5.7f1**, URP 2D, uGUI + TextMeshPro, Input System 1.20 (đã cài, **chưa dùng** trong gameplay).
* Project Unity nằm ở thư mục con: `Star Vanguard/` (có dấu cách — luôn quote đường dẫn).
* Script: `Star Vanguard/Assets/Scripts/` (runtime), `Assets/Editor/` (tool), `Assets/Tests/` (sẽ tạo).
* Scene: `BootstrapScene` (index 0, nơi ở duy nhất của persistent managers) → `MainMenuScene` → `GameplayScene`.
* Networking (Mirror) đang **không hoạt động** — xem P0-1 trong báo cáo audit. Đừng giả định co-op chạy được.

## 1. Luật cứng (vi phạm = revert ngay)

1. **KHÔNG** sửa trực tiếp YAML của `.unity`, `.prefab`, `.asset` bằng tay hoặc regex.
   Dùng MCP (`manage_scene`, `manage_gameobject`, `manage_components`, `manage_prefabs`) hoặc Editor script.
2. **KHÔNG** xoá, tạo tay, hay copy `.meta`. Đổi tên `X.cs` ⇒ đổi tên `X.cs.meta` **trong cùng một commit**.
3. Đổi tên field `[SerializeField]` ⇒ **bắt buộc** thêm `[FormerlySerializedAs("tênCũ")]` (`using UnityEngine.Serialization`).
4. Tên file `.cs` phải trùng tên class `MonoBehaviour`/`ScriptableObject` (Unity yêu cầu).
5. Không `Instantiate()`/`Destroy()` trong gameplay cho đạn/địch/pickup/VFX/icon — dùng `PoolManager` + `IPoolable`.
6. Không xoá hoặc "làm yếu" test để build xanh. Nếu test sai, sửa code hoặc sửa test **có giải thích trong commit**.
7. Không đổi `ProjectSettings` (độ phân giải, quality, active input handler, layer/tag) mà không nêu rõ lý do và xin duyệt.
8. Task chạm **> 5 file**, xoá asset, hoặc đổi cấu trúc scene ⇒ **dừng lại, trình kế hoạch, xin duyệt trước**.

## 2. Ba cổng kiểm chứng (bắt buộc sau mỗi thay đổi)

| Gate | Cách chạy | Điều kiện qua |
| :--- | :--- | :--- |
| **G1 — Compile** | `refresh_unity` rồi `read_console` (lọc Error) | 0 error, không có warning mới |
| **G2 — Hồi quy** | `run_tests` (EditMode + PlayMode) và/hoặc `execute_menu_item StarVanguard.EditorTools.StarVanguardValidator.ValidateAll` | 100% pass |
| **G3 — Tĩnh** | `python3 Tools/check_project.py` | 0 FAIL; số WARN không tăng |

Nếu không chạy được MCP, phải nói rõ "chưa kiểm chứng" trong báo cáo — **không được** tuyên bố "đã xong" khi chưa qua gate.

## 3. Quy trình chuẩn

```
RECON  → read_console + manage_scene(hierarchy) + đọc file liên quan
PLAN   → 3–6 bước, liệt kê file/prefab/scene sẽ chạm, nêu rủi ro
PATCH  → sửa .cs bằng edit tool (có diff Git) / sửa Inspector qua MCP
VERIFY → G1 → G2 → G3
EYES   → manage_editor(play) + kiểm tra hình ảnh (bắt buộc với task UI/HUD)
COMMIT → "T-xxx: mô tả ngắn" (1 mục đích / commit), cập nhật docs/backlog
```

## 4. Quy ước mã nguồn hiện có (giữ nguyên, đừng "hiện đại hoá" tuỳ tiện)

* Field private: `m_PascalCase`; tham số: `i_Name`; hằng: `k_Name`; static: `s_Name`.
* Singleton: `Singleton<T>` + `[SceneBoundSingleton]` cho object thuộc scene (UI, WaveDirector, Parallax, Player).
  **Không** tự thêm `DontDestroyOnLoad` ngoài cơ chế này.
* Event: `public static event Action<...>` có cặp `OnXxxChanged`; nơi subscribe **phải** unsubscribe trong `OnDestroy` (không đặt điều kiện `if (Manager.HasInstance)`).
* So sánh `UnityEngine.Object`: dùng `== null` / `!= null`, **không** dùng `?.` hay `??` (Unity fake-null).
* Namespace: code mới đặt trong `StarVanguard.*` (Runtime) hoặc `StarVanguard.UI`; không thêm class mới vào global namespace.
* API tìm object: dùng `FindAnyObjectByType<T>()` / `FindObjectsByType<T>()` (dạng **không** tham số — hợp lệ từ Unity 6.4). Không dùng `FindObjectOfType` (deprecated).

## 5. Những chỗ đã biết là nợ kỹ thuật (đừng nhân bản thêm)

| Vị trí | Vấn đề | Task |
| :--- | :--- | :--- |
| `UIManager.cs:51,57` | tự `AddComponent` component UI lúc runtime | T-201 |
| `OverheatGaugeUI.cs` | HUD vẽ bằng `OnGUI`, GC mỗi frame, emoji không có glyph | T-201 |
| `Assets/Scripts/Networking/*` | nằm trong `#if MIRROR` nhưng không có Mirror, không được gắn vào scene | T-202/T-203 |
| `AudioManager.cs:85-92` | 1 AudioSource + đổi pitch toàn cục | T-302 |
| `Bomb.cs:47-59`, `PoolManager.cs:217-278` | `FindObjectsByType` trong luồng gameplay | T-305 |
| `PlayerShooting.cs:202` | `new float[]` mỗi phát Plasma | T-305 |
| `GameManager.cs:53-56, 97-100` | ép `SetResolution` 2 lần, bỏ qua setting người chơi | T-304 |
| `MainMenuScene.unity` (12.8k dòng) | UI dựng bằng code, scene phình to | T-401 |
| `HomeScreenController.cs`, `ControlsPanelController.cs` | dead code | T-402 |

## 6. An toàn & Git

* Làm trên branch riêng; không commit thẳng `main`.
* Trước khi cho agent sửa scene lớn: `git commit` trạng thái hiện tại để có điểm revert.
* Sau khi sửa: `git diff --stat`; nếu scene/prefab tăng hàng nghìn dòng cho một thay đổi nhỏ ⇒ sai cách, revert.
* Không commit `Library/`, `Temp/`, `Build/`, `*.csproj`, `*.sln` (đã có trong `.gitignore`).
* Không chạy `execute_code` (C# tuỳ ý trong Editor) trừ khi người dùng cho phép rõ ràng.

## 7. Định nghĩa "xong"

Task chỉ được đánh dấu hoàn thành khi báo cáo có đủ: `git diff --stat`, log console sau sửa, kết quả test/validator, output `check_project.py`, và ảnh chụp Play Mode với task UI.
