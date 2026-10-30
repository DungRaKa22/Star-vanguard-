# 🤖 PHƯƠNG ÁN TÍCH HỢP UNITY "PLUGIN" VÀO ANTIGRAVITY ĐỂ NÂNG CẤP STAR VANGUARD

> **Mục tiêu:** để agent trong Antigravity **đọc được trạng thái thật của Unity Editor** (console, hierarchy, component, scene), **sửa được** scene/prefab/asset, **chạy được** Play Mode & test — thay vì chỉ "viết code rồi bạn tự dán vào Unity".
> **Đối tượng:** dự án `Star Vanguard` — Unity `6000.5.7f1`, URP 2D, không có Mirror, không có asmdef.
> **Tài liệu liên quan:** `docs/AUDIT_2026-10-30.md` (đánh giá), `docs/ANTIGRAVITY_BACKLOG.md` (task cụ thể), `AGENTS.md` (luật cho agent).

---

## 1. TL;DR — chọn phương án nào?

| Phương án | Trạng thái 10/2026 | Khuyến nghị |
| :--- | :--- | :--- |
| **A. MCP for Unity (CoplayDev) v10.0.0** — plugin Unity mã nguồn mở MIT, 48 tool, chạy qua Python/uv, kết nối Antigravity qua HTTP `localhost:8080` | Duy trì tích cực, hỗ trợ Unity 2021.3 → 6.x | ✅ **DÙNG LÀM CHÍNH** |
| **B. Unity AI Assistant + Unity MCP Server chính thức** (`com.unity.ai.assistant`), relay `~/.unity/relay/relay_win.exe` | Unity tự ghi trong docs gói 2.18: *"Unity MCP server is deprecated. Use the Unity CLI instead"* | ⚠️ Chỉ dùng nếu bạn cần tính năng AI của Unity; đừng làm nền tảng lâu dài |
| **C. Unity CLI + Unity Pipeline package** | Còn **experimental** (`docs.unity.com/en-us/unity-cli`: "The features and documentation might change") | 🔭 Theo dõi, thay thế B khi ổn định |

**Kết luận:** dựng **A** làm xương sống (ổn định, miễn phí, nhiều tool, tự cấu hình được cho Antigravity), giữ **B** như phương án dự phòng, và **không** xây quy trình nào phụ thuộc riêng vào B vì nó đã bị đánh dấu deprecated.

> ⚠️ Lưu ý quan trọng: "plugin Unity trong Antigravity" **không phải** một extension cài trong Antigravity. Kiến trúc thực tế là:
> `Antigravity (MCP client)` ⇄ `MCP server (Python, cổng 8080)` ⇄ `Unity Editor (bridge C# trong package)`.
> Antigravity chỉ cần một entry trong `mcp_config.json`, còn phần "plugin" là **một package Unity**.

---

## 2. Kiến trúc & luồng dữ liệu

```
┌──────────────────────────┐        MCP (JSON-RPC)        ┌────────────────────────────┐
│  Antigravity IDE / App   │ ───────────────────────────► │  MCP server (Python + uv)  │
│  Agent + Rules/AGENTS.md │ ◄─────────────────────────── │  HTTP :8080 hoặc stdio     │
└──────────────────────────┘        tool results          └─────────────┬──────────────┘
                                                                        │ TCP/pipe
                                                          ┌─────────────▼──────────────┐
                                                          │  Unity Editor (Bridge C#)  │
                                                          │  MCPForUnity package       │
                                                          └─────────────┬──────────────┘
                                                                        │
                                                          ┌─────────────▼──────────────┐
                                                          │  Assets/, Scenes, Console, │
                                                          │  Play Mode, Test Runner    │
                                                          └────────────────────────────┘
```

**Phân chia việc cho hợp lý (rất quan trọng để không phá project):**

| Việc | Ai làm | Vì sao |
| :--- | :--- | :--- |
| Viết/sửa file `.cs` | **Agent của Antigravity (file tools)**, hoặc `manage_script` | Có diff Git, review được |
| Gắn component, sửa field trong Inspector, tạo prefab | **MCP** (`manage_components`, `manage_prefabs`) | Đúng serialization của Unity, không sửa YAML tay |
| Chạy Play Mode, đọc Console, chụp trạng thái scene | **MCP** (`manage_editor`, `read_console`, `manage_scene`) | Không thể làm bằng cách đọc file |
| Chạy test | **MCP** (`run_tests` + `get_test_job`) | Kết quả thật từ Test Runner |
| Sửa YAML `.unity/.prefab` bằng tay/regex | **KHÔNG AI ĐƯỢC** | Phá GUID/fileID, hỏng scene (xem `AGENTS.md`) |

