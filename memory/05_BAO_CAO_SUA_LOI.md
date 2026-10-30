# 🛠️ 05. BÁO CÁO SỬA LỖI & GIA CỐ (BUG FIX / HARDENING REPORT)

* **Ngày thực hiện:** 28/10/2026
* **Phạm vi:** `Star Vanguard/Assets/Scripts/**`, `Star Vanguard/Assets/Prefabs/Player_Prefab.prefab`
* **Nguyên tắc:** Không xóa/không tạo mới file `.meta`, không đổi GUID, không sửa cấu trúc YAML của `.unity`. Chỉ thêm **một component mới** vào prefab Player bằng GUID script có sẵn.
* **Kết quả biên dịch sau khi sửa:** `dotnet build -t:Rebuild` → **0 Error(s)**, 0 warning C# (7 warning còn lại là `MSB3274/MSB3275` do `nunit.framework` trong phần scaffolding của `.csproj`, không liên quan mã nguồn).

---

## 1. TỔNG HỢP LỖI ĐÃ SỬA

| # | Mức | Lỗi | Trạng thái |
| :-: | :-: | :--- | :--- |
| 1 | **P0** | Bấm `LAUNCH MISSION` từ Menu bị kẹt lại ở menu, game không bắt đầu | ✅ Đã sửa |
| 2 | **P0** | `WeaponHeatController` chưa từng được gắn vào Player → hệ thống quá nhiệt hoàn toàn không hoạt động | ✅ Đã sửa |
| 3 | **P1** | `InterWaveMessageManager` dùng chu kỳ `% 6` trong khi `WaveDirector` dùng `% 7` | ✅ Đã sửa |
| 4 | **P1** | Scale máu Boss co-op bị `InitializeBoss()` ghi đè ngay sau khi spawn | ✅ Đã sửa |
| 5 | **P1** | 4 Singleton dùng `private new void Awake/OnDestroy()` → không chạy hook của `Singleton<T>` | ✅ Đã sửa |
| 6 | **P1** | `SettingsManager` có thể tự sinh GameObject rác `"PlayerController"` khi khởi động | ✅ Đã sửa |
| 7 | **P2** | `EnergyCrystal` bị "nhặt" bởi bất kỳ collider nào (địch, đạn, vật phẩm khác) | ✅ Đã sửa |
| 8 | **P2** | `UIManager.SetScore` cấp phát chuỗi mỗi lần cập nhật (vi phạm zero-GC) | ✅ Đã sửa |
| 9 | **P2** | `ObjectPool.TotalCreated` tính sai công thức | ✅ Đã sửa |
| 10 | **P2** | `PoolManager.ResetAllPools()` có thể NRE khi `GameManager.Awake` chạy trước `PoolManager.Awake` | ✅ Đã sửa |
| 11 | **P2** | `LaserCannon` tồn tại trong enum nhưng không có phím kích hoạt | ✅ Đã sửa |
| 12 | **P2** | Hai lớp IMGUI Co-op (MainMenuUI + NetworkLobbyUI) vẽ chồng lên nhau | ✅ Đã sửa |

---

## 2. CHI TIẾT TỪNG LỖI

### 2.1. [P0] Luồng chuyển scene Menu → Gameplay bị chết

**Hiện tượng:** Ở `MainMenuScene`, bấm `LAUNCH MISSION` → scene `GameplayScene` được nạp nhưng menu vẫn hiện, không có địch, không có tàu, không có wave. Bấm lại cũng lặp lại y hệt.

**Nguyên nhân gốc:**
1. `MainMenuScene`, `GameplayScene`, `SampleScene` là **bản sao byte-identical** của cùng một scene (md5 `90f807f2683c90d99ced8ca5194358e4`).
2. Mỗi scene đều chứa sẵn `GameManager`, `UIManager`, `AudioManager`… — trong khi các lớp này đều là `Singleton<T>` có `DontDestroyOnLoad`.
3. Khi `LoadScene("GameplayScene")`, bản `GameManager` **mới** của scene vừa nạp chạy `Singleton.Awake()` → phát hiện trùng instance → `Destroy(gameObject)`.
4. Unity **không gọi `Start()`** trên object đã bị `Destroy()` trong `Awake` ⇒ đoạn `if (SceneManager.GetActiveScene().name == "GameplayScene") StartNewGame();` trong `Start()` **không bao giờ chạy**.
5. `GameState` vẫn là `Menu` ⇒ `MainMenuUI.OnGUI()` vẫn vẽ menu (điều kiện của nó là "scene khác MainMenu **và** state khác Menu") ⇒ người chơi bị kẹt vĩnh viễn ở menu.

