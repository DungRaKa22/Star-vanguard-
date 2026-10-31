# ADR-002 — Tái thiết kế chế độ LAN Co-op 2 người bằng Unity Netcode for GameObjects (NGO)

* **Trạng thái:** ✅ Đã quyết định & Bắt đầu thi công (31/10/2026)
* **Người quyết định:** Chủ dự án Star Vanguard & Antigravity
* **Liên quan:** `docs/ADR-001-coop.md` (Mục 5), `docs/ANTIGRAVITY_BACKLOG.md` (Phase 4 / T-203), `plan_phase4_lan_coop.md`
* **Loại:** Kiến trúc mạng / Netcode framework

---

## 1. Bối cảnh & Cơ sở quyết định

Sau khi hoàn tất 100% Single-Player Core trong các Sprint 0–3 (đạt chuẩn 3 cổng: 0 lỗi biên dịch, 67/67 PlayMode assertions, 13/13 NUnit tests, 37/37 Validator checks, và 0 FAIL tĩnh), dự án Star Vanguard bước vào **Phase 4: Chế độ Co-op 2 người chơi qua mạng nội bộ (LAN Co-op)**.

Trước đây, ADR-001 đã cắt bỏ lớp Mirror cũ vì code hỏng, không có thư viện và `#if MIRROR` bị bỏ qua. ADR-001 mục 5 đặt ra 5 điều kiện tiên quyết để mở lại Co-op:
1. Đã có asmdef tách bạch và bộ test PlayMode xanh 100%.
2. Có Netcode framework được lựa chọn rõ ràng thông qua ADR riêng.
3. Phân tách input và mô phỏng (không để `PlayerController` gọi chéo làm xung đột chuột giữa 2 máy).
4. Có quy trình kiểm thử 2 instance (Editor + Standalone Windows Build).
5. Kế hoạch và ngân sách được thống nhất.

Dự án hiện tại đã đáp ứng đầy đủ cả 5 điều kiện trên.

---

## 2. Lựa chọn Framework: Unity Netcode for GameObjects (NGO)

So sánh giữa **Unity Netcode for GameObjects (NGO)** và **Mirror Networking**:

| Tiêu chí | Unity Netcode for GameObjects (NGO) | Mirror Networking |
| :--- | :--- | :--- |
| **Nguồn gốc** | Giải pháp chính thức (First-party) của Unity Technologies | Thư viện mã nguồn mở cộng đồng |
| **Tương thích Unity 6** | Tích hợp hoàn hảo Unity 6000.5.7f1 qua Package Manager | Cần cài từ OpenUPM/Git, phụ thuộc bản cập nhật bên thứ 3 |
| **Tầng truyền tải (Transport)** | `UnityTransport` (kế thừa UTP, tối ưu UDP, hỗ trợ đa luồng) | Kcp, Telepathy, SimpleWebTransport |
| **Độ ổn định API** | Hiện đại: `NetworkBehaviour`, `NetworkVariable`, `[Rpc]`, `NetworkManager` | Cổ điển: `SyncVar`, `[Command]`, `[ClientRpc]` |
| **Hỗ trợ kiểm thử** | Tích hợp công cụ `Multiplayer Play Mode` trên Unity 6 | Phải dùng ParrelSync hoặc build tay |

👉 **Quyết định:** Chọn **Unity Netcode for GameObjects (NGO)** kết hợp `UnityTransport` (UDP Port `7777`) cho Star Vanguard.

---

## 3. Kiến trúc Đề xuất (Zero Coupling & Deterministic Seed)