---

## 3. Phương án A — Cài đặt MCP for Unity + Antigravity (30–45 phút)

### 3.1. Chuẩn bị (một lần)

| Thành phần | Yêu cầu | Kiểm tra |
| :--- | :--- | :--- |
| Unity Editor | 2021.3 LTS → 6.x (dự án: 6000.5.7f1 ✔) | mở được `Star Vanguard` |
| Python | ≥ 3.10 | `python --version` |
| uv (Python toolchain) | bắt buộc | `uv --version` (nếu thiếu: `winget install astral-sh.uv` / `brew install uv`) |
| Git trên PATH | để cài package qua git URL | `git --version` |
| Antigravity | IDE hoặc App bản mới | mở được Agent panel |

### 3.2. Cài package vào Unity

1. Mở project `Star Vanguard` bằng Unity 6000.5.7f1, chờ import xong.
2. `Window → Package Manager` → nút **+** → **Add package from git URL…**
3. Dán (ghim phiên bản để cả nhóm dùng giống nhau):
   ```
   https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0
   ```
   (dùng `#main` nếu muốn bản mới nhất; `openupm add com.coplaydev.unity-mcp` là cách thay thế)
4. Chờ Unity compile. Nếu có lỗi đỏ ở Console → xem mục 9 (Troubleshooting).
5. Menu mới xuất hiện: **`Window → MCP for Unity`**.

### 3.3. Bật server

6. `Window → MCP for Unity` → **Setup / Local Setup Window**:
   * Kiểm tra 2 dấu ✔ xanh cho **Python** và **uv**.
7. Mở **Toggle MCP Window** → **Transport = HTTP** (mặc định), **HTTP URL = `http://localhost:8080`** → bấm **Start Server**.
   * Unity sẽ mở một cửa sổ terminal riêng chạy `uvx … mcp-for-unity --transport http`. **Phải giữ cửa sổ đó mở.**
   * Cảnh báo "HTTP mode" là bình thường (chỉ localhost).

### 3.4. Kết nối Antigravity

Có **2 cách**, chọn 1:

**Cách 1 — để Unity tự ghi config (nhanh nhất)**

8. Trong `Window → MCP for Unity`, mục **Client Configuration**: chọn `Antigravity` trong dropdown → bấm **Configure** (hoặc **Configure All Detected Clients**).
9. Sang Antigravity: icon **`…`** ở panel Agent → **MCP Servers → Manage MCP Servers → Refresh**. Thấy `UnityMCP` (hoặc tên tương ứng) ở trạng thái **Enabled/Connected** là xong.

**Cách 2 — dùng config trong repo (khuyến nghị cho nhóm/BTL, có version control)**

Repo này đã có sẵn file `.agents/mcp_config.json`. Antigravity đọc **workspace config** tại `<workspace>/.agents/mcp_config.json` (và global tại `~/.gemini/config/mcp_config.json`). Do đó:

* Mở Antigravity với **workspace = root repo** `Star-vanguard-` (không mở riêng thư mục `Star Vanguard`).
* `Manage MCP Servers → View raw config` để xem file global; nếu bản Antigravity của bạn **chỉ** đọc global, hãy copy nội dung `.agents/mcp_config.json` vào `~/.gemini/config/mcp_config.json` (Windows: `%USERPROFILE%\.gemini\config\mcp_config.json`; các bản cũ hơn: `%USERPROFILE%\.gemini\antigravity\mcp_config.json`).

> 🔎 **Gotcha đã gặp ở nhiều người:** Antigravity **không** đặt working directory cho MCP server vào thư mục project (với server stdio, CWD có thể là thư mục cài Antigravity). Vì vậy **luôn dùng đường dẫn tuyệt đối** trong config và ưu tiên transport HTTP (`serverUrl`) để tránh phụ thuộc CWD.

### 3.5. Cấu hình mẫu (chọn 1 biến thể)

**Biến thể 1 — HTTP (khớp `.agents/mcp_config.json` trong repo, khuyến nghị):**

