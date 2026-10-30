# 🧠 STAR VANGUARD ONLINE - PROJECT MEMORY HUB
> **Dự án:** Star Vanguard Online (Chiến Cơ Không Gian Co-op LAN 1920x1080)  
> **Phiên bản Engine Target:** Unity 6 (`6000.5.7f1`) - Universal Render Pipeline 2D (URP)  
> **Networking:** Mirror Networking (Host/Client Co-op LAN 2-4 Players)  
> **Dự án phân tích nhị phân gốc:** *Chicken Invaders 5: Cluck of the Dark Side* (`CI5demo.exe`) qua bộ công cụ **REA** (`reverse-engineer-anything`)  
> **Thời gian cập nhật:** 29/10/2026 (Phiên bản Visual Menu Scene Authoring & Baker Tool, Play Mode 55/55 PASS)  

---

## 📌 MỤC ĐÍCH CỦA THƯ MỤC MEMORY

Thư mục `memory/` đóng vai trò là **Bộ nhớ vĩnh cửu (Persistent Project Memory)** của dự án, lưu trữ toàn bộ bối cảnh, lịch sử thay đổi, đặc tả kiến trúc kỹ thuật, tiến độ thực hiện và kế hoạch hành động tiếp theo. Bộ nhớ này giúp mọi Agent AI và lập trình viên con người có thể ngay lập tức nắm bắt toàn cảnh dự án mà không bị mất ngữ cảnh qua các phiên làm việc.

---

## 🗂️ MỤC LỤC TÀI LIỆU TRONG MEMORY

| STT | Tài liệu | Nội dung chính | Trạng thái |
| :-: | :--- | :--- | :-: |
| **01** | [`01_DA_LAM.md`](file:///c:/Documents/UNITY%202026/memory/01_DA_LAM.md) | Toàn bộ lịch sử công việc đã hoàn thành: Quét 44+ script, sửa lỗi WebP, fix 31 lỗi deprecated Unity 6, phân tích REA, 4 hệ thống cốt lõi, UI Architecture mới, tách BootstrapScene và kiểm thử Play Mode 55/55 pass. | ✅ Đã cập nhật (29/10) |
| **02** | [`02_HIEN_CO.md`](file:///c:/Documents/UNITY%202026/memory/02_HIEN_CO.md) | Tổng kê tài nguyên và hệ thống hiện có: Danh mục script C# (Core, UI, Networking, Editor Tools, BootstrapLoader), Prefab, Sprites 32-bit RGBA, Audio, 3 Scenes phân cấp. | ✅ Đã cập nhật (29/10) |
| **03** | [`03_CAU_TRUC_GAME.md`](file:///c:/Documents/UNITY%202026/memory/03_CAU_TRUC_GAME.md) | Sơ đồ kiến trúc kỹ thuật: Kiến trúc 3-Scene Bootstrap, phân biệt `[SceneBoundSingleton]` vs Persistent Singleton, Object Pooling, Event Bus, FSM, Wave Spawner. | ✅ Đã cập nhật (29/10) |
| **04** | [`04_TIEN_DO_VA_KE_HOACH.md`](file:///c:/Documents/UNITY%202026/memory/04_TIEN_DO_VA_KE_HOACH.md) | Bảng theo dõi tiến độ 8 Phase: Dedicated Bootstrap Scene, triệt tiêu bản copy persistent managers và cảnh báo chuyển giao, đạt 55/55 assertions pass. | ✅ Đã cập nhật (29/10) |
| **05** | [`05_BAO_CAO_SUA_LOI.md`](file:///c:/Documents/UNITY%202026/memory/05_BAO_CAO_SUA_LOI.md) | Báo cáo sửa lỗi và nghiệm thu Play Mode: Triệt tiêu 100% bản copy persistent manager trong GameplayScene/MainMenuScene, nghiệm thu thực tế 55/55 assertions pass qua `smoke_bootstrap_final.log`. | ✅ Đã cập nhật (29/10) |

---

## 🗺️ BẢN ĐỒ KHÔNG GIAN LÀM VIỆC (WORKSPACE LAYOUT)

```
C:\Documents\UNITY 2026\
├── memory\                       <--- [BỘ NHỚ TRUNG TÂM NÀY]
│   ├── README.md
│   ├── 01_DA_LAM.md
│   ├── 02_HIEN_CO.md
│   ├── 03_CAU_TRUC_GAME.md
│   └── 04_TIEN_DO_VA_KE_HOACH.md
│
├── Star Vanguard\                <--- [DỰ ÁN UNITY 6 CHÍNH]
│   ├── Assets\
│   │   ├── Scripts\              (44+ scripts C# đã refactor, 0 error)
│   │   ├── Sprites\              (Tài nguyên hình ảnh 2D)
│   │   ├── Prefabs\              (Player, Enemies, Projectiles, Boss)
│   │   └── Audio\                (BGM, SFX)
│   ├── Packages\manifest.json
│   └── ProjectSettings\
│
├── data rea\                     <--- [DỮ LIỆU DỊCH NGƯỢC TỪ CHICKEN INVADERS 5]
│   ├── ChickenInvaders 5\        (CI5demo.exe, WAD archives .dat.hq2x)
│   └── Docs\
│       ├── 10_reverse_engineering_report.md  (Báo cáo tổng hợp 30KB)
│       ├── 04_deep_analysis.md               (De Casteljau, 13 Weapons, WAD)
│       ├── 05_function_map.md                (Bản đồ hàm thực thi VA)
│       ├── 06_unresolved_questions.md         (Giới hạn kỹ thuật)
│       ├── REA_Specs\                        (rea_extracted_formulas.md)
│       └── systems\                          (01 -> 08 Báo cáo hệ thống gameplay)
│
├── ChickenInvaders-Unity-master\ <--- [MÃ NGUỒN CƠ SỞ BAN ĐẦU ĐỂ THAM CHIẾU]
└── ANTIGRAVITY_TEAMWORK_PLAN.md  <--- [KẾ HOẠCH PHỐI HỢP ĐỘI NGŨ 5 AGENT]
```
