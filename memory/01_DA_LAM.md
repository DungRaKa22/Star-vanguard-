# 📜 01. NHỮNG THỨ ĐÃ LÀM (COMPLETED WORK HISTORY)

Tài liệu này ghi lại toàn bộ lộ trình và chi tiết các công việc kỹ thuật đã thực hiện thành công trên dự án **Star Vanguard** từ giai đoạn tiếp nhận mã nguồn gốc đến hiện tại.

---

## 1. GIAI ĐOẠN 1: TIẾP NHẬN & CHUYỂN DỊCH DỰ ÁN SANG UNITY 6 (`6000.5.7f1`)

### 1.1. Đồng bộ và Bảo toàn GUID Metadata
* **Quét toàn bộ mã nguồn cơ sở:** Khảo sát 44 file C# và cấu trúc cây thư mục từ `ChickenInvaders-Unity-master`.
* **Chuyển giao sang `Star Vanguard`:**
  * Đồng bộ toàn bộ tài nguyên vào `C:\Documents\UNITY 2026\Star Vanguard\Assets\`.
  * **Quy tắc bất biến:** Bảo toàn 100% các file `.meta` và mã GUID gốc để đảm bảo không bị đứt gãy liên kết (broken references) trong các file Prefab và Scene (`SampleScene.unity`).
  * Đồng bộ các file cấu hình engine cốt lõi: `TagManager.asset`, `Physics2DSettings.asset`, `EditorBuildSettings.asset`, `InputManager.asset`.
  * Đổi định danh sản phẩm trong `ProjectSettings.asset`:
    * `companyName: StarVanguardStudio`
    * `productName: Star Vanguard`
    * `activeInputHandler: 2` (Hỗ trợ song song cả Legacy Input và New Input System).

### 1.2. Khắc phục các Lỗi Biên Dịch & Định Dạng Tài Nguyên Khởi Điểm
1. **Lỗi ảnh WebP giả mạo `.png`:**
   * *Hiện tượng:* Unity báo lỗi `Could not create asset from Assets/Sprites/Egg.png: File could not be read` trên 8 file ảnh (`BlueChicken_Sprite.png`, `WeaponPickup.png`, `Food.png`, `BombUI.png`, v.v.).
   * *Nguyên nhân:* Header file thực chất là định dạng WebP (`RIFF...WEBP`) nhưng lại đặt đuôi `.png`.
   * *Khắc phục:* Viết script Python sử dụng thư viện Pillow để re-encode toàn bộ 8 file sang chuẩn PNG 32-bit RGBA thực thụ, giữ nguyên file `.meta` đi kèm.
2. **Sửa lỗi phụ thuộc Mirror Package Git:**
   * *Hiện tượng:* Package Manager báo lỗi do đường dẫn git repo Mirror không chứa `package.json` tại thư mục gốc.
   * *Khắc phục:* Xóa URL git lỗi khỏi `Packages/manifest.json`. Tạo file [`StarVanguardNetworkManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/StarVanguardNetworkManager.cs) với chỉ thị tiền xử lý `#if MIRROR ... #else ... #endif` làm lớp đệm shim, giúp dự án biên dịch sạch 100% trong khi chờ import asset Mirror chính thức.
