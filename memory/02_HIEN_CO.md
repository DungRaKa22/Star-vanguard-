# 📦 02. NHỮNG THỨ HIỆN CÓ (CURRENT ASSETS & SYSTEMS INVENTORY)

Tài liệu này kiểm kê chi tiết toàn bộ tài nguyên, cấu trúc thư mục, tệp mã nguồn C#, hệ thống logic và tài liệu đặc tả hiện đang có mặt trong không gian làm việc của dự án.

---

## 1. HỆ THỐNG MÃ NGUỒN C# (`Star Vanguard/Assets/Scripts/`)

Dự án hiện có **44 script C#** chính được phân loại theo từng nhóm chức năng:

| Nhóm chức năng | Tệp C# | Mô tả vai trò |
| :--- | :--- | :--- |
| **Quản lý Cốt lõi (Core)** | [`BootstrapLoader.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BootstrapLoader.cs) | Bộ điều khiển khởi động tại `BootstrapScene`, tự động chuyển sang `MainMenuScene` và nạp additive trong Editor. |
| | [`GameManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/GameManager.cs) | Quản lý vòng đời trò chơi, điểm số, mạng, độ khó (Cadet/Veteran/Ace) và Game State. |
| | [`Singleton.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Singleton.cs) | Base class Singleton có phân biệt `[SceneBoundSingleton]` và Persistent Singleton, chống leak object ma khi thoát scene. |
| | [`SingletonRegistry.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/SingletonRegistry.cs) | Quản lý static lifecycle, reset domain reload subsystem registration (đã loại bỏ cơ chế dropped copy do không còn bản copy). |
| | [`PoolManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PoolManager.cs) | Quản lý tái sử dụng đối tượng (Bullets, Enemies, Crystals, Explosions). |
| | [`eGameState.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eGameState.cs) | Enum trạng thái game: `Menu`, `Playing`, `Paused`, `GameOver`. |
| **Phi thuyền Người chơi** | [`PlayerController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerController.cs) | Điều khiển di chuyển, giới hạn màn hình và chuyển cảnh Hyperspace. |
| | [`PlayerShooting.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerShooting.cs) | Quản lý khai hỏa đạn, cấp độ nâng cấp đạn và kết nối tản nhiệt. |
| | [`WeaponHeatController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/WeaponHeatController.cs) | Quản lý nhiệt độ vũ khí, thời gian phạt và mở khóa an toàn. |
| | [`PlayerBombing.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerBombing.cs) | Kích hoạt bom Plasma quét sạch toàn bộ đạn và quái trên màn hình. |
| | [`PlayerHitBox.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerHitBox.cs) | Bắt va chạm với đạn quái và thân tàu địch. |
| | [`PlayerRespawn.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerRespawn.cs) | Hồi sinh tàu, kích hoạt trạng thái bất tử tạm thời (Invulnerability). |
| | [`PlayerStats.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PlayerStats.cs) | Lưu trữ số mạng, số bom và cấp độ súng. |
| **Kẻ địch & Đợt tấn công**| [`WaveDirector.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/WaveDirector.cs) | Chu kỳ 7 wave (Line, Bezier Sweeper, Arc, Dive, Columns, Pincer, Boss). |
| | [`EnemyController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnemyController.cs) | Thực thể quái thường, lượng máu, nhận sát thương và rơi vật phẩm. |
| | [`EnemyMover.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnemyMover.cs) | Bộ tính toán chuyển động: Đường thẳng, Sin, Bổ nhào, Vòng cung và **Cubic Bezier**. |
| | [`eEnemyMoveType.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eEnemyMoveType.cs) | Enum kiểu di chuyển của quái. |
| | [`EnemyShootingSimple.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnemyShootingSimple.cs)| Chu kỳ thả đạn quái ngẫu nhiên. |
| **Quái Trùm (Boss)** | [`BossController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BossController.cs) | Máu trùm, thang máu Co-op `ScaleForCoop()`, chuỗi phát nổ. |
| | [`BossShooting.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BossShooting.cs) | Bão đạn 3 giai đoạn: Nhắm bắn $\to$ Tỏa 3 hướng $\to$ Xoắn ốc 12 tia $360^\circ$. |
| | [`BossMover.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/BossMover.cs) | Quỹ đạo bay qua lại trên đỉnh màn hình của Boss. |
| **Vật phẩm & Vũ khí** | [`EnergyCrystal.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnergyCrystal.cs) | Tinh thể năng lượng rơi ra từ quái (thay thế đùi gà cũ). |
| | [`PickupManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PickupManager.cs) | Điều phối rơi tinh thể và hộp quà với thuật toán Pity Counter REA. |
| | [`PowerUpPickup.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/PowerUpPickup.cs) | Hộp quà nâng cấp cấp độ đạn phi thuyền. |
| | [`Bullet.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Bullet.cs) | Đạn của người chơi bay lên trên. |
| | [`EnemyBullet.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/EnemyBullet.cs) | Đạn của quái/trùm bay xuống theo hướng góc. |
| | [`Bomb.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Bomb.cs) | Thực thể bom kích nổ toàn màn hình. |
| **Giao diện & Âm thanh** | [`UIManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UIManager.cs) | Hiển thị HUD điểm số, mạng, bom và các Panel giao diện. |
| | [`AudioManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/AudioManager.cs) | Phát nhạc nền BGM, âm thanh SFX, fade nhạc trước Boss. |
| | [`eSFXId.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eSFXId.cs) | Danh mục âm thanh SFX. |
| | [`eMusic.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eMusic.cs) | Danh mục nhạc nền BGM. |
| | [`FixedAspectCamera.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/FixedAspectCamera.cs) | Giữ chuẩn tỉ lệ khung hình camera 16:9 (`1920x1080` Full HD) thích ứng mọi màn hình. |
| | [`CanvasAspectFitter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/CanvasAspectFitter.cs) | Neo giữ toàn bộ Canvas UI nằm trọn vẹn trong Viewport Camera 16:9, chống tràn viền trên mọi tỉ lệ. |
| | [`OverheatGaugeUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/OverheatGaugeUI.cs) | Hiển thị thanh Quá nhiệt vũ khí 3 dải màu, nhịp giật bắn súng (cadence), cảnh báo quá nhiệt và tiến độ 50 Tinh thể -> Bom Plasma. |
| | [`MainMenuScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/MainMenuScreen.cs) | Điều phối Menu chính, hỗ trợ Dual-Mode (tự động nhận diện UI đã căn chỉnh trong Scene hoặc tự sinh fallback). |
| | [`ControlsScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/ControlsScreen.cs) | Khung điều khiển & hướng dẫn phím bấm, hỗ trợ Bind sự kiện vào đối tượng Scene. |
| | [`SettingsScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/SettingsScreen.cs) | Khung Cài đặt hoàn chỉnh (3 Slider âm lượng Master/Music/SFX, 3 Toggle điều khiển, Reset Defaults và Back) chuẩn UITheme. |
| | [`UIPanelRouter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UIPanelRouter.cs) | Router chuyển đổi qua lại giữa các Panel duy nhất, hỗ trợ Back-stack và Fade chuyển cảnh. |
| | [`UITheme.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UITheme.cs) | Bộ quy chuẩn thông số UI, màu sắc, font typography và nạp sprite 9-slice bo góc. |
| **Công cụ Editor (Assets/Editor/)** | [`StarVanguardMenuBaker.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardMenuBaker.cs) | Công cụ 1-Click bake toàn bộ UI Menu (Menu, Settings, Controls, Co-op) vào Scene. |
| | [`MainMenuScreenEditor.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/MainMenuScreenEditor.cs) | Custom Inspector cho `MainMenuScreen` kèm nút bấm và thông báo hướng dẫn trực quan. |
| | [`UIPanelRouterEditor.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/UIPanelRouterEditor.cs) | Custom Inspector cho `UIPanelRouter` tích hợp thanh chuyển đổi panel. |
| | [`UIPanelPreviewToolbar.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/UIPanelPreviewToolbar.cs) | Thanh công cụ chuyển đổi & xem trước từng giao diện trong Editor 1-click không chồng đè. |
| | [`StarVanguardBootstrapSetup.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs) | Thiết lập BootstrapScene và dọn sạch duplicate persistent managers. |
| | [`StarVanguardPlayModeSmoke.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs) | Bộ test tự động 55 assertions kiểm thử Play Mode không lỗi. |
| | [`eWeaponType.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/eWeaponType.cs) | Định nghĩa 4 chủng loại vũ khí Sci-Fi (Red Laser, Green Neutron, Yellow Vulcan, Purple Plasma). |
| | [`ParallaxManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/ParallaxManager.cs) | Điều khiển các lớp cuộn nền vũ trụ và tăng tốc Hyperspace. |
| | [`ParallaxLayer.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/ParallaxLayer.cs) | Từng lớp nền trôi vô tận. |
| **Kiến trúc UI Mới (`StarVanguard.UI`)** | [`UITheme.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UITheme.cs) | Design System chuẩn 1080p, lưới 8px, bảng kiểu chữ, bộ sinh sprite 9-slice bo góc và viền dạ quang runtime. |
| | [`UIPanelRouter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/UIPanelRouter.cs) | Bộ định tuyến panel ngăn xếp (Stack Router), fade chuyển cảnh, chống mở trùng panel và đảm bảo nút Back hoạt động chính xác. |
| | [`MainMenuScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/MainMenuScreen.cs) | Giao diện Menu chính uGUI/TMP hoàn chỉnh, chọn độ khó Cadet/Veteran/Ace, dọn dẹp canvas trùng lặp. |
| | [`ControlsScreen.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/ControlsScreen.cs) | Khung điều khiển & hướng dẫn phím 980x720, không che nút bấm. |
| | [`InputMap.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/UI/InputMap.cs) | Bảng tra cứu phím chuẩn duy nhất cho bàn phím và chuột. |
| **Mạng Co-op LAN** | [`StarVanguardNetworkManager.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/StarVanguardNetworkManager.cs) | Quản lý kết nối Host/Client, spawn vị trí Co-op và tự động scale máu Boss. |
| | [`NetworkPlayerController.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/NetworkPlayerController.cs) | Đồng bộ tàu người chơi (P1-P4 mã màu), đồng bộ bắn đạn không độ trễ qua PoolManager cục bộ, SyncVar điểm và mạng. |
| | [`NetworkWaveSynchronizer.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/NetworkWaveSynchronizer.cs) | Đồng bộ hạt giống màn chơi `waveSeed` giúp quỹ đạo Bézier của mọi quái vật khớp 100%. |
| | [`NetworkLobbyUI.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Scripts/Networking/NetworkLobbyUI.cs) | Giao diện phòng chờ Co-op LAN tương lai (Solo / Host / Join / Disconnect / IP Customization). |
| **Công cụ Editor & Testing** | [`StarVanguardBootstrapSetup.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardBootstrapSetup.cs) | Công cụ tạo BootstrapScene, dọn sạch bản copy persistent managers và loại bỏ dead listeners. |
| | [`StarVanguardSceneSplitter.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardSceneSplitter.cs) | Công cụ phân tách scene MainMenu và Gameplay, dọn sạch object thừa. |
| | [`StarVanguardUIAudit.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardUIAudit.cs) | Công cụ dump và kiểm tra cây phân cấp UI, rect bounds và raycast target. |
| | [`StarVanguardPlayModeSmoke.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardPlayModeSmoke.cs) | Bộ kiểm thử tự động headless Play Mode mở từ BootstrapScene đạt 55/55 test pass. |
| | [`StarVanguardValidator.cs`](file:///c:/Documents/UNITY%202026/Star%20Vanguard/Assets/Editor/StarVanguardValidator.cs) | Kiểm tra tính toàn vẹn asset và cấu hình build scene. |

---

## 2. KHO TÀI NGUYÊN ĐỒ HỌA & ÂM THANH (`Star Vanguard/Assets/`)

### 2.1. Sprites Hiện Tại (`Assets/Sprites/`)
* **Logo Game Mới:** `ChickenInvadersLogo.png` $\to$ Logo neon kim loại tương lai **"STAR VANGUARD: ALIEN ONSLAUGHT"**.
* **Phi thuyền Người chơi:** `Player_Sprite.png` (Tàu chiến màu trắng/xanh, hỗ trợ nhuộm màu theo mã quân hàm Co-op P1-P4).
* **Mục tiêu Quân đoàn Alien (Đã Re-skin):**
  * `BlueChicken_Sprite.png` $\to$ **Alien Scout Drone** góc cạnh, mắt đỏ rực.
  * `ChickenBoss.png` $\to$ **Alien Dreadnought Boss** chiến hạm bọc thép khổng lồ với tháp pháo hạng nặng.
  * `Egg.png`, `BrokenEgg.png` $\to$ Cầu năng lượng **Plasma Orb** và hạt nổ xung kích **Plasma Impact**.
  * `Food.png` $\to$ Tinh thể năng lượng vát cạnh phát sáng **Energy Crystal**.
* **Đạn & Hiệu ứng (Mới tạo):**
  * `Bullets/Bullet_RedLaser.png` (Đạn Đỏ mặc định rực rỡ có lõi trắng).
  * `Bullets/Bullet_GreenNeutron.png` (Tia thương lục bảo xuyên quái nhỏ).
  * `Bullets/Bullet_YellowVulcan.png` (Đạn động năng vàng kim bắn siêu tốc).
  * `Bullets/Bullet_PurplePlasma.png` (Cầu năng lượng tím tỏa quạt diện rộng).
  * `Lazer.png` (Đã cập nhật sang đạn đỏ lõi trắng).
  * `RedLazer.png` (Đạn kẻ địch).
  * `Shield.png` (Vòng bảo vệ khiên năng lượng).
* **Vật phẩm Rơi (Mới tạo):**
  * `Pickups/NetherStar_Upgrade.png` (Nether Star phong cách Minecraft tăng cấp hỏa lực).
  * `Pickups/GiftBox_Red.png` (Hộp quà đổi sang Đạn Đỏ).
  * `Pickups/GiftBox_Green.png` (Hộp quà đổi sang Đạn Lục).
  * `Pickups/GiftBox_Yellow.png` (Hộp quà đổi sang Đạn Vàng).
  * `Pickups/GiftBox_Purple.png` (Hộp quà đổi sang Đạn Tím).
* **Giao diện Nút Bấm & Khung Viền Sci-Fi (Mới tạo):**
  * `UI/Button_SciFi_Normal.png`, `Button_SciFi_Hover.png`, `Button_SciFi_Pressed.png`.
  * `UI/UI_Rounded.png` (Ảnh 9-slice bo góc nền nút bấm và panel).
  * `UI/UI_RoundedOutline.png` (Khung viền neon phát sáng 9-slice).
* **Scenes Trò Chơi:**
  * `Assets/Scenes/BootstrapScene.unity` (Scene Khởi động duy nhất & Nơi cư trú của Persistent Managers - Build Index 0).
  * `Assets/Scenes/MainMenuScene.unity` (Scene Menu chính biệt lập - Build Index 1, 0 persistent manager copies).
  * `Assets/Scenes/GameplayScene.unity` (Scene Chiến đấu không gian tập trung - Build Index 2, 0 persistent manager copies).
* **Nền vũ trụ Parallax (Chuẩn rộng 1920px+):**
  * `BlueNebula.png` (Tinh vân xanh thẳm).
  * `Stars Small_1.png` (4096x4096px - dải sao xa siêu nét).
  * `Stars-Big_1.png` (2048x2048px - các chòm sao lớn trôi chậm).

### 2.2. Âm thanh Hiện Tại (`Assets/Audio/`)
* Nhạc nền chủ đề `eMusic.Theme`, nhạc đấu trùm `eMusic.Boss`.
* Hiệu ứng âm thanh `eSFXId`: `PlayerShoot`, `EnemyShoot`, `Explosion`, `BossHit`, `PowerUp`, `NewRound`...

---

## 3. KHO DỮ LIỆU DỊCH NGƯỢC REA ĐÃ XÁC MINH (`c:\Documents\UNITY 2026\data rea\`)

Toàn bộ thông số nhị phân trích xuất từ `CI5demo.exe` nằm trong `data rea/Docs/`:
1. **Báo cáo tổng hợp kiến trúc:** [`10_reverse_engineering_report.md`](file:///c:/Documents/UNITY%202026/data%20rea/Docs/10_reverse_engineering_report.md) (30 KB).
2. **Hệ thống Quá nhiệt:** Tăng nhiệt $\Delta H = 0.045$, tản nhiệt $C_{cool} = 0.35\text{ s}^{-1}$, ngưỡng khóa súng $T_{penalty} \approx 2.85\text{s}$ ([`02_projectile_and_weapon.md`](file:///c:/Documents/UNITY%202026/data%20rea/Docs/systems/02_projectile_and_weapon.md)).
3. **Quỹ đạo Bézier De Casteljau:** Công thức $B(t)$ nội suy 3 cấp từ 4 điểm kiểm soát $P_0, P_1, P_2, P_3$ ([`03_enemy_spawning_and_movement.md`](file:///c:/Documents/UNITY%202026/data%20rea/Docs/systems/03_enemy_spawning_and_movement.md)).
4. **Cơ chế Thức ăn $\to$ Tên lửa:** Đúng **50 đơn vị thịt/năng lượng** sẽ kích hoạt thưởng **+1 Bom/Tên Lửa** ([`05_score_and_lives.md`](file:///c:/Documents/UNITY%202026/data%20rea/Docs/systems/05_score_and_lives.md)).
5. **Cấu trúc Màn chơi 120 Wave:** $Chapter = Wave / 10$, $SubWave = Wave \pmod{10}$, Hyperspace sau Wave 10 ([`06_level_progression.md`](file:///c:/Documents/UNITY%202026/data%20rea/Docs/systems/06_level_progression.md)).
6. **Mô hình Trùm 3 Giai đoạn:** 100% - 65% (Tỉa đơn) $\to$ 65% - 30% (Chùm 3 hướng) $\to$ < 30% (Xoắn ốc 12 tia $360^\circ$ quay liên tục).
