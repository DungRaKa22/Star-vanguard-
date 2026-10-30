# 🗂️ BACKLOG THI CÔNG — STAR VANGUARD (dùng với Antigravity + Unity MCP)

> Sinh ra từ `docs/AUDIT_2026-10-30.md`. Mỗi task đủ nhỏ để một phiên agent hoàn tất và tự nghiệm thu.
> **Cột "Gate"** = cổng kiểm chứng bắt buộc trước khi đánh dấu xong:
> `G1` compile sạch (`refresh_unity` + `read_console`), `G2` test/validator xanh, `G3` `python3 Tools/check_project.py` không tăng FAIL/WARN.

**Ước lượng:** 1d ≈ 1 ngày làm việc của 1 người. **Ưu tiên:** P0 chặn release → P2 polish.

---

## 1. Bảng tổng hợp

| ID | Ưu tiên | Task | File / Asset đích | Tool MCP chính | Gate | Ước lượng |
| :--- | :-: | :--- | :--- | :--- | :-: | :-: |
| **T-003** | ✅ xong | Thêm `AGENTS.md` + `.agents/mcp_config.json` + `Tools/check_project.py` (đã làm trong phiên này) | root | — | G3 | 0.5d |
| **T-001** | P0 | Đưa `check_project.py` + `StarVanguardValidator` vào CI (GitHub Actions), artifact báo cáo | `.github/workflows/` | — | G3 | 0.5d |
| **T-002** | P1 | README root + gộp tài liệu trùng + LICENSE + xoá link tuyệt đối trong `memory/` | `README.md`, `docs/` | — | — | 0.5d |
| **T-101** | P0 | Tạo 5 assembly definition (Runtime/UI/Editor/Net/Tests) + sửa mọi `using` | `Assets/**/*.asmdef` | `create_script`, `refresh_unity` | G1 | 1d |
| **T-102** | P0 | Chuyển `StarVanguardPlayModeSmoke` thành NUnit test thật (EditMode + PlayMode) | `Assets/Tests/**` | `run_tests`, `get_test_job` | G1+G2 | 1.5d |
| **T-103** | ✅ xong | ADR-001: chọn phương án **B — cắt bỏ Co-op LAN** | `docs/ADR-001-coop.md` | — | — | 0.2d |
| **T-201** | P0 | Hợp nhất HUD: bỏ `OnGUI`, chuyển OverheatGauge sang uGUI/TMP, bỏ `AddComponent` ngầm, bỏ emoji | `OverheatGaugeUI.cs`, `UIManager.cs`, `GameplayScene` | `manage_components`, `manage_scene`, `manage_editor` | G1+G2+G3 | 1.5d |
| **T-202** | ✅ xong (code) | (Phương án B) Xoá `Assets/Scripts/Networking/` + `HomeScreenController`, bỏ `UIPanelId.Coop`, bỏ panel/nút Co-op, bỏ `m_NetworkLobbyUI`, bỏ `ScaleForCoop` call | Networking, `MainMenuScreen.cs`, `UIManager.cs`, `WaveDirector.cs` | edit tool | G3 ✅ / G1+G2 chờ Unity | 0.5d |
| **T-204** | P0 | ⚠️ **Còn lại của T-202:** mở Unity chạy `Tools ▸ Star Vanguard ▸ 1b. Remove Legacy Co-op UI from MainMenuScene and Save` rồi Play thử menu | `MainMenuScene.unity` | `execute_menu_item` | G1+G2+G3 | 0.1d |
| **T-203** | P0 | (Nếu chọn A) Cài Mirror + `versionDefines MIRROR` + gắn NetworkManager/Player prefab + test 2 instance | `Packages/manifest.json`, asmdef, prefabs | `manage_packages`, `manage_prefabs` | G1+G2 | 3–5d |
| **T-301** | P1 | Migrate gameplay input sang Input System (InputReader + gamepad + rebinding) | 5 script, `.inputactions` | `script_apply_edits`, `validate_script` | G1+G2 | 2d |
| **T-302** | P1 | Audio: AudioMixer (Master/Music/SFX) + pool 8–12 AudioSource, bỏ đổi pitch toàn cục | `AudioManager.cs`, `Assets/Audio/Mixer.mixer` | `manage_asset`, `manage_components` | G1+G2 | 1.5d |
| **T-303** | P1 | Vòng đời event + fake-null: bỏ `?.`/`??` trên `UnityEngine.Object`, unsubscribe không điều kiện | `AudioManager`, `UIManager`, `PlayerShooting`, khác | edit tool | G1+G2 | 0.5d |
| **T-304** | P1 | Chính sách FPS/độ phân giải: `targetFrameRate`, vSync theo setting, bỏ ép `SetResolution` 2 lần | `GameManager.cs`, `QualitySettings` | `manage_graphics` | G1 | 0.5d |
| **T-305** | P1 | Bỏ cấp phát trong đường đạn: mảng tĩnh cho Plasma, registry thay `FindObjectsByType` trong `Bomb`/`PoolManager` | `PlayerShooting.cs`, `Bomb.cs`, `PoolManager.cs` | edit tool, `manage_profiler` | G1+G2 | 1d |
| **T-306** | P2 | Save/Leaderboard: JSON có `schemaVersion` + migration, tách khỏi PlayerPrefs thô | `LeaderboardManager.cs`, `SettingsManager.cs` | edit tool | G2 | 1d |
| **T-401** | P1 | Prefab hoá UI (Home/Settings/Controls/Coop) → scene < 3.000 dòng | `MainMenuScene`, `UI_Prefabs/*` | `manage_prefabs`, `batch_execute` | G1+G2+so sánh ảnh | 2–3d |
| **T-402** | P1 | Dọn rác: `HomeScreenController`, `ControlsPanelController`, `URP2DSceneTemplate.unity`, `m_RaycastTarget` thừa | nhiều | `manage_asset` | G3 | 0.5d |
| **T-403** | P2 | Thống nhất namespace + đặt tên (`StarVanguard.*`), bổ sung `[FormerlySerializedAs]` khi đổi tên | toàn bộ script | edit tool | G1 | 1.5d |
| **T-404** | P2 | Pipeline asset: `AssetPostprocessor` cho sprite (PPU/filter/9-slice) + gắn `generate_scifi_sprites.py` vào `Tools/` | `Assets/Editor/`, `Tools/` | `manage_asset` | G3 | 1d |
| **T-501** | P1 | Perf pass có số: profile 60s wave 5, báo cáo before/after (FPS, GC/frame, ms) | toàn bộ | `manage_profiler` | G1+G2 | 1d |
| **T-502** | P2 | Game feel: camera shake, hit-stop, hit flash, URP bloom cho đạn | prefabs, URP volume | `manage_graphics`, `manage_camera` | G1+ảnh | 1.5d |
| **T-503** | P2 | Việt hoá UI + font TMP có glyph tiếng Việt (kiểm tra dấu) | `Assets/TextMesh Pro`, UI prefabs | `manage_asset` | ảnh chụp | 1d |
| **T-601** | P1 | Build Windows + đóng gói zip + ghi log build vào `docs/` | `Build/` (git-ignored) | `manage_build` | G2 | 0.5d |