3. **Triệt tiêu 31 cảnh báo Deprecated API (CS0618) của Unity 6:**
   * Thay thế toàn bộ `FindFirstObjectByType<T>()` và `FindObjectsByType<T>(FindObjectsSortMode)` lỗi thời thành `FindAnyObjectByType<T>()` và `FindObjectsByType<T>()` chuẩn Unity 6 trong:
     * [`Bomb.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Bomb.cs)
     * [`GameManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/GameManager.cs)
     * [`PoolManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PoolManager.cs)
     * [`Singleton.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Singleton.cs)
     * [`UIManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UIManager.cs)
4. **Sửa lỗi Domain Reloading của Singleton Pattern:**
   * *Hiện tượng:* Trong Unity 6, cờ tĩnh `_isQuitting` không được tự động reset khi chuyển qua lại Play Mode trong Editor, dẫn tới `GameManager.Instance` trả về `null` gây `NullReferenceException` tại `PlayerController.cs`, `PlayerShooting.cs`, `PlayerBombing.cs`.
   * *Khắc phục:* Bổ sung `_isQuitting = false;` trong `Awake()` của `Singleton.cs` và kiểm tra `Application.isPlaying`. Thêm kiểm tra `if (transform.parent != null) transform.SetParent(null);` trước khi gọi `DontDestroyOnLoad`.

---

## 2. GIAI ĐOẠN 2: DỊCH NGƯỢC TOÀN DIỆN CHICKEN INVADERS 5 BẰNG REA

### 2.1. Thiết lập Bộ công cụ & Môi trường Phân tích
* Tích hợp thành công MCP Server **REA (`rea-agents` v6.7.0)** kết hợp **Ghidra 12.1.4 PUBLIC** và **JDK 21.0.9 HotSpot 64-bit**.
* Nạp mục tiêu nhị phân `CI5demo.exe` (SHA-256: `6107cf79c9671f01f224b7d2b4508cf9296fe924f890825fe8f96ece60f23a84`).

### 2.2. Trích xuất Cấu trúc & Bản đồ Hàm
* Bóc tách thành công **124 file nguồn C++ gốc**, **132 lớp hướng đối tượng** và **288 phương thức** thuộc 2 tầng kiến trúc:
  * `project_uve`: Động cơ độc quyền Ultra VGA Engine (Direct3D 9, BASS Audio, DirectInput, WAD Virtual Filesystem, Bezier Math, NetRole/RPC).
  * `project_game`: Triển khai trò chơi (13 loại vũ khí, 20 loại đạn, FSM Context, Boss Damage Stages, Drop Rate).
* Giải mã cơ chế kho tệp WAD (`XOR 0xCC`) và định dạng tệp màn chơi `.lev` (`XOR 0xAA`).
* Biên soạn bộ hồ sơ dịch ngược 8 hệ thống tại `c:\Documents\UNITY 2026\data rea\Docs\`.

---

## 3. GIAI ĐOẠN 3: TÁI CẤU TRÚC (REFACTOR) & TÍCH HỢP 4 CƠ CHẾ CỐT LÕI VÀO STAR VANGUARD

### 3.1. Chuyển đổi Khái niệm Thuật ngữ (De-Chickenization)
* Đổi tên nguyên tử file và lớp `Food.cs` $\rightarrow$ [`EnergyCrystal.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnergyCrystal.cs) (giữ nguyên GUID `.meta` `abf85f5108b6c484c8a2e6a1cfd000e0`). Tạo lớp kế thừa tương thích ngược `public class Food : EnergyCrystal {}`.
* Áp dụng thuộc tính `[FormerlySerializedAs]` để bảo toàn dữ liệu Inspector trong Unity:
  * `m_CrystalPrefab` thay cho `m_FoodPrefab` trong [`PickupManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PickupManager.cs).
  * `m_MinCrystalDrops`, `m_MaxCrystalDrops` thay cho `m_MinFoodDrops`, `m_MaxFoodDrops` trong [`BossController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BossController.cs).
  * `m_CrystalPoolSize` trong [`PoolManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PoolManager.cs).
* Chuyển đổi toàn bộ PlayerPrefs keys sang tiền tố `StarVanguard_*` trong `LeaderboardManager.cs` và `SettingsManager.cs`.
* Chuyển đổi tên hàm và biến từ "Egg" sang "Plasma / Projectile".

### 3.2. Lập trình 4 Cơ chế REA Trích xuất

1. **Cơ chế 1: Quản lý Quá nhiệt Vũ khí (Weapon Overheat System):**
   * Tạo mới [`WeaponHeatController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/WeaponHeatController.cs).
   * Triển khai công thức: Tăng nhiệt mỗi phát $\Delta H = 0.045$, hằng số tản nhiệt $C_{cool} = 0.35\text{ s}^{-1}$, thời gian khóa súng chịu phạt $T_{penalty} \approx 2.85\text{s}$.
   * Tích hợp sự kiện `OnHeatChanged` và `OnOverheatStateChanged` vào [`PlayerShooting.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerShooting.cs).
2. **Cơ chế 2: Quỹ đạo Bay Đường Cong Cubic Bézier:**
   * Bổ sung `eEnemyMoveType.CubicBezier` vào [`eEnemyMoveType.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eEnemyMoveType.cs).
   * Lập trình hàm `InitCubicBezier()` và `handleCubicBezierWave()` trong [`EnemyMover.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnemyMover.cs), tính toán tọa độ theo thuật toán De Casteljau và tự động xoay góc phi thuyền theo tiếp tuyến vận tốc $B'(t)$.
   * Kích hoạt đợt quét lướt sóng chữ S `Wave_CubicBezier` vào chu kỳ 7 wave hoàn chỉnh trong [`WaveDirector.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/WaveDirector.cs).