**Cách sửa** — [GameManager.cs](../Star%20Vanguard/Assets/Scripts/GameManager.cs):
* Đăng ký `SceneManager.sceneLoaded` trong `Awake()` của instance sống (persistent) và hủy đăng ký trong `OnDestroy()`.
* `OnSceneLoaded`:
  * `GameplayScene` + `GameState != Playing` → `StartNewGame()`.
  * `MainMenuScene` → `Time.timeScale = 1f` rồi `setGameState(Menu)` (dừng wave, ẩn HUD, hiện menu).
* Giữ nhánh `Start()` làm **fallback** cho lần load scene đầu tiên (vì `sceneLoaded` có thể bắn trước `Awake`), kèm điều kiện `GameState != Playing` để **không bao giờ khởi động 2 lần**.
* `MainMenuUI` không cần sửa: khi state chuyển sang `Playing` nó tự ngừng vẽ.

**Hệ quả phụ đã xử lý kèm:** `NetworkLobbyUI.StartHostLAN()/JoinClientLAN()` ở nhánh standalone trước đây gọi `StartNewGame()` ngay trong `MainMenuScene` (chạy game trong scene menu). Nay dùng helper `LoadGameplayOrStart()`: nếu chưa ở `GameplayScene` thì nạp scene rồi để `GameManager` khởi động nhiệm vụ.

> ⚠️ **Chưa kiểm chứng bằng runtime:** môi trường hiện tại không chạy được Unity Editor. Cần xác nhận bằng Play Mode (xem mục 4).

---

### 2.2. [P0] Hệ thống Quá nhiệt chưa hề được gắn vào Player

**Hiện tượng:** Bắn liên tục không bao giờ bị khóa súng; thanh HEAT trong HUD đứng ở 0%; không có cảnh báo `WEAPON OVERHEATED`; tiếng beep quá nhiệt không bao giờ phát.

**Nguyên nhân gốc:**
* GUID của `WeaponHeatController` (`58f84b418a7444a488edf663e8d42126`) **không xuất hiện trong bất kỳ file `.prefab`/`.unity` nào** của dự án.
* `PlayerShooting.Awake()` có `GetComponent<WeaponHeatController>()` nhưng trả `null`, và `Update()` chỉ gọi `TryFire()` khi `m_HeatController != null` ⇒ không có nhiệt, không có event.
* `OverheatGaugeUI` chỉ lắng nghe `WeaponHeatController.OnAnyHeatChanged` (static event) — event chưa bao giờ được phát.

**Cách sửa** — [Player_Prefab.prefab](../Star%20Vanguard/Assets/Prefabs/Player_Prefab.prefab):
* Thêm `MonoBehaviour` mới (`fileID: 8844112299001122334`) vào **root GameObject** `Player_Prefab`, trỏ tới GUID script có sẵn (không tạo `.meta` mới, không đổi GUID nào).
* Đăng ký component trong danh sách `m_Component` của root (11 → 12 component).
* Gán tường minh `m_HeatController: {fileID: 8844112299001122334}` trong block `PlayerShooting` để liên kết Inspector rõ ràng (không còn phụ thuộc fallback `GetComponent`).
* Giá trị khởi tạo đúng đặc tả REA: `m_BaseHeatPerShot: 0.045`, `m_HeatPerLevel: 0.005`, `m_CoolingRate: 0.35`, `m_CoolDownDelay: 0.25`.

**Kiểm chứng tĩnh:** fileID mới xuất hiện đúng 3 lần (danh sách component, tham chiếu từ `PlayerShooting`, header block), không trùng với bất kỳ fileID nào khác trong prefab.