```json
{
  "mcpServers": {
    "UnityMCP": {
      "serverUrl": "http://localhost:8080/mcp",
      "timeoutSeconds": 300
    }
  }
}
```

**Biến thể 2 — stdio qua `uvx` (không cần bấm Start Server, nhưng phải có `uvx` trong PATH):**

```json
{
  "mcpServers": {
    "UnityMCP": {
      "command": "uvx",
      "args": ["--quiet", "--from", "mcpforunityserver", "mcp-for-unity", "--transport", "stdio"],
      "timeoutSeconds": 300
    }
  }
}
```

**Biến thể 3 — Unity MCP Server chính thức (phương án B, relay):**

```json
{
  "mcpServers": {
    "unity-mcp": {
      "command": "C:\\Users\\<TÊN_BẠN>\\.unity\\relay\\relay_win.exe",
      "args": ["--mcp", "--project-path", "D:\\...\\Star-vanguard-\\Star Vanguard"]
    }
  }
}
```
(macOS: `~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64`; Linux: `~/.unity/relay/relay_linux` — luôn có cờ `--mcp`)
Khi dùng biến thể 3: trong Unity bật `Edit → Project Settings → AI → Unity MCP Server` (Bridge = Running), và **lần đầu kết nối phải bấm Allow** ở mục *Pending Connections*.

> Giới hạn thực tế: Antigravity khuyến nghị **< 50 tool** bật cùng lúc. MCP for Unity có 48 tool chia nhóm → chỉ bật nhóm cần (`core`, `testing`, `profiling`, `ui`) và tắt `asset_gen`, `probuilder`, `animation` nếu không dùng.

### 3.6. Kiểm tra kết nối (smoke test)

Dán vào Agent của Antigravity:

```
Đọc Unity Console qua MCP, liệt kê mọi Error và Warning trong 5 phút gần nhất.
Sau đó cho tôi biết: scene đang mở tên gì, có bao nhiêu GameObject root,
và phiên bản Unity Editor đang chạy.
```

**Đạt khi:** agent trả về đúng tên scene (`BootstrapScene`/`MainMenuScene`/`GameplayScene`), đúng số root object, đúng `6000.5.7f1` — nghĩa là đã có "mắt" vào Editor.

---

## 4. Bản đồ tool MCP → công việc thực tế của Star Vanguard

| Tool (nhóm) | Dùng để | Task trong backlog |
| :--- | :--- | :--- |
| `read_console` | đọc/lọc/xoá Console (Error/Warning/Log) | Mọi task — bước Verify |
| `manage_scene` | mở/lưu/tạo scene, đọc hierarchy | T-201, T-401 |
| `manage_gameobject` | tạo/xoá/đổi tên/sắp xếp object | T-201, T-402 |
| `manage_components` | add/remove/set property component (đúng serialization) | T-102, T-201, T-301 |
| `manage_prefabs` | tạo/sửa prefab, apply/revert override | T-401 (prefab hoá UI), T-403 |
| `manage_asset` | import, move, delete, đổi import settings (texture/sprite) | T-404 (dọn template), T-501 |
| `create_script` / `manage_script` / `script_apply_edits` / `apply_text_edits` | tạo & sửa C# **theo cấu trúc** (an toàn hơn sửa text) | T-101, T-103, T-301 |
| `validate_script` | kiểm tra cú pháp/biên dịch trước khi chạy | Mọi task sửa code |
| `refresh_unity` | ép AssetDatabase refresh + compile | Sau mỗi lần ghi file |
| `find_gameobjects` / `find_in_file` | định vị object/tham chiếu nhanh | T-201, T-402 |
| `run_tests` + `get_test_job` | chạy EditMode/PlayMode test, trả job_id async | T-101, T-102, mọi sprint |
| `execute_menu_item` | bấm menu item có sẵn của dự án (validator, baker, splitter) | T-001, T-101 |
| `manage_editor` | play/pause/step, đọc trạng thái editor | T-102, T-202 |
| `manage_profiler` | lấy counter, memory snapshot, frame debugger | T-501 (perf pass) |
| `manage_packages` | thêm/xoá package (vd. Mirror, Cinemachine) | T-104 (Mirror) |
| `manage_physics` | collision matrix, 2D physics settings | T-303 |
| `manage_graphics` | URP renderer, post-processing, quality | T-502 |
| `manage_camera` | camera & Cinemachine | T-402 |
| `manage_ui` | UI Toolkit (UXML/USS) — chỉ dùng nếu chuyển sang UI Toolkit | (không ưu tiên) |
| `unity_docs` / `unity_reflect` | tra API Unity 6 chính xác (chống dùng API deprecated) | Mọi task |
| `batch_execute` | gộp nhiều lệnh trong 1 lượt (nhanh hơn nhiều) | T-401, T-402 |
| `manage_build` | build player, đổi platform, build profile | T-601 |
| `execute_code` (nhóm `scripting_ext`) | chạy C# tuỳ ý trong Editor — **rất mạnh, rất nguy hiểm** | Chỉ dùng khi được phép rõ ràng |