3. **Cơ chế 3: Thuật toán Rơi Đồ & Bộ Đếm Bảo Hiểm (Pity Counter):**
   * Nâng cấp [`PickupManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PickupManager.cs) với ngưỡng $K_{threshold} = 15$, $K_{max} = 30$. Đảm bảo tỉ lệ rơi hộp nâng cấp vũ khí tăng tuyến tính sau 15 quái và đạt $100\%$ ở quái thứ 30.
4. **Cơ chế 4: Trùm Đa Giai Đoạn Bullet Hell & Co-op Health Scaling:**
   * Nâng cấp [`BossShooting.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BossShooting.cs) thành hệ thống Bullet Hell 3 giai đoạn:
     * **Stage 1 (> 65% HP):** Đạn tỉa đơn định hướng mục tiêu.
     * **Stage 2 (30% - 65% HP):** Bắn đạn chùm 3 hướng tỏa $-20^\circ, 0^\circ, +20^\circ$.
     * **Stage 3 (< 30% HP - Cuồng bạo):** Bão đạn xoắn ốc tỏa tròn 12 tia quay $360^\circ$ liên tục với tốc độ bắn gấp đôi!
   * Đồng bộ máu thời gian thực từ [`BossController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BossController.cs) sang `BossShooting.UpdateHpPercent()`.
   * Bổ sung hàm `ScaleForCoop(playerCount)`: $HP_{coop}(N) = HP_{base} \cdot (1 + 0.65 \cdot (N - 1))$.

---

## 4. GIAI ĐOẠN 4: CHUẨN HÓA 1080P, RE-SKIN SCI-FI & KHO VŨ KHÍ REA

### 4.1. Chuẩn hóa Màn hình 1920x1080 (16:9 Full HD)
* [`FixedAspectCamera.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/FixedAspectCamera.cs): Chuyển đổi tỉ lệ mục tiêu từ `4f / 3f` sang chuẩn `16f / 9f` (1.7778), hỗ trợ pillarbox/letterbox thích ứng đa độ phân giải.
* [`PlayerController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerController.cs): Tự động tính toán lại biên di chuyển Viewport theo camera 16:9, mở rộng không gian cơ động cho chiến cơ.

### 4.2. Giao diện Đo Quá Nhiệt & Sạc Bom Năng Lượng (Overheat HUD)
* Lập trình [`OverheatGaugeUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/OverheatGaugeUI.cs) với chuẩn Zero-GC IMGUI & tích hợp vào `UIManager.cs`:
  * Thanh hiển thị nhiệt độ súng thời gian thực chuyển đổi 3 dải màu: Xanh ngọc ($<40\%$) $\to$ Vàng hổ phách ($40\% - 75\%$) $\to$ Đỏ rực ($>75\%$).
  * Cảnh báo chớp nháy viền đỏ và thông điệp `⚠️ WEAPON JAMMED - OVERHEATED` khi súng quá nhiệt.
  * Tích hợp thanh đo tích lũy $50$ Tinh thể Năng lượng $\to$ tự động chế tạo $+1$ Quả Bom Plasma vào [`PlayerStats.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerStats.cs) và [`EnergyCrystal.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnergyCrystal.cs).

### 4.3. Re-skin Mỹ thuật Vũ trụ Hiện đại (Bảo toàn 100% GUID .meta)
* `ChickenInvadersLogo.png` $\to$ Logo neon kim loại 3D chuẩn Sci-Fi: **"STAR VANGUARD: ALIEN ONSLAUGHT"**.
* `BlueChicken_Sprite.png` $\to$ Chiến hạm **Alien Scout Drone** góc cạnh, giáp tím thẫm, rãnh năng lượng cyan và mắt cảm biến đỏ rực hướng xuống dưới.
* `ChickenBoss.png` $\to$ Tuần dương hạm **Alien Dreadnought Boss** vũ trang hạng nặng với tháp pháo đôi.
* `Egg.png` & `BrokenEgg.png` $\to$ Cầu năng lượng **Plasma Orb** và hạt nổ xung kích **Plasma Impact**.
* `Food.png` $\to$ Tinh thể năng lượng vát cạnh phát sáng **Energy Crystal**.

### 4.4. Kho Vũ khí 5 Chủng loại Tái hiện từ Nhị phân REA CI5
* Thêm Enum [`eWeaponType.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eWeaponType.cs) và nâng cấp [`PlayerShooting.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerShooting.cs):
  1. **Ion Blaster (Phím 1):** Tia kép plasma xanh ngọc cân bằng, tốc độ bắn nhanh.
  2. **Neutron Piercer (Phím 2):** Tia thương năng lượng lục xuyên giáp nhiều mục tiêu (`m_RemainingPierces`).
  3. **Vulcan Chaingun (Phím 3):** Đại liên động năng siêu thanh với thuật toán tản mát LCG PRNG từ `OutletUzi::server_onFire` ($A=214013, C=2531011$).
  4. **Laser Cannon (Phím 4):** Pháo laser đỏ hội tụ uy lực cao, cắt xé tàu địch.
  5. **Plasma Swarm (Phím 5):** Bão đạn năng lượng tím đa góc tỏa quạt diện rộng.