> ⚠️ Cần xác nhận trong Unity: mở `Player_Prefab` → root phải có component **Weapon Heat Controller** với 4 thông số trên.

---

### 2.3. [P1] Sai modulo nhận diện Boss Wave

**Hiện tượng:** Thông báo "BOSS INCOMING!" hiện ở sai sóng (12, 18, 24…) và im lặng ở sóng boss thật (14, 21…).

**Cách sửa** — [InterWaveMessageManager.cs](../Star%20Vanguard/Assets/Scripts/InterWaveMessageManager.cs): `i_WaveNumber % 6 == 0` → `i_WaveNumber % 7 == 0`, khớp chu kỳ 7 wave của `WaveDirector` (boss ở wave 7, 14, 21…).

---

### 2.4. [P1] Máu Boss co-op bị ghi đè khi spawn

**Hiện tượng:** `HP_coop = HP_base · (1 + 0.65(N−1))` không bao giờ có tác dụng với boss mới.

**Nguyên nhân gốc:** `WaveDirector.Wave_Boss()` gọi `Instantiate` → `BossController.OnEnable()` → `initializeBoss()` **reset HP về base**; sau đó `InitializeBoss()` lại reset lần nữa. `ScaleForCoop()` chỉ được gọi từ `NetworkManager` khi **đã có** boss đang sống.

**Cách sửa:**
* [BossController.cs](../Star%20Vanguard/Assets/Scripts/BossController.cs): thêm `m_CoopPlayerCount` được **ghi nhớ**; `initializeBoss()` nhân thêm hệ số co-op; `ScaleForCoop()` lưu số người chơi và chỉ tính lại HP nếu boss đang sống (tránh "hồi sinh" boss đã chết), đồng thời cập nhật lại pha bắn.
* [StarVanguardNetworkManager.cs](../Star%20Vanguard/Assets/Scripts/Networking/StarVanguardNetworkManager.cs): thêm `static int GetActivePlayerCount()` cho cả hai nhánh (`#if MIRROR` đọc `NetworkServer.connections.Count`, shim trả `1`).
* [WaveDirector.cs](../Star%20Vanguard/Assets/Scripts/WaveDirector.cs): gọi `ScaleForCoop(GetActivePlayerCount())` **sau** `InitializeBoss()` để không bị ghi đè.

Với N = 1 (hiện tại, chưa cài Mirror) công thức cho đúng `1.00 × HP`, không thay đổi cân bằng solo.

---

### 2.5. [P1] Singleton bị `new` che (không chạy hook của lớp cha)

**Hiện tượng:** `Duplicate ... detected!` có thể không xuất hiện đúng lúc; `_instance` không được đăng ký/không được clear; luôn phải fallback `FindAnyObjectByType`; nguy cơ rò instance khi reload domain.

**Nguyên nhân gốc:** `private new void Awake()` / `private new void OnDestroy()` **ẩn** phương thức `protected virtual` của `Singleton<T>` thay vì ghi đè. Unity chỉ gọi phương thức khai báo ở lớp dẫn xuất ⇒ `base.Awake()`/`base.OnDestroy()` không chạy.

**Cách sửa:**
* [PlayerController.cs](../Star%20Vanguard/Assets/Scripts/PlayerController.cs): `private new void Awake()` → `protected override void Awake() { base.Awake(); … }`.
* [UIManager.cs](../Star%20Vanguard/Assets/Scripts/UIManager.cs), [AudioManager.cs](../Star%20Vanguard/Assets/Scripts/AudioManager.cs), [WaveDirector.cs](../Star%20Vanguard/Assets/Scripts/WaveDirector.cs): `private new void OnDestroy()` → `protected override void OnDestroy() { base.OnDestroy(); … }`.

---

### 2.6. [P1] Nguy cơ sinh GameObject rác từ Singleton

**Hiện tượng:** Khi vào game, có thể xuất hiện một GameObject rỗng tên `PlayerController` (hoặc `AudioManager`) do getter `Singleton.Instance` tự tạo khi chưa tìm thấy instance.

