# 🚀 KẾ HOẠCH PHÁT TRIỂN & CHUYỂN ĐỔI ĐỒ ÁN (BTL UNITY)

* **Tên đề tài đăng ký:** Nghiên cứu công cụ Unity và xây dựng trò chơi 2D Space Shooter: Chiến Cơ Không Gian (Star Vanguard)
* **Ứng dụng các mẫu thiết kế (Design Patterns):** Singleton, Object Pooling, State Machine, Observer (C# Events), Interface-Driven Design (`IDamageable`, `IPoolable`).
* **Phiên bản Engine:** Unity 6 (`6000.0.54f1` trở lên) - Universal Render Pipeline (URP).

---

## 📅 GIAI ĐOẠN 1: KHỞI TẠO & LÀM SẠCH DẤU VẾT DỰ ÁN GỐC (NGÀY 1 - 2)

Mục tiêu của giai đoạn này là biến kho mã nguồn gốc thành project cá nhân hóa 100%, không còn tên tác giả cũ hay từ khóa liên quan đến "Chicken Invaders".

- [ ] **Bước 1.1: Xóa lịch sử Git cũ và khởi tạo Repo mới**
  ```bash
  # Chạy trong Terminal tại thư mục gốc của project
  rm -rf .git
  git init
  git add .
  git commit -m "Initial project structure for Star Vanguard"
  ```
- [ ] **Bước 1.2: Thay đổi thông tin định danh trong Unity**
  - Vào `Edit` -> `Project Settings` -> `Player`:
    - **Company Name:** Đổi thành tên bạn hoặc tên nhóm (VD: `TienDungStudio`).
    - **Product Name:** Đổi thành `Star Vanguard - Chien Co Khong Gian`.
    - **Default Icon:** Thay icon con gà bằng icon Phi thuyền vũ trụ.
- [ ] **Bước 1.3: Đổi tên định danh trong Code (Refactoring bằng IDE)**
  - Mở project bằng Visual Studio hoặc Rider, dùng tổ hợp phím `Ctrl + R, R` (hoặc `F2`) để đổi tên đồng loạt các từ khóa nhạy cảm (nếu có):
    - `Chicken` $\rightarrow$ `AlienUnit` / `EnemyShip`
    - `Egg` $\rightarrow$ `PlasmaOrb` / `EnemyProjectile`
    - `Feather` $\rightarrow$ `MetalDebris`
    - `Food` / `Roast` $\rightarrow$ `EnergyCore` / `CrystalPickup`

---

## 🎨 GIAI ĐOẠN 2: THAY ÁO ĐỒ HỌA & ÂM THANH - RESKIN (NGÀY 3 - 5)

Sử dụng bộ tài nguyên miễn phí **Kenney Space Shooter Redux**, **Kenney Alien UFO Pack** và **FoozleCC Void Series**.

### 2.1. Bảng ánh xạ thay thế Prefab & Sprite
| Đối tượng gốc | Đối tượng mới (Star Vanguard) | Thao tác kỹ thuật trong Unity |
| :--- | :--- | :--- |
| **Player Ship** | Chiến cơ Ngân Hà (Màu xanh/trắng) | Thay `Sprite` trong Prefab Player, chỉnh lại `PolygonCollider2D` hoặc `BoxCollider2D`. |
| **Gà thường (Wave 1-5)** | Tàu trinh sát Alien / Đĩa bay UFO | Thay `Sprite` trên các Prefab Enemy. Trong `Animator`, thay các frame gà vỗ cánh bằng frame đèn UFO nhấp nháy hoặc động cơ phản lực. |
| **Boss Gà (Wave 6)** | Tàu Mẹ Hủy Diệt (Alien Mothership) | Thay bằng Sprite phi thuyền khổng lồ. Giữ nguyên script `EnemyController` và các kiểu bắn. |
| **Trứng gà rơi** | Đạn Plasma Đỏ / Tím | Thay Sprite quả trứng bằng cầu năng lượng. Thay animation "trứng vỡ" ở đáy màn hình bằng hiệu ứng tia lửa điện (`Spark`). |
| **Đùi gà / Gà quay** | Tinh thể Năng lượng (Energy Crystal) | Thay Sprite trong `PickupManager` thành hình ngôi sao/viên kim cương. |
| **Lông gà bay ra khi nổ** | Mảnh vỡ kim loại & Lửa vũ trụ | Vào `Particle System` của vụ nổ, thay Texture chiếc lông gà bằng mảnh vỡ tàu vũ trụ hoặc hạt sáng (Glow Particle). |

### 2.2. Thay thế Hệ thống Âm thanh (`AudioManager`)
* **Tuyệt đối không giữ lại âm thanh gốc của Chicken Invaders:**
  - [ ] Thay nhạc nền Menu và In-game bằng nhạc điện tử không gian (**Synthwave / Sci-Fi Ambient**).
  - [ ] Thay tiếng gà kêu khi trúng đạn/chết bằng tiếng kim loại va chạm và tiếng nổ phi thuyền (`Explosion_SciFi.wav`).
  - [ ] Thay tiếng đẻ trứng bằng tiếng súng Laser của địch (`Laser_Enemy.wav`).
  - [ ] Thay tiếng nhặt đùi gà bằng tiếng nạp năng lượng (`PowerUp_Collect.wav`).

---

## ⚙️ GIAI ĐOẠN 3: TÙY BIẾN GAMEPLAY & CODE ĐỂ TẠO DẤU ẤN CÁ NHÂN (NGÀY 6 - 9)

Để chứng minh bạn hiểu sâu về code khi giảng viên hỏi vấn đáp, hãy thực hiện 4 tinh chỉnh kỹ thuật sau:

### 3.1. Tinh chỉnh `WaveDirector.cs` và `EnemyMover.cs`
- [ ] Thay đổi thông số biên độ và tần số của hàm lượng giác $\sin(x)$ trong các đợt bay hình vòng cung (Arc patterns):
  $$y(t) = A \cdot \sin(\omega \cdot t + \varphi)$$
  *(Thay đổi giá trị biên độ $A$ và tần số góc $\omega$ trong Inspector hoặc code để tạo quỹ đạo bay mới).*
- [ ] Thay đổi số lượng quái mỗi hàng/cột trong `Formation Waves` (Ví dụ: từ lưới $5 \times 8$ sang đội hình chữ V hoặc $4 \times 10$).

### 3.2. Nâng cấp `LeaderboardManager.cs` & `SettingsManager.cs`
- [ ] Đổi tên file JSON lưu điểm (Ví dụ: từ `leaderboard.json` sang `star_vanguard_top5.json`).
- [ ] Đổi các khóa `PlayerPrefs` trong `SettingsManager` (Ví dụ: `MasterVolume` $\rightarrow$ `SV_MasterVol`).

### 3.3. Việt hóa toàn bộ Giao diện (`UIManager` & `TextMeshPro`)
- [ ] Sử dụng Font hỗ trợ tiếng Việt (như *Roboto*, *Orbitron* hoặc *Chakra Petch* đã tạo `TextMeshPro Font Asset`).
- [ ] Chuyển ngữ toàn bộ hệ thống Menu:
  - `START GAME` $\rightarrow$ `KHỞI ĐỘNG CHIẾN DỊCH`
  - `LEADERBOARD` $\rightarrow$ `BẢNG VINH DANH`
  - `SETTINGS` $\rightarrow$ `CÀI ĐẶT HỆ THỐNG`
  - `PAUSED` $\rightarrow$ `TẠM DỪNG NHIỆM VỤ`
  - `LIGHT SPEED!` $\rightarrow$ `BƯỚC NHẢY KHÔNG GIAN!`
  - Thêm dòng chữ ở góc màn hình chính: *"Đồ án môn học - Thực hiện bởi: [Tên của bạn] - MSSV: [Mã số]"*.

---

## 📊 GIAI ĐOẠN 4: CẤU TRÚC BÁO CÁO & SƠ ĐỒ KIẾN TRÚC (NGÀY 10 - 12)

Sử dụng trực tiếp cấu trúc code hiện có của project để viết báo cáo Word và làm Slide thuyết trình:

### 4.1. Đề cương Báo cáo chuẩn CNTT
1. **Chương 1: Tổng quan đề tài**
   - Lý do chọn đề tài, mục tiêu nghiên cứu, phạm vi đề tài (Game 2D Space Shooter trên Unity 6).
2. **Chương 2: Cơ sở lý thuyết & Công nghệ sử dụng**
   - Kiến trúc Unity Engine (GameObject, Component, Lifecycle `Awake`, `Start`, `Update`, `FixedUpdate`).
   - Universal Render Pipeline (URP) 2D và New Input System.
   - Các mẫu thiết kế phần mềm (Design Patterns): **Singleton**, **Object Pooling**, **State Machine**, **Observer (C# Events)**.
3. **Chương 3: Phân tích và Thiết kế hệ thống**
   - Sơ đồ Use Case tổng quát.
   - Sơ đồ Lớp (Class Diagram) cho hệ thống Quản lý (`GameManager`, `PoolManager`, `WaveDirector`) và Thực thể (`EnemyController`, `EnemyMover`, `IDamageable`, `IPoolable`).
   - Sơ đồ Trạng thái (State Diagram) của vòng đời trò chơi (`Menu` $\rightarrow$ `Playing` $\rightarrow$ `Paused` $\rightarrow$ `GameOver`).
4. **Chương 4: Cài đặt và Thử nghiệm**
   - Chụp ảnh giao diện thực tế sau khi đã Reskin.
   - Giải thích thuật toán Object Pooling và cơ chế lưu điểm bằng JSON.
5. **Chương 5: Kết luận và Hướng phát triển**
   - Đánh giá kết quả đạt được, hướng mở rộng (Thêm cửa hàng nâng cấp tàu, chế độ Co-op 2 người chơi).

---

## 💻 HƯỚNG DẪN XUẤT CẤU TRÚC CODE ĐỂ PHÂN TÍCH TIẾP

Nếu bạn muốn gửi cấu trúc code của repo `Nadir0702/ChickenInvaders-Unity` lên cuộc trò chuyện để chỉnh sửa chi tiết từng file, hãy mở Terminal (Command Prompt / PowerShell) tại thư mục project trên máy bạn và chạy lệnh sau:

```powershell
# Lệnh liệt kê toàn bộ các file C# trong thư mục Assets
Get-ChildItem -Path .\Assets -Filter *.cs -Recurse | Select-Object FullName
```
*(Copy kết quả trả về hoặc nội dung file `.cs` bạn muốn sửa gửi vào khung chat).*