* Hỗ trợ bắn tự động giữ phím (`Input.GetKey(KeyCode.Space)`) và chuyển vũ khí tức thời qua phím số 1..5.

---

## 5. GIAI ĐOẠN 5: KIẾN TRÚC MẠNG CO-OP MULTIPLAYER (MIRROR LAN)

### 5.1. Bộ Điều Khiển Mạng Đồng Bộ Hóa Cốt Lõi
* [`StarVanguardNetworkManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/StarVanguardNetworkManager.cs): Quản lý vòng đời Host/Server LAN, phân bổ vị trí xuất hiện (Spawn Positions) cho tối đa 4 phi đội chiến cơ và tự động kích hoạt Co-op HP Scaling cho Boss.
* [`NetworkPlayerController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/NetworkPlayerController.cs):
  * Đồng bộ biến trạng thái `[SyncVar]` cho Chỉ số phi công (Player Index), Loại vũ khí, Điểm số và Số mạng còn lại.
  * Cơ chế đồng bộ bắn đạn bằng kiến trúc `[Command] CmdFireBullet` $\to$ `[ClientRpc(includeOwner = false)] RpcSpawnRemoteBullet`: Client bắn tức thời không độ trễ, các client khác lấy đạn từ `PoolManager` cục bộ giúp triệt tiêu chi phí vật lý trên server và loại bỏ hoàn toàn GC Alloc.
  * Phân biệt chiến cơ qua mã màu quân hàm: P1 Xanh Cyan Neon (`#00E5FF`), P2 Đỏ Thép Crimson (`#FF3B30`), P3 Lục Emerald (`#00E676`), P4 Vàng Solar (`#FFD600`).
* [`NetworkWaveSynchronizer.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/NetworkWaveSynchronizer.cs):
  * Máy chủ nắm quyền phân bổ hạt giống ngẫu nhiên `waveSeed` giúp quỹ đạo Bézier của mọi quái vật trên mọi màn hình khớp nhau 100%.
  * Tự động điều chỉnh hệ số Co-op Scaling cho Boss theo số lượng người chơi kết nối thực tế.

### 5.2. Giao Diện Phòng Chờ Mạng LAN Cybernetic
* [`NetworkLobbyUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/NetworkLobbyUI.cs):
  * Tích hợp giao diện điều khiển mạng tương lai ngay tại Menu chính (độ phân giải 1080p).
  * Hỗ trợ 3 chế độ: Chơi Đơn (Solo Mission), Tạo phòng Máy chủ (Host LAN) và Gia nhập Phi đội (Join Wingman) với ô nhập địa chỉ IP linh hoạt (`127.0.0.1` / Mạng LAN nội bộ).
  * Hiển thị trạng thái kết nối thời gian thực và hỗ trợ ngắt kết nối an toàn.
  * Tích hợp sâu vào [`UIManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UIManager.cs) và [`HomeScreenController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/HomeScreenController.cs).

## 6. GIAI ĐOẠN 6: TÁI CẤU TRÚC GIAO DIỆN 1080P, HỆ THỐNG ĐẠN & BỘ NHỚ THEO USER FEEDBACK

1. **Khóa Chuẩn Hiển Thị 1920x1080 (16:9 Full HD):**
   * Sửa lỗi hardcode `1440` trong [`GameManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/GameManager.cs) sang `1920x1080`.
   * Tạo script [`CanvasAspectFitter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/CanvasAspectFitter.cs) tự động khóa khung chữ nhật Canvas khớp với Viewport của [`FixedAspectCamera.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/FixedAspectCamera.cs), ngăn ngừa triệt để hiện tượng mất hay cắt xén Tim, Tên lửa, Điểm số trên mọi tỉ lệ màn hình (`Free Aspect`, 16:9, 16:10, 21:9).
2. **Thanh Hiển Thị Nhịp Bắn & Đo Lường Quá Nhiệt (Overheat Rhythm HUD):**
   * Nâng cấp [`OverheatGaugeUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/OverheatGaugeUI.cs) với hiệu ứng vi xung giật (micro-recoil pulse) mỗi khi bắn đạn.
   * Thanh nhiệt chuyển màu Xanh ngọc $\to$ Vàng $\to$ Đỏ rực kèm cảnh báo quá nhiệt chớp nháy và đồng hồ đếm ngược làm nguội.
   * Hiển thị thanh tiến độ tích lũy 50 Tinh thể Năng lượng đổi 1 Bom Plasma, cùng hiển thị số mạng (Hull), số bom và điểm số ở thanh HUD đỉnh màn hình.