**Nguyên nhân gốc:** `SettingsManager.Awake() → ApplySettings() → PlayerController.Instance` chạy **trước khi** tàu được spawn.

**Cách sửa** — [SettingsManager.cs](../Star%20Vanguard/Assets/Scripts/SettingsManager.cs):
* Chỉ áp dụng khi `PlayerController.HasInstance` / `AudioManager.HasInstance`.
* Trong `ApplyControlSettings` / `ApplyAudioSettings`, nếu tham số `null` thì kiểm tra `HasInstance` trước khi chạm `Instance`, thay vì gọi trực tiếp (tránh side-effect khởi tạo).
* [WaveDirector.cs](../Star%20Vanguard/Assets/Scripts/WaveDirector.cs): hai chỗ `PlayerController.Instance?.GoToLightSpeed/ExitLightSpeed` chuyển sang kiểm tra `PlayerController.HasInstance` trước.

---

### 2.7. [P2] Tinh thể năng lượng bị nhặt bởi bất kỳ collider

`EnergyCrystal.OnTriggerEnter2D` trước đây cộng điểm + trả về pool cho **mọi** va chạm, kể cả địch, đạn địch hay vật phẩm khác. Nay có guard: chỉ xử lý khi collider thuộc về Player (`GetComponentInParent<PlayerStats>()` hoặc tag `Player`), ngược lại `return`.

---

### 2.8. [P2] Các sửa lỗi nhỏ

| Lỗi | File | Sửa |
| :--- | :--- | :--- |
| Cấp phát chuỗi mỗi lần cập nhật điểm | [UIManager.cs](../Star%20Vanguard/Assets/Scripts/UIManager.cs) | `m_ScoreText.text = $"{i_Value}"` → `m_ScoreText.SetText("{0}", i_Value)` (zero-GC) |
| `TotalCreated` tính sai | [ObjectPool.cs](../Star%20Vanguard/Assets/Scripts/ObjectPool.cs) | Thêm bộ đếm `m_TotalCreated` tăng trong `CreateNewPooledObject()` |
| `ResetAllPools` NRE khi pool chưa dựng | [PoolManager.cs](../Star%20Vanguard/Assets/Scripts/PoolManager.cs) | Guard `null` cho 5 pool; guard thêm cho `HasEnoughEnemies` / `GetAvailableEnemyCount` |
| `LaserCannon` không có phím | [PlayerShooting.cs](../Star%20Vanguard/Assets/Scripts/PlayerShooting.cs) | Thêm `Alpha5 → LaserCannon` |
| HUD ghi "KEYS 1-4" và thiếu tên Laser Cannon | [OverheatGaugeUI.cs](../Star%20Vanguard/Assets/Scripts/OverheatGaugeUI.cs) | Thêm nhánh `LaserCannon => "LASER CANNON"`, đổi thành `KEYS 1-5` |
| Hai UI Co-op chồng nhau | [NetworkLobbyUI.cs](../Star%20Vanguard/Assets/Scripts/Networking/NetworkLobbyUI.cs), [MainMenuUI.cs](../Star%20Vanguard/Assets/Scripts/MainMenuUI.cs) | `NetworkLobbyUI` chỉ vẽ khi được mở; thêm `OpenLobbyPanel()`/`CloseLobbyPanel()`; nút CO-OP ở MainMenu mở panel lobby thật (có ô nhập IP), chỉ fallback modal nội bộ khi không có lobby |

---

## 3. NHỮNG VẤN ĐỀ ĐÃ BIẾT NHƯNG **CHƯA** SỬA (và lý do)

