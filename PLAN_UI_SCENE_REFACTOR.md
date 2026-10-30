# STAR VANGUARD — KẾ HOẠCH TÁI CẤU TRÚC SCENE & UI

> Trạng thái: **P0 + P1 HOÀN TẤT** (2026-10-29) — xem mục H để biết đã làm gì và kiểm chứng ra sao.

---

## A. Chẩn đoán (số liệu đo được, không phỏng đoán)

| # | Bằng chứng | Hệ quả |
|---|---|---|
| 1 | `md5sum` của `GameplayScene.unity`, `MainMenuScene.unity`, `SampleScene.unity` **giống hệt nhau** (`90f807f2…`, 345.837 B, 367 GameObject/scene) | Cả 3 scene là **một** scene copy 3 lần. Không có scene nào "là menu", không có scene nào "là gameplay". |
| 2 | 13 panel/HUD đều `m_IsActive: 1` trong file scene (HomeScreen, Settings, Controls, Leaderboard, Pause, GameOver, NewHighScore, InterWave, HUD, …) | Mở scene là thấy **tất cả chồng lên nhau** — đúng như 2 ảnh bạn gửi. Chỉ vì `UIManager.Start()` gọi `hideAllPanels()` nên lúc *chạy* mới tạm ổn. |
| 3 | Tồn tại **2 hệ UI song song**: 12 panel uGUI/TMP **+** `MainMenuUI.OnGUI()` (IMGUI) vẽ "LAUNCH MISSION / CO-OP / CONTROLS / EXIT" và modal Controls | Hai menu tranh nhau cùng màn hình, cùng nhận click → cảm giác "đơ/hỗn loạn". |
| 4 | `UIManager.Start()` tự `gameObject.AddComponent<MainMenuUI>()` nếu thiếu | Menu IMGUI **xuất hiện cả trong GameplayScene**, đè lên gameplay. |
| 5 | Cỡ chữ rải rác: 50 (×24 chỗ), 75, 60 (×2), 36 (×2), 30, 24 (×5) | Không có thang typography → mỗi panel một kiểu, nhìn "rời rạc". |
| 6 | `SampleScene` vẫn nằm trong Build Settings (index 2) | Bản build có 1 scene rác trùng lặp, dễ ship nhầm. |
| 7 | CanvasScaler: `ScaleWithScreenSize`, 1920×1080, match 0.5 | **Đã đúng** → giữ nguyên, không đổi. |
| 8 | Keycode hiện dùng: Space, Return, KeypadEnter, LeftShift, Escape, Alpha1–5, Mouse0/1 | Rời rạc, không có bảng phím chung, panel CONTROLS hard-code text dễ lệch code. |

**Kết luận:** gameplay "chạy được" chỉ vì nó là scene duy nhất có state machine ép ẩn panel. Vấn đề không phải đồ hoạ — mà là **kiến trúc scene/UI**: 1 scene vạn năng + 2 hệ UI + không có design token.

---

## B. Thứ tự scene mục tiêu

```
Build Settings
  0  MainMenuScene      (Menu / Lobby / Settings / Controls / Leaderboard)
  1  GameplayScene      (HUD / Pause / GameOver / NewHighScore / InterWave)
  —  SampleScene        XOÁ khỏi build + xoá file
```

Luồng chạy:

```
Boot ─▶ MainMenu ──LAUNCH──▶ Gameplay ──ESC──▶ Paused ──RESUME──▶ Gameplay
                  ◀──EXIT MENU──┘   │                    │
                  ◀──────────── GameOver ◀──hết mạng─────┘
```

Phân bổ object (nguyên tắc: **scene nào chỉ chứa thứ scene đó cần**):

| MainMenuScene | GameplayScene |
|---|---|
| Main Camera, EventSystem, Global Light 2D | Main Camera, EventSystem, Global Light 2D |
| `MenuRoot` → Home / Settings / Controls / Leaderboard / Coop | `HudRoot` → HUD, OverheatGauge |
| Managers: GameManager, AudioManager, SettingsManager, LeaderboardManager | `OverlayRoot` → Pause / GameOver / NewHighScore / InterWave |
| ParallaxRoot (nền menu) | Player, Pools, Boundaries, Parallax, WaveDirector, Bosses |
| — | (không có Home/Settings/Controls → Settings trong pause = prefab dùng chung) |

**Cách làm an toàn:** không sửa YAML bằng tay. Viết editor tool `StarVanguardSceneSplitter` chạy 1 lần: backup → xoá object theo danh sách tên → set toàn bộ panel `SetActive(false)` trong file → lưu scene → rebuild build settings. Chạy lại được nhiều lần (idempotent).

---

## C. Thang đo UI thống nhất (design token)

Reference **1920×1080**, match 0.5, lưới **8 px**.