---

## 5. Vòng lặp làm việc chuẩn (6 bước) — dùng cho mọi task

```
1. RECON   → read_console + manage_scene(get hierarchy) + đọc file liên quan
2. PLAN    → agent viết kế hoạch 3–6 bước, nêu file/prefab/scene sẽ chạm, xin xác nhận nếu > 5 file
3. PATCH   → sửa .cs bằng edit tool của Antigravity (có diff Git) hoặc manage_script;
             sửa Inspector/prefab qua manage_components/manage_prefabs (KHÔNG sửa YAML tay)
4. VERIFY  → refresh_unity → read_console (0 error) → run_tests/get_test_job
             → execute_menu_item StarVanguardValidator.ValidateAll
             → python3 Tools/check_project.py (0 FAIL mới)
5. EYES    → manage_editor(play) → quan sát/chụp scene → manage_editor(stop)
             (task UI/HUD bắt buộc có bước này)
6. COMMIT  → git add -A && git commit -m "T-xxx: <mô tả>" ; cập nhật tài liệu + backlog
```

**Ba cổng chặn (gate) không được bỏ qua:**

| Gate | Lệnh / tool | Điều kiện qua |
| :--- | :--- | :--- |
| G1 Biên dịch | `refresh_unity` + `read_console` | 0 error, không có warning mới |
| G2 Hồi quy | `run_tests` (PlayMode + EditMode) + validator batchmode | 100% pass |
| G3 Tĩnh | `python3 Tools/check_project.py` | 0 FAIL, số WARN không tăng |

> Sau mỗi lần AI sửa scene/prefab, **kiểm tra `git diff --stat`**: nếu diff scene tăng vài nghìn dòng mà chỉ để đổi 1 field → agent đã làm sai cách, phải revert và dùng `manage_components`.

---

## 6. Prompt mẫu (copy-paste) cho các task lớn của dự án

> Mẹo: luôn mở đầu bằng ngữ cảnh dự án + gate, kết thúc bằng yêu cầu báo cáo bằng chứng.

### P-00 — Prompt "khung" cho mọi phiên

```
Bạn là Unity engineer cho dự án Star Vanguard (Unity 6000.5.7f1, URP 2D, uGUI/TMP).
Đọc AGENTS.md và docs/AUDIT_2026-10-30.md trước khi làm bất cứ điều gì.

Luật cứng:
- Không sửa trực tiếp YAML của .unity/.prefab/.asset.
- Không xoá/đổi tên file .meta; nếu đổi tên file .cs phải đổi cả .meta trong cùng 1 commit.
- Đổi tên field [SerializeField] phải thêm [FormerlySerializedAs].
- Sau mỗi thay đổi: refresh_unity → read_console (0 error) → python3 Tools/check_project.py (0 FAIL).

Task hôm nay: <mô tả>.
Trước khi code, hãy: (1) đọc console, (2) liệt kê file/prefab sẽ chạm, (3) nêu rủi ro. Chờ tôi duyệt nếu chạm > 5 file.
```

### P-01 — Dựng lưới an toàn (T-101, làm ĐẦU TIÊN)

```
Tạo kiến trúc assembly cho project:
- Assets/Scripts/StarVanguard.Runtime.asmdef (runtime, refs: Unity.InputSystem, Unity.TextMeshPro, Unity.RenderPipelines.Universal.Runtime)
- Assets/Scripts/UI/StarVanguard.UI.asmdef (refs Runtime + uGUI + TMP, autoReferenced false)
- Assets/Editor/StarVanguard.Editor.asmdef (Editor only, refs Runtime + UI)
- Assets/Tests/EditMode/StarVanguard.Tests.EditMode.asmdef (refs Runtime, testAssemblies true)
- Assets/Tests/PlayMode/... tương tự
Sau đó chuyển StarVanguardPlayModeSmoke thành NUnit [UnityTest] trong Tests/PlayMode.
Điều kiện xong: Test Runner chạy được, tất cả test xanh, read_console không có error.
Báo cáo: ảnh chụp kết quả Test Runner + số test.
```