3. **Bộ 4 Chủng Loại Đạn Sci-Fi & Đạn Đỏ Mặc Định:**
   * Khắc phục hoàn toàn lỗi đạn bị đen bằng các sprite 32-bit RGBA có lõi trắng phát sáng (White-Hot Core):
     * `Bullet_RedLaser.png`: Đạn Đỏ mặc định, chùm tia song song/bốn tia đỏ rực.
     * `Bullet_GreenNeutron.png`: Tia thương lục bảo xuyên qua quái nhỏ.
     * `Bullet_YellowVulcan.png`: Đạn động năng vàng kim bắn cực nhanh kèm tản mát LCG PRNG.
     * `Bullet_PurplePlasma.png`: Cầu năng lượng tím tỏa quạt diện rộng.
   * Nâng cấp [`PlayerShooting.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerShooting.cs) và [`Bullet.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Bullet.cs).
4. **Hệ Thống Rơi Vật Phẩm Mới (Nether Star & Hộp Quà 4 Màu):**
   * Thay biểu tượng tăng cấp hỏa lực bằng `NetherStar_Upgrade.png` (Ngôi sao Nether Star phong cách Minecraft).
   * Chuyển 4 hộp quà màu (`GiftBox_Red`, `GiftBox_Green`, `GiftBox_Yellow`, `GiftBox_Purple`) thành vật phẩm đổi sang hệ đạn tương ứng khi chạm vào chiến cơ qua [`PowerUpPickup.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PowerUpPickup.cs) và [`PickupManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PickupManager.cs).
5. **Tách Biệt 2 Scene Riêng Biệt & Redesign Toàn Bộ UI Menu:**
   * Tạo Scene [`MainMenuScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/MainMenuScene.unity): Logo Star Vanguard đặt trang trọng ở trên cao (không bị che khuất), cụm nút bấm Sci-Fi Cyberpunk mới đặt ở dưới ([`MainMenuUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/MainMenuUI.cs)).
   * Tạo Scene [`GameplayScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/GameplayScene.unity): Scene chiến đấu tập trung, không còn panel menu che chắn.
   * Đăng ký cả 2 Scene vào [`EditorBuildSettings.asset`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/ProjectSettings/EditorBuildSettings.asset).
6. **Khắc Phục Triệt Để Lỗi Rò Rỉ Đối Tượng Scene Khi Thoát (OnDestroy Object Spawn):**
   * *Hiện tượng:* Console báo lỗi `Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)` và cảnh báo lặp `PoolManager: HUD Icon pool not initialized yet!`.
   * *Nguyên nhân:* Khi thoát scene, `UIManager.OnDestroy()` gọi `PoolManager.Instance?.ReturnHUDIcon(...)`. Vì `PoolManager` đã bị hủy trong quá trình dọn dẹp scene, thuộc tính getter `Singleton.Instance` lại tự động gọi `new GameObject("PoolManager")`, tạo ra thực thể mới ngay trong lúc scene đang đóng! Thực thể này chưa kịp chạy `Start()` nên các pool đều `null`.
   * *Khắc phục:*
     * Cập nhật [`Singleton.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Singleton.cs): Bổ sung `HasInstance`, kiểm tra nghiêm ngặt `_isQuitting` và `!Application.isPlaying`, tuyệt đối không tạo mới GameObject khi ứng dụng đang đóng hoặc thoát scene.
     * Cập nhật [`UIManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UIManager.cs): Trong `OnDestroy()`, giải phóng danh sách icon nội bộ bằng `m_HeartInstances.Clear()` mà không gọi ngược lại `PoolManager.Instance`.
     * Chuyển thời điểm khởi tạo pool trong [`PoolManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PoolManager.cs) từ `Start()` sang `Awake()`, đảm bảo mọi pool sẵn sàng ngay từ khung hình đầu tiên.

---

## 7. KẾT QUẢ KIỂM TRA BIÊN DỊCH TOÀN DỰ ÁN

Dự án được kiểm tra biên dịch liên tục qua lệnh `dotnet build` trên cả hai tệp `Assembly-CSharp.csproj` và `Assembly-CSharp-Editor.csproj`:
```text
Build succeeded.
    0 Error(s)
    0 Code Warning(s)
```

---