| Token | Giá trị | Dùng cho |
|---|---|---|
| Display | 72 | Logo "STAR VANGUARD", GAME OVER |
| H1 | 48 | Tiêu đề panel (SETTINGS, LEADERBOARD) |
| H2 | 32 | Tiêu đề mục, tên wave |
| Body | 24 | Nội dung, nhãn nút |
| Caption | 18 | Chú thích, hint phím, version |
| HUD | 36 | Score, lives, bomb, % nhiệt |

| Component | Kích thước |
|---|---|
| Button primary | 440 × 76, padding 24, gap 16, radius 12 |
| Button secondary | 340 × 64 |
| Panel form (Settings/Controls) | rộng 720, padding 40 |
| Panel rộng (Leaderboard) | rộng 1000 |
| Icon HUD | 32 × 32 (giữ) |
| Safe area | lề 64 hai bên |

Triển khai: `UITheme` (ScriptableObject) + `UiStyles.cs` (hằng số + helper) + editor tool `ApplyUiTheme` quét toàn bộ `TextMeshProUGUI` trong scene và gán size theo **role** → thay 24 chỗ font 50 bằng 1 lần chạy, không sửa tay.

---

## D. Bàn phím / chuột — một nguồn sự thật

`InputMap.cs` (static) là nơi duy nhất khai báo phím; mọi script đọc từ đây; panel CONTROLS **sinh text từ chính bảng này** nên không bao giờ lệch.

| Hành động | Phím / chuột |
|---|---|
| Di chuyển | `WASD` **hoặc** mũi tên, hoặc chuột (direct follow) |
| Bắn | `Space` / Chuột trái (giữ = autofire) |
| Bom | `LeftShift` / Chuột phải |
| Đổi vũ khí | `1` `2` `3` `4` |
| Pause / Resume | `Esc` |
| Xác nhận | `Enter` / `KeypadEnter` |
| Quay lại | `Esc` / `Backspace` |
| Điều hướng menu bằng phím | `↑` `↓` + `Enter` (thêm mới) |

Bỏ `Alpha5` (không có vũ khí thứ 5) để không tạo phím chết.

---

## E. Các phase thi công

| Phase | Nội dung | Ước lượng |
|---|---|---|
| **P0 — Dọn nền** | Xoá `SampleScene`; tách object giữa 2 scene bằng editor tool; mọi panel mặc định `inactive`; build settings còn 2 scene | 0,5 ngày |
| **P1 — Một hệ UI** | Bỏ IMGUI khỏi `MainMenuUI`, dựng menu uGUI/TMP thật; `PanelRouter` (đúng 1 panel active, có back-stack, fade 0.15 s bằng CanvasGroup); bỏ auto-AddComponent | 1 ngày |
| **P2 — Design token** | `UITheme` + `ApplyUiTheme`; restyle toàn bộ panel theo bảng C | 1 ngày |
| **P3 — Input** | `InputMap` + panel Controls sinh từ bảng; thêm điều hướng menu bằng phím; hint 3 dòng lần đầu chơi | 0,5 ngày |
| **P4 — Scene flow** | `SceneRouter` fade 0,25 s, chống double-click; state machine map 1 state ↔ 1 panel; restart không cần reload | 0,5 ngày |
| **P5 — Chống tái phát** | Mở rộng smoke headless: mỗi state assert **đúng 1 panel active**, không 2 EventSystem, không Canvas lồng nhau, chụp screenshot; `UI_LayoutAudit` cảnh báo lệch safe-area / font ngoài thang | 0,5 ngày |

Tổng: **~4 ngày**. Sau P0+P1 là đã hết "hỗn loạn" nhìn thấy được.

---

## F. Sáng kiến nâng trải nghiệm (chọn theo mức ưu tiên)

1. **Menu holodeck** — tái dùng `ParallaxManager` sẵn có cho nền menu + phi thuyền bay ngang → menu sống, không cần asset mới.
2. **HUD cockpit** — dồn score/lives/bomb/nhiệt thành 1 thanh dưới, dùng token mới, đọc được trong 0,2 s.
3. **Briefing giữa wave** — dùng `InterWaveMessagePanel` hiện có, in "WAVE 3 — PINCER" + 1 câu dạy cơ chế (dạy người chơi bằng chính wave sắp tới).
4. **Chọn độ khó ở menu** — 3 nút (Cadet / Veteran / Ace) map vào `GameManager.CurrentDifficultyTier` đã có sẵn.
5. **Settings có preview** — kéo volume nghe tiếng beep ngay, kéo độ nhạy thấy con trỏ phản hồi.
6. **Accessibility** — remap phím, giảm rung màn hình, palette mù màu, toggle "giữ để bắn" vs "bắn tự động".
7. **Leaderboard kiểu arcade** — nhập tên 3 ký tự khi vào top 10, lưu local.

---

## G. Rủi ro & giảm thiểu