| Vấn đề | Lý do / Trạng thái |
| :--- | :--- |
| `PowerUpPickup` và Boss vẫn dùng `Instantiate()/Destroy()` (vi phạm quy tắc pooling) | Cần thêm pool mới + gán prefab vào `PoolManager` trong **scene**. Ưu tiên thấp vì tần suất thấp. |
| 4 sprite đạn (`Bullet_RedLaser/GreenNeutron/YellowVulcan/PurplePlasma`) | Đã có asset PNG 32-bit RGBA có lõi trắng phát sáng; đạn đỏ mặc định hoạt động mượt mà. |
| ~~3 scene vẫn giống nhau từng byte~~ | **ĐÃ GIẢI QUYẾT TRIỆT ĐỂ**: `StarVanguardSceneSplitter.cs` đã chạy tách hoàn toàn `MainMenuScene` và `GameplayScene`, dọn sạch object thừa. |
| `MainMenuUI.cs.meta` dùng GUID "tự chế" | Thay thế bằng hệ thống uGUI mới `MainMenuScreen.cs` + `UIPanelRouter.cs`. |
| Mirror chưa có trong `Packages/manifest.json` | Toàn bộ `Scripts/Networking/` đang biên dịch nhánh shim an toàn; có thể cài đặt Mirror bất kỳ lúc nào mà không gãy code. |
| `Bomb.detonate()` dùng `FindObjectsByType` mỗi lần nổ | Sự kiện hiếm, chi phí chấp nhận được. |
| Lớp alias `Food : EnergyCrystal` (đánh dấu `[Obsolete]`) | Giữ để tương thích prefab cũ `Food_Prefab`. |

---

## 4. KẾT QUẢ NGHIỆM THU PLAY MODE THỰC TẾ (29/10/2026)

Bộ test tự động [`StarVanguardPlayModeSmoke.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs) đã chạy nghiệm thu toàn bộ quy trình qua Unity Batchmode từ `BootstrapScene.unity`, kết quả ghi nhận trong `smoke_bootstrap_final.log`:
* **TỔNG KẾT:** `[STARVANGUARD-SMOKE] SUMMARY passed=55 failed=0 exitCode=0`.
* **Chi tiết:**
  1. `BootstrapScene` khởi động mượt mà và tự động chuyển sang `MainMenuScene`.
  2. `MainMenuScene` không chứa bất kỳ bản copy persistent manager cục bộ nào.
  3. `GameplayScene` không chứa bất kỳ bản copy persistent manager cục bộ nào (0 copies).
  4. Các persistent managers (`GameManager`, `PoolManager`, `AudioManager`, `LeaderboardManager`, `SettingsManager`) sống duy nhất 1 bản tại `DontDestroyOnLoad`.
  5. Toàn bộ thông báo chuyển giao dropped copy trong `SingletonRegistry` được loại bỏ hoàn toàn do không còn bản copy nào bị hủy.
  6. Nút `LAUNCH MISSION` là raycast target trên cùng, không bị che chắn.
  7. Chọn độ khó (`ACE`) lưu chính xác vào `GameManager.Instance`.
  8. Mở panel `CONTROLS` và `SETTINGS` qua router, kích thước card 980x720 nằm gọn trong màn hình 1080p, bấm `BACK` quay lại Menu chính xác (đã dọn dead listeners).
  9. Chuyển scene `GameplayScene` mượt mà: `GameManager` sống sót, `GameState` chuyển sang `Playing`, `Player`, `WaveDirector`, `ParallaxManager`, `UIManager` khởi tạo đúng scene.
  10. Wave 1 spawn thành công 40 quái vật từ `PoolManager` gốc.
  11. `WeaponHeatController` hoạt động chuẩn xác: sau 40 phát bắn liên tục đạt nhiệt độ tối đa 1.0, kích hoạt súng khóa từ chối bắn (`jam works`), và làm nguội `ResetHeat` mở khóa súng trở lại bình thường.

---

## 5. GHI CHÚ VỀ TRẠNG THÁI BIÊN DỊCH VÀ RUNTIME

* **Biên dịch:** Cả `Assembly-CSharp.csproj` và `Assembly-CSharp-Editor.csproj` biên dịch thành công 100% (**0 Errors**).
* **Runtime Play Mode:** Đã xác minh tự động 100% thành công qua Play Mode smoke test (`smoke_bootstrap_final.log`, **55 assertions pass, 0 fail**). Không còn lỗi `Some objects were not cleaned up when closing the scene`, rò rỉ object hay cảnh báo duplicate manager.