```mermaid
graph TD
    subgraph UI_Assembly ["StarVanguard.UI (Không phụ thuộc NGO)"]
        Menu["MainMenuScreen.prefab"] --> CoopPanel["LAN Coop Modal / Panel (uGUI)"]
    end

    subgraph Net_Assembly ["StarVanguard.Net (Assembly Cầu Nối Mạng)"]
        Presenter["CoopMenuPresenter (Lắng nghe UI Button Events)"]
        NetManager["NetworkManagerBootstrap (NGO NetworkManager + UnityTransport)"]
        NetBridge["NetworkPlayerBridge (Owner Position Sync & Bullet RPC)"]
        NetWave["CoopWaveCoordinator (Shared Random Seed, Enemy Hit RPC & Boss HP x2)"]
    end

    subgraph Runtime_Core ["StarVanguard.Runtime (Độc lập 100% với Netcode)"]
        PlayerShip["PlayerController (ActivePlayers List) & PlayerShooting"]
        WaveDir["WaveDirector & EnemyController"]
        Boss["BossController (ScaleForCoop Hook)"]
        Pool["PoolManager (Zero-GC Bullet & VFX Spawning)"]
    end

    CoopPanel -.->|Đăng ký sự kiện từ xa| Presenter
    Presenter -->|StartHost / StartClient| NetManager
    NetManager -->|NetworkSceneManager.LoadScene| NetWave
    NetManager --> NetBridge
    NetBridge -->|Bật/Tắt theo IsOwner| PlayerShip
    NetBridge -->|Spawn đạn qua RPC| Pool
    NetWave -->|InitState(Seed) & Sync Kill| WaveDir
    NetWave -->|ScaleForCoop(2)| Boss
```

### Nguyên tắc then chốt:
1. **Cô lập Assembly một chiều:**
   - `StarVanguard.Runtime.asmdef` và `StarVanguard.UI.asmdef` **tuyệt đối không** tham chiếu tới `StarVanguard.Net` hay `Unity.Netcode.Runtime`.
   - `StarVanguard.Net.asmdef` là assembly tầng trên, tham chiếu tới `StarVanguard.Runtime`, `StarVanguard.UI`, `Unity.Netcode.Runtime`, `Unity.Collections`, `Unity.InputSystem`.
2. **Owner-Authoritative Movement (0ms input lag):**
   - Chủ sở hữu tàu di chuyển chuột/bàn phím tức thì và ghi vào `NetworkVariable<Vector2>` với quyền ghi `Owner`. Máy đối diện nội suy mượt mà qua `Vector3.Lerp`.
   - Tàu đối phương sẽ tắt `PlayerController` và `PlayerShooting` trên máy khách để tránh việc chuột điều khiển cả 2 tàu cùng lúc.
3. **Deterministic Shared Seed:**
   - Host sinh hạt giống ngẫu nhiên `waveSeed` trước mỗi Wave và phát qua RPC `StartSynchronizedWaveRpc(waveIndex, waveSeed)`. Cả 2 máy gọi `Random.InitState(waveSeed)` trước khi sinh đàn quái $\rightarrow$ Toàn bộ quỹ đạo bay và hành vi giống hệt nhau mà không cần gắn `NetworkObject` lên hàng trăm con quái/đạn.
4. **Boss HP Scaling:**
   - Khi `ConnectedClientsIds.Count >= 2`, tự động gọi `BossController.Instance.ScaleForCoop(2)` để tăng 65% máu Boss theo đúng công thức thiết kế.
5. **Cơ chế sống sót đồng đội (Co-op Revive/Survival):**
   - Trận đấu vẫn tiếp tục khi 1 người chơi hy sinh, chỉ `GameOver` khi cả 2 người cùng hết mạng.

---

## 4. Kế hoạch nghiệm thu & 3 Cổng Kiểm Chứng
- **G1 (Compile):** Biên dịch sạch 5 assemblies không có lỗi.
- **G2 (Single-Player Regression):** 100% bài test EditMode & PlayMode Smoke của chế độ chơi đơn phải tiếp tục đỗ xanh.
- **G2 (2-Instance LAN Test):** Chạy Host trên Editor và Client trên `Build/Windows/StarVanguard.exe`, kết nối qua `127.0.0.1:7777`.
- **G3 (Static Audit):** `python Tools/check_project.py` đạt 0 FAIL.