| Rủi ro | Giảm thiểu |
|---|---|
| Sửa scene bằng editor tool có thể mất object | Backup `.unity` trước mỗi lần chạy; tool idempotent; chạy smoke sau mỗi phase |

---

## H. Đã thực hiện (P0 + P1)

**P0 — Tách scene** (tool `StarVanguardSceneSplitter`, backup ở `SceneBackups/`):

- `MainMenuScene` giờ **không còn**: `Player`, `Boundaries`, `WaveDirector`, `InterWaveMessageManager`, `HUDContainer`, `PausePanel`, `GameOverPanel`, `NewHighScorePanel`, `InterWaveMessagePanel`, `HomeScreenPanel` (10 object gameplay/menu cũ).
- `GameplayScene` **không còn**: `HomeScreenPanel`, `ControlsPanel` (+ container phím bên trong).
- Mọi panel còn lại mặc định `inactive` trong file scene (mở scene không còn thấy rác).
- `SampleScene` đã xoá file + khỏi build. Build order: `0 MainMenuScene`, `1 GameplayScene`.
- Nối tiếp: `UIPanelRouter` + `MainMenuScreen` được gắn vào `Canvas` của từng scene.

**P1 — Một hệ UI**:

- Xoá `MainMenuUI.cs` (IMGUI) — menu thứ hai đè lên uGUI đã biến mất.
- `UITheme.cs`: thang đo 1920×1080, lưới 8 px, type ramp 72/48/32/24/18/36, nút 440×76, sprite bo góc 9-slice sinh bằng code.
- `UIPanelRouter.cs`: đúng một panel hiện tại, back-stack, fade 0,12 s (unscaled).
- `MainMenuScreen.cs`: logo + 5 nút + chọn độ khó (Cadet/Veteran/Ace) + thẻ BEST SCORE + footer phím; panel Co-op dựng bằng uGUI gọi thẳng `NetworkLobbyUI.StartHostLAN/JoinClientLAN`.
- `UIManager` không còn tự `AddComponent<MainMenuUI>()`; mọi chuyển panel đi qua router.
- `GameManager`: `SelectedDifficulty` + `SetSelectedDifficulty()`; độ khó menu dịch đường cong tier của wave.
- `LeaderboardManager.GetHighestScore()` cho thẻ BEST SCORE.

**Kiểm chứng** — smoke headless mở rộng lên 25 assert (`passed=25 failed=0 exitCode=0`): menu build đúng, chỉ 1 panel hiển thị, không HUD/WaveDirector/Player trong menu, đúng 1 Canvas + 1 EventSystem, độ khó tới được `GameManager`, và gameplay vẫn spawn wave 1 + cơ chế quá nhiệt hoạt động.

**Còn lại**: `Reports/` (briefing giữa wave), HUD cockpit, `SceneRouter` fade chuyển cảnh, và 2 overlay IMGUI còn sót (`OverheatGaugeUI`, `NetworkLobbyUI`) sẽ chuyển nốt sang uGUI ở P2/P3.

**Vá lần 2 — Settings (2026-10-29):** nút BACK của Settings nằm ở `(-810, 685)` tính từ tâm panel, tức **lơ lửng ngoài panel và trên cả màn hình** → không bao giờ bấm được; đã đưa về đáy panel (300×100, cách đáy 40 px). Ngoài ra listener persistent trỏ tới `ControlsPanelController` với target rỗng đã bị gỡ; **không** gắn thêm listener `HideSettingsPanel` vì `SettingsUI` đã tự gắn `CloseSettings` lúc chạy — gắn trùng làm BACK gọi 2 lần, router bị pop 2 nấc và rơi về `None`. Smoke: `passed=38 failed=0`, log `depth=2 → 1 → Home`.

**Vá lần 1 — Controls (2026-10-29):** panel CONTROLS cũ chiếm `1920×1440` và có lớp `Panel` `1680×820` **raycast** vẽ đè lên chính nút BACK của nó → mở ra là mất click. Đã xoá panel cũ khỏi scene và dựng `ControlsScreen` (card `980×720`) sinh từ `InputMap`. Đồng thời chuẩn hoá: panel nào cao quá 1040 px đều bị hạ xuống (`SettingsPanel` 1520×1120, `GameOverPanel` 1337×1317, `InterWaveMessagePanel` 1920×1440). Smoke giờ raycast thật vào nút BACK và nút LAUNCH MISSION để chứng minh không còn lớp chặn click: `passed=32 failed=0`.
| `MainMenuUI` bỏ IMGUI làm mất nút Co-op | Panel Co-op dựng lại bằng uGUI trong P1, giữ nguyên API `NetworkLobbyUI` |
| GameplayScene đang chạy tốt, dễ vỡ | P0 chỉ **xoá object không dùng** trong từng scene, không đổi component của gameplay |
| 2 EventSystem sau khi tách scene | P5 assert; P0 đảm bảo mỗi scene đúng 1 |
