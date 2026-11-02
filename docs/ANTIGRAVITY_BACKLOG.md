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
| **T-002** | ✅ xong | README root + hướng dẫn kiến trúc & controls + dọn cảnh báo `docs_cleanup` | `README.md`, `docs/` | — | G3 | 0.5d |
| **T-101** | ✅ xong | Tạo 5 assembly definition (Runtime/UI/Editor/Tests.EditMode/Tests.PlayMode) | `Assets/**/*.asmdef` | `create_script`, `refresh_unity` | G1+G3 | 1d |
| **T-102** | ✅ xong | Smoke test & NUnit validator tự động (37 checks + PlayMode smoke test) | `Assets/Editor/**`, `Assets/Tests/**` | `run_tests`, batchmode | G1+G2 | 1.5d |
| **T-103** | ✅ xong | ADR-001: cắt bỏ Mirror Co-op LAN nợ kỹ thuật cũ | `docs/ADR-001-coop.md` | — | — | 0.2d |
| **T-201** | ✅ xong | Hợp nhất HUD: uGUI Zero-GC OverheatGaugeUI, bỏ `OnGUI`, chuyển sang static event `OnHeatChanged` | `OverheatGaugeUI.cs`, `UIManager.cs`, `GameplayScene` | `manage_components` | G1+G2+G3 | 1.5d |
| **T-202** | ✅ xong | Dọn dẹp tàn dư Networking cũ, bỏ `HomeScreenController`, chuyển hướng navigation | Runtime, UI | edit tool | G1+G2+G3 | 0.5d |
| **T-204** | ✅ xong | Dọn sạch `CoopPanel/CoopCard/CoopButton` chết khỏi `MainMenuScene.unity` | `MainMenuScene.unity` | Editor script | G1+G2+G3 | 0.1d |
| **T-203** | ✅ xong | Tái thiết kế LAN Co-op theo ADR-002: Unity Netcode for GameObjects (NGO) 2.5.0 + Unity Transport | NGO packages, `NetworkGameManager`, `CoopScreen` | batchmode | G1+G2+G3 | 3d |
| **T-405** | ✅ xong | Đồng bộ In-Game UI: PauseScreen, SettingsScreen, GameOverScreen chuẩn UITheme sci-fi Zero-GC | `PauseScreen.cs`, `SettingsScreen.cs`, `GameOverScreen.cs` | batchmode | G1+G2+G3 | 1.5d |
| **T-301** | P1 | Migrate gameplay input sang Input System (InputReader + gamepad + rebinding) | 5 script, `.inputactions` | `script_apply_edits`, `validate_script` | G1+G2 | 2d |
| **T-302** | ✅ xong | Audio: AudioMixer (Master/Music/SFX) + pool 12 AudioSource, bỏ đổi pitch toàn cục | `AudioManager.cs`, `Assets/Audio/AudioMixer.mixer` | `manage_asset`, `manage_components` | G1+G2 | 1.5d |
| **T-303** | ✅ xong | Vòng đời event + fake-null: bỏ `?.`/`??` trên `UnityEngine.Object`, unsubscribe không điều kiện | `AudioManager`, `UIManager`, `PlayerShooting`, khác | edit tool | G1+G2 | 0.5d |
| **T-304** | ✅ xong | Chính sách FPS/độ phân giải: `targetFrameRate 60`, vSync theo setting, bỏ ép `SetResolution` 2 lần | `GameManager.cs`, `QualitySettings` | `manage_graphics` | G1+G3 | 0.5d |
| **T-305** | ✅ xong | Bỏ cấp phát trong đường đạn: mảng tĩnh cho Plasma, registry thay `FindObjectsByType` trong `Bomb`/`PoolManager` | `PlayerShooting.cs`, `Bomb.cs`, `PoolManager.cs` | edit tool, `manage_profiler` | G1+G2+G3 | 1d |
| **T-306** | P2 | Save/Leaderboard: JSON có `schemaVersion` + migration, tách khỏi PlayerPrefs thô | `LeaderboardManager.cs`, `SettingsManager.cs` | edit tool | G2 | 1d |
| **T-401** | P1 | Prefab hoá UI (Home/Settings/Controls/Coop) → scene < 3.000 dòng | `MainMenuScene`, `UI_Prefabs/*` | `manage_prefabs`, `batch_execute` | G1+G2+so sánh ảnh | 2–3d |
| **T-402** | P1 | Dọn rác: `HomeScreenController`, `ControlsPanelController`, `URP2DSceneTemplate.unity`, `m_RaycastTarget` thừa | nhiều | `manage_asset` | G3 | 0.5d |
| **T-403** | P2 | Thống nhất namespace + đặt tên (`StarVanguard.*`), bổ sung `[FormerlySerializedAs]` khi đổi tên | toàn bộ script | edit tool | G1 | 1.5d |
| **T-404** | P2 | Pipeline asset: `AssetPostprocessor` cho sprite (PPU/filter/9-slice) + gắn `generate_scifi_sprites.py` vào `Tools/` | `Assets/Editor/`, `Tools/` | `manage_asset` | G3 | 1d |
| **T-501** | P1 | Perf pass có số: profile 60s wave 5, báo cáo before/after (FPS, GC/frame, ms) | toàn bộ | `manage_profiler` | G1+G2 | 1d |
| **T-502** | ✅ xong | Game feel: camera shake với Perlin/decay, URP Bloom, Vignette cho đạn và không gian | prefabs, URP volume | `manage_graphics`, `manage_camera` | G1+G2 | 1.5d |
| **T-503** | P2 | Việt hoá UI + font TMP có glyph tiếng Việt (kiểm tra dấu) | `Assets/TextMesh Pro`, UI prefabs | `manage_asset` | ảnh chụp | 1d |
| **T-601** | ✅ xong | Build Windows Standalone Release (`StarVanguard.exe`) tự động qua batchmode | `Build/Windows/` | `StarVanguardBuildPipeline` | G1+G2+G3 | 0.5d |

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
| 30/10/2026 | T-103 + T-202 | Cắt bỏ Mirror Co-op cũ theo ADR-001: xoá 4 script Networking cũ + dọn sạch dead code | G1+G2+G3 | Đã dọn dẹp an toàn |
| 30/10/2026 | T-204 | Dọn sạch `CoopPanel/CoopCard/CoopButton` khỏi `MainMenuScene.unity` | G1+G2+G3 | WARN `legacy_coop` đã hết (0 FAIL, 18 OK) |
| 30/10/2026 | T-201 + T-401 | uGUI Zero-GC OverheatGaugeUI + đóng gói UI Prefabs MainMenuScene (< 1900 dòng) | G1+G2+G3 | 100% Zero-GC, không còn OnGUI |
| 31/10/2026 | T-203 (ADR-002) | Tái thiết kế LAN Co-op chuẩn Unity Netcode (NGO 2.5.0) + NetworkGameManager + CoopScreen UI | G1+G2+G3 | Host LAN / Join IP / Sync vị trí phi thuyền |
| 31/10/2026 | T-405 (In-Game UI) | Khắc phục nút Back Co-op; Xây dựng mới PauseScreen (5 actions) & SettingsScreen (Audio/Graphics) | G1+G2+G3 | UITheme chuẩn dark navy #0B0E17 & neon cyan #00F0FF |
| 31/10/2026 | T-405 (GameOver) | Tái thiết kế toàn diện GameOverScreen: Final Score 52pt, New High Score Input, Leaderboard Top 5 | G1+G2+G3 | Nút Chơi Lại, Cài Đặt, Về Menu chuẩn sci-fi; 37/37 checks pass |
| 31/10/2026 | T-601 | Build Windows Release `Build/Windows/StarVanguard.exe` (150 MB) | G1+G2+G3 | 100% playable, 0 compile errors |