### P-02 — Sửa HUD trùng (P0-2)

```
Đang có 2 hệ HUD chồng nhau trong GameplayScene: HUD uGUI (UIManager.m_ScoreText/m_LivesContainer/
m_BombsContainer đã gán) và HUD vẽ bằng OnGUI trong OverheatGaugeUI (m_EnableProceduralHUD = true).

Yêu cầu:
1. Chuyển OverheatGaugeUI sang uGUI thuần: heat bar = Slider/Image fill, text = TMP, dùng UITheme.
2. Xoá hoàn toàn OnGUI và các Texture2D tạo runtime; hiển thị điểm/mạng/bom chỉ do UIManager lo.
3. Bỏ AddComponent ngầm trong UIManager.Start; gán component sẵn trong scene.
4. Không dùng emoji trong text TMP (font không có glyph) — thay bằng icon sprite hoặc ký tự ASCII.
Điều kiện xong: read_console 0 error; chơi thử 1 wave, HUD chỉ có 1 lớp, không còn chữ trùng;
Profiler: không còn allocation GC mỗi frame ở OnGUI.
```

### P-03 — Input System (P1-1)

```
Migrate toàn bộ input gameplay sang Input System 1.20 dùng Assets/InputSystem_Actions.inputactions:
- Tạo lớp InputReader (ScriptableObject hoặc singleton) expose: Move (Vector2), Fire (button, hold),
  Bomb (button), SwitchWeapon (1..5 / scroll), Pause (Esc) — hỗ trợ cả Keyboard&Mouse và Gamepad.
- Thay 15 chỗ dùng Input.GetKey* trong GameManager, PlayerController, PlayerShooting, PlayerBombing.
- Giữ hành vi y hệt hiện tại (test bằng cách chơi 2 wave).
- activeInputHandler giữ "Both" trong lúc chuyển, sau khi xong đổi sang "Input System Package (New)".
Điều kiện xong: không còn Input.GetKey* trong Assets/Scripts; play mode 2 wave không lỗi; gamepad hoạt động.
```

### P-04 — Co-op (cần quyết định A hay B)

```
Cài Mirror qua manage_packages (com.unity.mirror), rồi:
- Thêm versionDefines "com.unity.mirror" → MIRROR trong StarVanguard.Runtime.asmdef (hoặc bỏ hẳn #if MIRROR).
- Tạo NetworkManager prefab trong BootstrapScene, gắn StarVanguardNetworkManager.
- Gắn NetworkPlayerController vào Player_Prefab, đăng ký spawnable prefab.
- Kiểm chứng: build 2 instance (host + client) trên cùng máy, log ra 2 player, boss scale HP x2.
Điều kiện xong: 2 cửa sổ game kết nối, di chuyển/bắn đồng bộ, không có lỗi Mirror trong console.
Nếu không đủ thời gian: dừng lại, báo cáo, và thực hiện phương án B (xoá Networking + nút Co-op).
```

### P-05 — Prefab hoá UI / giảm kích thước scene (P1-9)

```
MainMenuScene đang 12.809 dòng YAML và MenuRoot được dựng bằng code (MainMenuScreen.cs 649 dòng).
Kế hoạch: convert các panel (Home/Settings/Controls/Coop) thành prefab riêng trong Assets/Prefabs/UI_Prefabs/,
MainMenuScreen chỉ còn nhiệm vụ bind sự kiện + điều hướng qua UIPanelRouter.
Điều kiện xong: scene < 3.000 dòng, prefab hoá đủ 4 panel, UI hiển thị y hệt trước (so sánh screenshot trước/sau),
git diff không chứa thay đổi GUID/fileID bất thường.
```

### P-06 — Perf pass có số liệu (P1-5, P1-6, P1-8)

```
Dùng manage_profiler: đo 60 giây gameplay ở wave 5 (nhiều địch + bom).
Ghi lại: FPS trung bình, GC alloc/frame, thời gian Bomb.Explode, số lần FindObjectsByType/frame.
Sau đó tối ưu:
- Thay FindObjectsByType trong Bomb/PoolManager bằng registry (IEnemyRegistry / ActiveRegistry).
- Bỏ new float[] trong PlayerShooting (dùng static readonly mảng).
- Đặt Application.targetFrameRate = 120 + vSync theo setting.
Điều kiện xong: báo cáo before/after bằng số (FPS, GC/frame, ms), không tăng lỗi.
```