**Tổng ước lượng:** ~20–26 ngày người; nếu dùng agent MCP song song theo nhánh (code / scene / test) có thể rút còn 8–12 ngày lịch.

---

## 2. Chi tiết các task P0

### T-101 — Assembly Definitions (làm TRƯỚC mọi thứ)

**Vì sao trước:** khi chưa có asmdef, mọi lần agent sửa 1 file là Unity biên dịch lại toàn bộ 69 script; test không chạy được; Editor tool lẫn vào build. Đây là điều kiện tiên quyết để có G1/G2 nhanh và ổn định.

```
Assets/Scripts/StarVanguard.Runtime.asmdef
  name: StarVanguard.Runtime
  references: Unity.InputSystem, Unity.TextMeshPro, Unity.RenderPipelines.Universal.Runtime
  autoReferenced: true

Assets/Scripts/UI/StarVanguard.UI.asmdef
  name: StarVanguard.UI
  references: StarVanguard.Runtime, UnityEngine.UI, Unity.TextMeshPro
  autoReferenced: false        <-- bắt buộc phải có ref tường minh, tránh phụ thuộc ngầm

Assets/Editor/StarVanguard.Editor.asmdef
  name: StarVanguard.Editor
  includePlatforms: [Editor]
  references: StarVanguard.Runtime, StarVanguard.UI

Assets/Tests/EditMode/StarVanguard.Tests.EditMode.asmdef   (testAssemblies: true)
Assets/Tests/PlayMode/StarVanguard.Tests.PlayMode.asmdef   (testAssemblies: true)
```