## 8. GIAI ĐOẠN 7: TÁI CẤU TRÚC KIẾN TRÚC UI THEME, ROUTER VÀ BỘ TEST PLAY MODE (29/10/2026)

### 8.1. Tách Scene Triệt Để Bằng Công Cụ Editor Chuyên Dụng
* Tạo công cụ [`StarVanguardSceneSplitter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardSceneSplitter.cs):
  * Tự động sao lưu và phân tách độc lập hai scene:
    * [`MainMenuScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/MainMenuScene.unity): Gỡ bỏ hoàn toàn mọi đối tượng gameplay thừa (`Player`, `Boundaries`, `WaveDirector`, `InterWaveMessageManager`, `HUDContainer`), dứt điểm hoàn toàn việc quái tự spawn hay game lag ngay tại Menu.
    * [`GameplayScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/GameplayScene.unity): Gỡ bỏ toàn bộ panel UI Menu cũ, chỉ giữ lại HUD và thực thể chiến đấu.
  * Cập nhật danh sách Scene trong `EditorBuildSettings.asset`.

### 8.2. Hệ Thống Kiến Trúc UI Toàn Diện (`StarVanguard.UI`)
* [`UITheme.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UITheme.cs):
  * Single Source of Truth cho toàn bộ giao diện: Lưới chuẩn 8px, bảng typography theo độ phân giải 1920x1080 (Display 72, H1 48, H2 32, Body 24, Caption 18, Hud 36).
  * Bộ sinh Sprite 9-slice bo góc mềm (Rounded Rect) và viền Neon dạ quang (Rounded Outline) tại runtime chống vỡ hạt/răng cưa.
  * Phương thức `ApplyCanvasScaler()` khóa chuẩn Full HD 1920x1080 (Match 0.5f).
* [`UIPanelRouter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UIPanelRouter.cs):
  * Quản lý chuyển panel đơn nhất theo cơ chế Stack Navigation (`Push`, `Pop`, `NavigateTo`) kèm hiệu ứng mờ dần (cross-fade 0.12s).
  * Khắc phục 100% tình trạng mở đè 2 panel lên nhau hoặc nút Back bị che khuất.
  * Tự động phát hiện các panel trong scene: `SettingsPanel`, `ControlsPanel`, `LeaderboardPanel`, `PausePanel`, `GameOverPanel`, `NewHighScorePanel`.
* [`MainMenuScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/MainMenuScreen.cs):
  * Dựng Menu chính uGUI + TMP hiện đại thay thế hoàn toàn IMGUI cũ.
  * Hỗ trợ chọn 3 mức độ khó (CADET, VETERAN, ACE) kết nối trực tiếp `GameManager.Instance.SetSelectedDifficulty()`.
  * Cơ chế tự động dọn dẹp các canvas/panel rác bị trùng (`RemoveStaleMenus`, `DisableUnknownCanvasObjects`).
* [`ControlsScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/ControlsScreen.cs) & [`InputMap.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/InputMap.cs):
  * Khung điều khiển gọn gàng 980x720, hiển thị bảng phím động từ `InputMap` (WASD/Mũi tên, Space bắn đạn, Shift thả bom, Phím 1-4 đổi súng, Chuột).