### P-07 — Nghiệm thu & đóng gói

```
Chạy: run_tests (EditMode + PlayMode) → execute_menu_item StarVanguardValidator.ValidateAll
→ python3 Tools/check_project.py --json docs/audit-run.json → build Windows qua manage_build.
Tổng hợp báo cáo: số test pass, số FAIL/WARN của check_project, kích thước build,
log console còn warning gì. Cập nhật docs/AUDIT_2026-10-30.md mục "Trạng thái sau sprint".
```

---

## 7. Guardrails — bổ sung vào `AGENTS.md` (đã tạo sẵn ở root repo)

Các luật **bắt buộc** (đã có trong `AGENTS.md`, tóm tắt ở đây):

1. **Cấm** sửa YAML của `.unity/.prefab/.asset` bằng tay/regex — chỉ qua MCP hoặc Editor script.
2. **Cấm** xoá/tự sinh `.meta`; đổi tên file `.cs` phải đổi tên `.meta` cùng lúc.
3. Đổi tên field serialize phải có `[FormerlySerializedAs("cũ")]`.
4. Mọi thay đổi phải qua 3 gate (compile / test / static check) trước khi commit.
5. Task chạm > 5 file hoặc xoá asset → **phải xin duyệt**.
6. Không bật `execute_code` (chạy C# tuỳ ý) cho agent tự do; nếu cần, chỉ chạy script có sẵn trong `Assets/Editor/`.
7. Commit nhỏ, message theo `T-xxx: mô tả`, mỗi commit 1 mục đích.
8. Agent không được tự ý đổi `ProjectSettings` (resolution, quality, input handler) mà không nêu rõ.

**Về an toàn (theo khuyến cáo của cộng đồng Antigravity):** Antigravity + MCP có quyền đọc/ghi file và chạy lệnh trong máy. Hãy:
* bật chế độ **yêu cầu xác nhận** trước khi chạy shell command;
* chỉ mở workspace là repo này, không mở ổ đĩa;
* làm việc trên **branch riêng** (`arena/...` hoặc `feat/...`), không commit thẳng `main`;
* backup/commit trước khi cho agent sửa scene lớn.

---

## 8. Rủi ro & cách giảm thiểu

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
| :--- | :-: | :-: | :--- |
| Agent sửa YAML scene → hỏng prefab/GUID | Trung bình | Cao | Luật #1 + review `git diff --stat` + chơi thử sau mỗi task |
| MCP server không kết nối (uv/Python/port) | Cao lần đầu | Thấp | Checklist mục 3.1; thử biến thể HTTP trước, stdio sau |
| Antigravity đọc config global thay vì workspace | Trung bình | Thấp | Copy `.agents/mcp_config.json` sang `~/.gemini/config/mcp_config.json` |
| Quá nhiều tool vượt 50 → agent chọn nhầm tool | Trung bình | Trung bình | Tắt nhóm không dùng (`asset_gen`, `probuilder`, `animation`) |
| Agent "sửa cho xanh" bằng cách xoá test | Thấp | Cao | Review diff phần `Assets/Tests/`; cấm xoá test trong `AGENTS.md` |
| Scene 12k dòng → mỗi lần lưu sinh diff khổng lồ | Cao | Trung bình | Prefab hoá UI (T-401); commit scene riêng, không trộn với code |
| Context bị đầy do file lớn | Trung bình | Thấp | Chia task nhỏ; dùng `find_in_file` thay vì đọc cả file; `batch_execute` |
| Mất đồng bộ khi có 2 Unity instance mở | Thấp | Trung bình | Dùng `set_active_instance` / `--project-path` chỉ đúng project |

---

## 9. Troubleshooting nhanh

| Triệu chứng | Nguyên nhân thường gặp | Cách xử lý |
| :--- | :--- | :--- |
| Unity có menu MCP nhưng Antigravity không thấy tool | chưa Start Server / chưa Refresh trong Manage MCP Servers | Start Local HTTP Server → Refresh; kiểm tra `curl http://localhost:8080/mcp` |
| `uv not found` | chưa cài uv hoặc chưa vào PATH | cài uv → **restart Unity** (Unity cache biến môi trường lúc khởi động) |
| Server chạy nhưng tool trả "not connected to Unity" | Unity Bridge chưa chạy / project khác | `Window → MCP for Unity` xem trạng thái Bridge; dùng `set_active_instance` |
| Package cài từ git lỗi đỏ | thiếu Git trên PATH / proxy chặn | cài Git, đặt `HTTP_PROXY`/`HTTPS_PROXY`; hoặc dùng OpenUPM |
| Agent sửa xong nhưng Unity không đổi | chưa `refresh_unity` / Editor đang ở Play Mode | refresh + thoát Play Mode trước khi sửa scene |
| Tool chạy nhưng ghi sai thư mục | CWD của Antigravity ≠ project | dùng đường dẫn tuyệt đối trong prompt/config |

---

## 10. Lộ trình 4 tuần dùng MCP để hoàn thiện dự án

| Tuần | Mục tiêu | Task (xem backlog) | Cổng nghiệm thu |
| :-: | :--- | :--- | :--- |
| **0** (0.5 ngày) | Cài đặt & chứng minh kết nối | Setup A + smoke test P-00 | Agent đọc được Console & scene thật |
| **1** | Lưới an toàn trước khi cho AI sửa | T-101 asmdef + NUnit; T-001 CI; T-002 README | Test Runner xanh; CI chạy `check_project.py` |
| **2** | Sửa P0 | T-101→T-103 (HUD), T-104 (quyết định Co-op) | 0 FAIL; HUD 1 lớp; 0 error console |
| **3** | Sửa P1 | T-301 Input System, T-302 AudioMixer, T-303 event/pool, T-304 FPS cap | 3 gate xanh + chơi thử 5 wave |
| **4** | Hoàn thiện & đóng gói | T-401 prefab hoá UI, T-402 dọn dead code/docs, T-501 perf pass, T-601 build | Build Windows chạy được, báo cáo before/after |

Chi tiết từng task (file đích, tool, DoD, ước lượng): **`docs/ANTIGRAVITY_BACKLOG.md`**.

---

## 11. Checklist "đã tích hợp đúng" (dán vào báo cáo BTL)

- [ ] `Window → MCP for Unity` hiện ✔ cho Python và uv
- [ ] HTTP server đang chạy ở `localhost:8080/mcp`
- [ ] Antigravity `Manage MCP Servers` thấy server và **Enabled**
- [ ] Smoke test trả đúng tên scene + phiên bản Unity `6000.5.7f1`
- [ ] `read_console` trả được log; `refresh_unity` compile thành công
- [ ] `run_tests` chạy được ít nhất 1 test và trả `job_id` + kết quả
- [ ] `execute_menu_item` chạy được `StarVanguardValidator.ValidateAll`
- [ ] `.agents/mcp_config.json` đã commit; `AGENTS.md` đã commit
- [ ] `python3 Tools/check_project.py` chạy được và trả về 0 FAIL (sau tuần 2)
- [ ] Có ít nhất 1 commit được tạo bởi quy trình MCP, có log/gate kèm theo

---

## 12. Nguồn tham chiếu

* Antigravity — tài liệu MCP chính thức (config global `~/.gemini/config/mcp_config.json`, workspace `.agents/mcp_config.json`, `serverUrl` cho HTTP): <https://antigravity.google/docs/mcp>
* MCP for Unity (CoplayDev) — repo & quickstart (git URL `#v10.0.0`, yêu cầu uv, 48 tool, nhóm `core/testing/profiling/ui`): <https://github.com/CoplayDev/unity-mcp>
* MCP for Unity — danh mục tool (`read_console`, `manage_scene`, `manage_components`, `manage_prefabs`, `run_tests`, `manage_profiler`…): <https://coplaydev.github.io/unity-mcp/reference/tools/>
* Unity AI Assistant — MCP server (**ghi chú deprecated → dùng Unity CLI**): <https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.18/manual/integration/unity-mcp-get-started.html>
* Unity CLI (experimental, cần Unity Pipeline package): <https://docs.unity.com/en-us/unity-cli>
* Google/Android Developers — hướng dẫn Antigravity × Unity MCP (relay `relay_win.exe`, cấp quyền Pending Connections): <https://developer.android.com/games/engines/unity/unity-mcp-antigravity>