**DoD:** Unity compile 0 error; `Test Runner` mở được và liệt kê assembly test; `check_project.py` không còn WARN `asmdef`.

> ⚠️ Lưu ý cho agent: khi thêm asmdef, script nào thiếu `using` sẽ đỏ hàng loạt. Hãy làm theo từng bước: tạo asmdef Runtime → compile → xử lý lỗi → mới tạo UI. Đừng tạo cả 5 cái cùng lúc.

---

### T-102 — Chuyển smoke test thành NUnit thật

Hiện tại `Assets/Editor/StarVanguardPlayModeSmoke.cs` có 57 lời gọi `Assert(...)` tự chế. Cách chuyển:

1. Tạo `Assets/Tests/PlayMode/PlayModeSmokeTests.cs`:

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayModeSmokeTests
{
    [UnityTest]
    public IEnumerator Bootstrap_Loads_Menu_With_Single_Canvas()
    {
        SceneManager.LoadScene("BootstrapScene");
        yield return null;                 // cho BootstrapLoader.Start chạy
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual("MainMenuScene", SceneManager.GetActiveScene().name);
        Assert.AreEqual(1, Object.FindObjectsByType<Canvas>().Length, "phải có đúng 1 Canvas");
    }

    [UnityTest]
    public IEnumerator Gameplay_Spawns_Player_And_No_Errors()
    {
        SceneManager.LoadScene("GameplayScene");
        yield return null;
        yield return new WaitForSeconds(1f);

        Assert.IsNotNull(Object.FindAnyObjectByType<PlayerController>(), "player phải tồn tại");
        LogAssert.NoUnexpectedReceived();   // không có error log
    }
}
```

2. Xoá dần phần tương ứng trong `StarVanguardPlayModeSmoke.cs`, giữ lại nút menu cho dev nếu muốn.
3. **DoD:** `run_tests(mode: "PlayMode")` trả ≥ 10 test, tất cả pass; log không có `NullReferenceException`.

> `LogAssert.NoUnexpectedReceived()` chính là thứ biến "chơi thử bằng mắt" thành kiểm thử tự động — rất đáng giá cho phần còn lại của dự án.

---

### T-201 — Hợp nhất HUD (chi tiết thi công)

**Hiện trạng:** uGUI HUD (score/lives/bombs) + IMGUI HUD (`OnGUI`) cùng vẽ; `OnGUI` gọi `FindAnyObjectByType` + `Camera.main` + string interpolation mỗi frame.

**Các bước:**

1. `read_console` → xác nhận đang có 0 error trước khi sửa (baseline).
2. Trong `GameplayScene`, chọn object chứa `UIManager` → `manage_components` **add** `OverheatGaugeUI` sẵn vào object, rồi gán `m_OverheatGaugeUI` (thay vì để `UIManager.Start` tự `AddComponent`).
3. Sửa `UIManager.cs:51` và `:57`: bỏ `?? gameObject.AddComponent<...>()`, thay bằng log warning nếu field null (fail-fast, dễ thấy lỗi authoring).
4. Sửa `OverheatGaugeUI.cs`:
   * đặt `m_EnableProceduralHUD` mặc định `false` và **xoá** toàn bộ `OnGUI`, `initStyles`, các `Texture2D` runtime (dòng 151-165 tạo texture, 167-197 initStyles, 200-298 OnGUI);
   * nhánh uGUI đã có sẵn field `m_HeatSlider`, `m_HeatFillImage`, `m_HeatStatusText` → bind trong scene qua `manage_components`;
   * thay emoji bằng ký tự ASCII (`SCORE`, `HULL`, `BOMBS`) hoặc sprite icon.
5. `refresh_unity` → `read_console` → `manage_editor(play)` → chơi 1 wave → chụp màn hình so sánh.
6. `python3 Tools/check_project.py` → WARN `hud_duplication` và `hot_path` (OverheatGaugeUI) phải biến mất.

**DoD:** HUD chỉ còn 1 lớp; không còn `OnGUI` trong `Assets/Scripts`; Profiler không còn allocation đều đặn mỗi frame khi đứng yên.

---

### T-202 / T-203 — Quyết định Co-op (✅ đã chọn B, xem `docs/ADR-001-coop.md`)

| | **Phương án B — ĐÃ CHỌN ✅** | Phương án A (để sau, xem mục 5 của ADR-001) |
| :--- | :--- | :--- |
| Việc | Xoá `Assets/Scripts/Networking/` (4 file), bỏ nút Co-op trong `MainMenuScreen`, bỏ `UIPanelId.Coop` hoặc chuyển thành "2 người cùng bàn phím", sửa `WaveDirector.cs:713` bỏ gọi `GetActivePlayerCount()` | Cài Mirror, thêm `versionDefines`, gắn `NetworkManager` + `NetworkPlayerController`, spawn prefab, sync boss HP |
| Thời gian | 0.5d | 3–5d + test 2 máy |
| Rủi ro | Thấp | Cao (desync, spawn authority, scene có network object) |
| Báo cáo BTL | Trung thực, gọn | Ấn tượng nhưng dễ vỡ khi demo |
| Gate | G1+G2 | G1+G2 + test 2 instance thật |

> Nếu chọn A: **bắt buộc** bật `MIRROR` bằng `versionDefines` trong asmdef (`"com.unity.mirror": "MIRROR"`), nếu không code vẫn bị loại khỏi build và bạn sẽ tưởng Mirror "không hoạt động".

---

## 3. Định nghĩa "Done" cho mỗi sprint

Một task chỉ được đánh ✅ khi có **đủ 4 bằng chứng** dán vào PR/commit message:

1. `git diff --stat` (phạm vi thay đổi hợp lý, không có scene diff khổng lồ bất thường);
2. Log `read_console` sau khi sửa: `0 error`;
3. Kết quả `run_tests` (số pass/fail) hoặc output `StarVanguardValidator`;
4. Output `python3 Tools/check_project.py` (không tăng FAIL/WARN so với baseline).

Với task UI/HUD: thêm **ảnh chụp trước/sau** (Play Mode).

---

## 4. Nhật ký tiến độ (điền dần)

| Ngày | Task | Kết quả | Gate | Ghi chú |
| :--- | :--- | :--- | :-: | :--- |
| 30/10/2026 | Audit + T-003 | Báo cáo `docs/AUDIT_2026-10-30.md`; `check_project.py` = 1 FAIL / 11 WARN / 12 OK | G3 | FAIL duy nhất = thiếu Mirror (đúng thực trạng) |
| 30/10/2026 | T-103 + T-202 | Cắt bỏ Co-op LAN theo ADR-001: xoá 4 script Networking + `HomeScreenController`, bỏ `UIPanelId.Coop`, bỏ panel/nút Co-op, bỏ `m_NetworkLobbyUI`, thêm Editor cleanup tool + check `legacy_coop` | G3 ✅ (0 FAIL / 10 WARN / 13 OK) | G1/G2 **chưa chạy** — cần mở Unity xác nhận compile + Play Mode |
| 30/10/2026 | T-204 (mở) | Chờ chạy menu cleanup trong Unity để dọn `CoopPanel/CoopCard/CoopButton` khỏi `MainMenuScene` | — | Sau đó WARN `legacy_coop` sẽ hết |