### 8.3. Gia Cố Kiến Trúc Singleton Phân Biệt Theo Vòng Đời Scene
* [`SingletonRegistry.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/SingletonRegistry.cs):
  * Đảm nhiệm `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` dọn dẹp static data trước mỗi phiên Play Mode mà không bị lỗi compiler như khi đặt trong generic class.
  * Gom thông báo duplicate manager khi đổi scene thành 1 log duy nhất thay vì spam console.
* [`Singleton.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Singleton.cs):
  * Bổ sung attribute `[SceneBoundSingleton]` và thuộc tính ảo `PersistAcrossScenes => false` cho các manager phụ thuộc scene ([`UIManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UIManager.cs), [`WaveDirector.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/WaveDirector.cs), [`ParallaxManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/ParallaxManager.cs), [`PlayerController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerController.cs), [`InterWaveMessageManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/InterWaveMessageManager.cs)).
  * Cấm tuyệt đối việc tự động tạo mới GameObject ma (`new GameObject()`) khi truy cập `.Instance` trong lúc scene đang unload hoặc thoát game.

### 8.4. Bộ Kiểm Thử Tự Động Play Mode Đạt 48/48 Test Pass
* Tạo test suite [`StarVanguardPlayModeSmoke.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs) và công cụ audit UI [`StarVanguardUIAudit.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardUIAudit.cs).
* Thực thi kiểm thử tự động toàn diện qua Unity batchmode, xuất kết quả tại `smoke_real.log`:
  * **Kết quả:** `[STARVANGUARD-SMOKE] SUMMARY passed=48 failed=0 exitCode=0`.
  * Xác nhận toàn bộ luồng Menu $\to$ Đổi panel $\to$ Nút bấm $\to$ Chuyển cảnh `GameplayScene` $\to$ Khởi tạo Tàu người chơi $\to$ Spawn Wave 1 (40 quái) $\to$ Cơ chế bắn liên tục đạt 100% nhiệt $\to$ Khóa súng $\to$ Làm nguội thành công 100%.

---

## 9. GIAI ĐOẠN 8: TÁCH RIÊNG BOOTSTRAP SCENE & TRIỆT TIÊU BẢN COPY PERSISTENT MANAGER (29/10/2026)

### 9.1. Tạo Dedicated Bootstrap Scene (`BootstrapScene.unity`)
* Tạo [`BootstrapScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/BootstrapScene.unity) đóng vai trò là Scene khởi tạo duy nhất tại Build Index 0:
  * Nơi cư trú duy nhất của các Persistent Manager: `GameManager` (kèm `PickupManager`), `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager`.
  * Bảo toàn 100% các serialized references: danh sách Prefab của các pool (`m_EnemyPrefab`, `m_CrystalPrefab`, `m_BulletPrefab`, `m_BombPrefab`), các Audio Clip, cấu hình Slider Volume, v.v.
* Tạo [`BootstrapLoader.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BootstrapLoader.cs):
  * Khi khởi động game: Khởi chạy các persistent manager rồi ngay lập tức gọi `SceneManager.LoadScene("MainMenuScene")`.
  * Hỗ trợ tiện ích Editor `#if UNITY_EDITOR`: Tự động nạp additively `BootstrapScene` thông qua `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` nếu lập trình viên bấm Play trực tiếp trong `MainMenuScene` hoặc `GameplayScene`.

### 9.2. Triệt Tiêu Bản Copy Trong GameplayScene và MainMenuScene
* Dùng công cụ [`StarVanguardBootstrapSetup.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs) dọn sạch hoàn toàn các bản copy của 5 Persistent Manager khỏi [`GameplayScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/GameplayScene.unity) và [`MainMenuScene.unity`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scenes/MainMenuScene.unity).
* Dưới node `Managers` trong `GameplayScene`, hiện tại **chỉ còn** các Scene-bound Manager thực thụ (`UIManager`, `ParallaxRoot`, `InterWaveMessageManager`, `WaveDirector`).

### 9.3. Xóa Bỏ Hoàn Toàn Thông Báo Chuyển Giao (Handover Notification)
* Do không còn bất kỳ bản copy trùng lặp nào xuất hiện trong các scene tiếp theo:
  * Loại bỏ danh sách `s_DroppedCopies`, các phương thức `ReportDroppedCopy()` và `FlushDroppedCopies()` khỏi [`SingletonRegistry.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/SingletonRegistry.cs).
  * Trong [`Singleton.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Singleton.cs), loại bỏ lệnh gọi ghi log dropped copy.
  * Console hoàn toàn sạch bóng, không còn bất kỳ dòng log cảnh báo chuyển giao nào khi đổi scene!

### 9.4. Nghiệm Thu Play Mode Đạt 55/55 Test Pass (0 Fail)
* Nâng cấp [`StarVanguardPlayModeSmoke.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs) mở từ `BootstrapScene.unity` và kiểm tra xác nhận:
  1. `BootstrapScene` chuyển tiếp hoàn hảo sang `MainMenuScene`.
  2. `MainMenuScene` không chứa bất kỳ bản copy persistent manager cục bộ nào.
  3. `GameplayScene` không chứa bất kỳ bản copy persistent manager cục bộ nào.
  4. `GameManager` và `PoolManager` tồn tại duy nhất 1 instance và thuộc về `DontDestroyOnLoad`.
  5. **Kết quả:** `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`.

---

## 10. GIAI ĐOẠN 9: HỖ TRỢ CĂN CHỈNH TRỰC QUAN GIAO DIỆN TRONG UNITY EDITOR (29/10/2026)

### 10.1. Nguyên Nhân Gốc Rễ Màn Hình Trắng Tinh Ngoài Play Mode
* Trước đó, UI của MainMenu được dựng theo mô hình **Runtime Procedural UI** ([`MainMenuScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/MainMenuScreen.cs) gọi `Build()` trong `Start()`).
* Khi mở scene ở chế độ **Edit Mode** (ngoài Play Mode), Unity không chạy `Start()`. Cây Hierarchy trong `Canvas` hoàn toàn trống rỗng (`MenuRoot` chưa được sinh ra). Designer/Dev nhìn thấy màn hình Canvas trắng tinh, không thể click chọn các nút hay dùng công cụ Rect Tool để kéo thả, căn lề, đổi font chữ.

### 10.2. Giải Pháp Triệt Để: Dual-Mode Architecture & Menu Baker Tool
* **Cơ chế Dual-Mode trong [`MainMenuScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/MainMenuScreen.cs):**
  * Viết thêm phương thức `BindHome()` và `BindCoopPanel()`: Khi game chạy, nó sẽ tìm kiếm xem trong Scene đã có sẵn `MenuRoot` hay chưa.
  * Nếu đã có sẵn (do người dùng tự căn chỉnh trong Editor): Hệ thống giữ nguyên 100% vị trí, kích thước, màu sắc và font chữ do người dùng thiết kế, chỉ gắn sự kiện `onClick.AddListener` và đồng bộ số liệu điểm cao / độ khó.
  * Nếu chưa có (fallback): Mới tự động dựng giao diện bằng code.
  * `RemoveStaleMenus()` được nâng cấp để chỉ loại bỏ các bản copy trùng lặp ngoài ý muốn (do Ctrl+D), bảo vệ bản menu gốc trong scene.
* **Tạo Tài Nguyên Sprite 9-Slice Bền Vững:**
  * Sinh file ảnh 32-bit RGBA [`UI_Rounded.png`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Sprites/UI/UI_Rounded.png) và [`UI_RoundedOutline.png`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Sprites/UI/UI_RoundedOutline.png) kèm `.meta` cấu hình `spriteBorder: {x: 14, y: 14, z: 14, w: 14}` trong `Assets/Sprites/UI/`.
  * Cập nhật [`UITheme.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UITheme.cs) nạp trực tiếp sprite asset từ disk để các `Image` component được serialize bền vững vào scene asset mà không bị lỗi `fileID: 0`.
* **Bộ Công Cụ Editor Tiện Ích 1-Click:**
  * [`StarVanguardMenuBaker.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardMenuBaker.cs): Menu bar item `Star Vanguard -> UI -> Bake Main Menu to Scene`.
  * [`MainMenuScreenEditor.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/MainMenuScreenEditor.cs): Custom Inspector cho `MainMenuScreen` với nút bấm trực quan `Bake Menu to Scene` và thông báo hướng dẫn cụ thể.

### 10.3. Giải Quyết Triệt Để Tình Trạng "Trắng Tinh" Cho Settings, Hướng Dẫn & Co-op
* **Phát hiện lỗi:**
  * Bảng `ControlsPanel` và `CoopPanel` trước đó mặc định ở trạng thái `m_IsActive: 0` (inactive) nên không hiển thị trong Scene View nếu chưa bật.
  * Bảng `SettingsPanel` cũ bị mất/hỏng các tham chiếu Slider và Toggle âm thanh/điều khiển (`RT not found`), các container con bị tắt, dẫn đến bảng hoàn toàn trống rỗng ("trắng tinh") kể cả khi cố bật lên xem.
* **Xây dựng [`SettingsScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/SettingsScreen.cs):**
  * Dựng lại hoàn chỉnh bảng Cài Đặt Sci-Fi kích thước chuẩn 980x720: 3 thanh trượt âm lượng (Master, Music, SFX kèm % chữ số hiển thị), 3 nút gạt Toggle điều khiển (Mouse Controls, Mouse Smoothing, Keyboard Smoothing), nút Reset Defaults và nút Back.
  * Tự động liên kết hai chiều vào [`SettingsUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/SettingsUI.cs) và `SettingsManager`.
* **Thanh Công Cụ Chuyển Đổi & Xem Trước Panel 1-Click ([`UIPanelPreviewToolbar.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/UIPanelPreviewToolbar.cs)):**
  * Tích hợp thanh công cụ trực quan vào Inspector của `Canvas`, `MainMenuScreen` và `UIPanelRouter`:
    * `[🏠 Menu Chính]` `[⚙️ Cài Đặt]` `[📖 Hướng Dẫn]` `[👥 Chơi Mạng]`
  * Mỗi khi bấm vào một panel, công cụ tự động kích hoạt duy nhất panel đó và ẩn các panel còn lại, giúp người dùng nhìn thấy trọn vẹn từng bảng trên Scene View để tự do căn chỉnh vị trí/màu sắc mà không bị chồng đè.
  * Tự động reset về trạng thái chuẩn (`MenuRoot` bật, các panel con tắt) khi bước vào Play Mode.
