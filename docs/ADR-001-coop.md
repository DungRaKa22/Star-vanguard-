# ADR-001 — Cắt bỏ lớp Co-op LAN (Mirror) khỏi bản chơi đơn

* **Trạng thái:** ✅ Đã quyết định & đã thi công (30/10/2026)
* **Người quyết định:** chủ dự án Star Vanguard
* **Liên quan:** P0-1 trong `docs/AUDIT_2026-10-30.md`, task T-103/T-202 trong `docs/ANTIGRAVITY_BACKLOG.md`
* **Loại:** kiến trúc / phạm vi (scope)

---

## 1. Bối cảnh

Dự án được tài liệu hoá là "Co-op LAN 2–4 người qua Mirror Networking", nhưng kiểm tra mã nguồn ngày 30/10/2026 cho thấy **tính năng này chưa từng chạy được**:

| Bằng chứng | Hệ quả |
| :--- | :--- |
| `Packages/manifest.json` không có gói Mirror (hay bất kỳ gói networking nào) | Không có thư viện mạng |
| 4 script trong `Assets/Scripts/Networking/` bọc trong `#if MIRROR` | Toàn bộ logic mạng bị loại khỏi biên dịch |
| Không có `.asmdef` / `versionDefines` / Scripting Define nào định nghĩa `MIRROR` | Kể cả khi cài Mirror, code vẫn không được biên dịch |
| `StarVanguardNetworkManager` (nhánh `#else`) in log *"Mirror Co-op ready"* | Gây hiểu nhầm là đã hoạt động |
| `NetworkPlayerController`, `NetworkWaveSynchronizer` không được gắn vào scene/prefab nào | Dead code 100% |
| `WaveDirector.cs` gọi `GetActivePlayerCount()` (fallback luôn trả `1`) | Boss không bao giờ scale theo số người |
| Menu có `CoopButton`, `CoopPanel` + `HostButton`/`JoinButton` | UI mời người chơi bấm vào một tính năng không tồn tại |

Tức là: **UI có, code có, nhưng tính năng không có.** Với một đồ án cần bảo vệ/demo, đây là rủi ro lớn hơn nhiều so với việc thiếu tính năng.

## 2. Quyết định

Chọn **phương án B** trong báo cáo audit:

> **Cắt bỏ hoàn toàn Co-op LAN khỏi bản hiện tại. Star Vanguard là game bắn súng không gian 1 người.**
> Giữ lại *điểm móc mở rộng* (`BossController.ScaleForCoop`) để lần làm co-op sau không phải thiết kế lại.

Phương án A (cài Mirror và làm thật) bị loại vì: cần 3–5 ngày, cần test 2 máy, dễ phát sinh desync/authority bug ngay sát ngày bảo vệ, và **không nên làm tính năng mạng trên một codebase chưa có test tự động** (khi đó không có cách nào biết mình làm hỏng gì).

## 3. Đã thi công

| # | Thay đổi | File |
| :-: | :--- | :--- |
| 1 | Xoá toàn bộ lớp networking (4 script + `.meta` + `Networking.meta`) | `Assets/Scripts/Networking/` (đã xoá) |
| 2 | Xoá `HomeScreenController` (chỉ tồn tại để gọi lobby mạng, 0 tham chiếu khác) | `Assets/Scripts/HomeScreenController.cs` (đã xoá) |
| 3 | Bỏ `UIPanelId.Coop` khỏi router | `Assets/Scripts/UI/UIPanelRouter.cs` |
| 4 | Menu: bỏ `BuildCoopPanel`, `BindCoopPanel`, `OnHostCoop`, `OnJoinCoop`; không tạo `CoopButton`; thêm `RemoveLegacyCoopObjects()` tự dọn object Co-op cũ ở runtime | `Assets/Scripts/UI/MainMenuScreen.cs` |
| 5 | Bỏ field `m_NetworkLobbyUI` và đoạn `AddComponent<NetworkLobbyUI>()` | `Assets/Scripts/UIManager.cs` |
| 6 | Boss: bỏ `ScaleForCoop(GetActivePlayerCount())`; HP do `InitializeBoss()` tính theo độ khó | `Assets/Scripts/WaveDirector.cs`, `BossController.cs` (chỉ sửa doc) |
| 7 | Thêm Editor tool dọn object Co-op còn trong scene rồi lưu scene (thay cho việc sửa YAML tay) | `Assets/Editor/StarVanguardCoopCleanup.cs` |
| 8 | Audit tĩnh: thêm check `legacy_coop` để nhắc cho tới khi scene được dọn | `Tools/check_project.py` |

### Việc còn lại phải làm **trong Unity** (không thể làm bằng script ngoài)

```
Unity → menu  Tools ▸ Star Vanguard ▸ 1b. Remove Legacy Co-op UI from MainMenuScene and Save
```

Lệnh này xoá `CoopPanel`, `CoopCard`, `CoopButton` khỏi `MainMenuScene.unity` bằng API của Unity (giữ nguyên fileID/GUID hợp lệ) và tự lưu scene. Sau khi chạy, `python3 Tools/check_project.py` sẽ hết WARN `legacy_coop`.

> Không sửa trực tiếp YAML của scene: xoá một GameObject bằng tay dễ để lại tham chiếu treo (`fileID` không tồn tại) và làm hỏng cả scene — xem luật #1 trong `AGENTS.md`.

## 4. Hệ quả

**Tích cực**

* Không còn UI mời gọi một tính năng chết → demo không bị "bấm vào không có gì xảy ra".
* Ít hơn 4 file + 1 file dead code phải bảo trì; `#if MIRROR` không còn che lỗi biên dịch.
* Audit tĩnh từ `1 FAIL / 11 WARN` xuống **`0 FAIL / 10 WARN`**.
* Không còn log gây hiểu nhầm về mạng.
* Báo cáo BTL giờ mô tả đúng sản phẩm (1 người), tránh rủi ro bị hỏi về phần mạng không tồn tại.

**Tiêu cực / đánh đổi**

* Mất phần "co-op" trong báo cáo → bù bằng các mục khác (REA spec, kiến trúc Singleton/pool, kiểm thử tự động, quy trình AI-assisted).
* Nếu sau này muốn làm co-op, phải viết lại lớp mạng từ đầu **nhưng** trên codebase đã có test (đúng thứ tự).

## 5. Điều kiện để mở lại Co-op (khi nào làm phương án A)

Chỉ bắt đầu lại khi **cả 5 điều kiện** sau đã đạt:

1. Có asmdef tách `Runtime`/`Net` (T-101) và test PlayMode xanh (T-102).
2. Có `Netcode` được chọn rõ: Mirror (có `versionDefines`) hoặc Netcode for GameObjects — quyết định bằng ADR riêng.
3. Gameplay đã tách "input → command → simulation" để server-authoritative, không phụ thuộc `PlayerController` gọi trực tiếp `PoolManager`.
4. Có quy trình test 2 instance (2 build hoặc ParrelSync) trong CI/danh sách kiểm thử tay.
5. Có ngân sách ≥ 5 ngày và **không** nằm trong tuần bảo vệ.

Khi đó `BossController.ScaleForCoop(int playerCount)` đã sẵn sàng: chỉ cần gọi lại từ chỗ spawn boss trong `WaveDirector`.
