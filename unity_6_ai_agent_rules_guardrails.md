# 🛡️ UNITY 6 AI AGENT RULES & GUARDRAILS (`.cursorrules` / `AGENTS.md`)

* **Project:** Star Vanguard - Chiến Cơ Không Gian (2D Space Shooter)
* **Engine Version:** Unity 6 (`6000.0.54f1`+) - Universal Render Pipeline (URP) 2D
* **Base Architecture:** Singleton Managers, Object Pooling (`IPoolable`), Interface-Driven (`IDamageable`), Event-Driven C# Actions, SOLID Principles.

> **CRITICAL DIRECTIVE FOR ALL AI AGENTS (`@Architect`, `@REA-Analyst`, `@Unity-Coder`, `@Asset-UI`):**  
> Bạn đang làm việc trên một dự án Unity 6 thực tế. Mọi thao tác sai lệch với hệ thống Serialization, GUID hoặc `.meta` của Unity sẽ làm hỏng toàn bộ Scene và Prefab. Bạn **BẮT BUỘC** phải tuân thủ nghiêm ngặt các quy tắc dưới đây trước khi thực hiện bất kỳ lệnh tạo, sửa, đổi tên hoặc xóa file nào.

---

## 1. QUY TẮC BẤT KhẢ XÂM PHẠM VỀ FILE `.meta`, `.prefab`, `.unity` (UNITY SERIALIZATION)

1. **TUYỆT ĐỐI KHÔNG XÓA HOẶC TỰ SINH FILE `.meta`:**
   * Mỗi tài nguyên trong Unity được định danh bằng mã `guid` nằm trong file `.meta` đi kèm.
   * **CẤM** xóa file `.meta` hoặc tự bịa ra mã `guid` mới.
   * Khi di chuyển hoặc đổi tên một file `.cs`, `.png`, `.wav`, `.prefab` bằng dòng lệnh hoặc công cụ của Agent, bạn **PHẢI** di chuyển/đổi tên cả file `.meta` tương ứng đi kèm theo đúng tên mới:
     ```bash
     # ĐÚNG:
     mv Assets/Scripts/Chicken.cs Assets/Scripts/AlienFighter.cs
     mv Assets/Scripts/Chicken.cs.meta Assets/Scripts/AlienFighter.cs.meta
     ```
2. **QUY TẮC ĐỔI TÊN CLASS VÀ FILE C# (`MonoBehaviour` & `ScriptableObject`):**
   * Trong Unity, tên file `.cs` **BẮT BUỘC phải trùng khớp 100%** (kể cả chữ hoa/thường) với tên `class` kế thừa từ `MonoBehaviour` hoặc `ScriptableObject` bên trong nó.
   * Nếu bạn đổi `public class ChickenController : MonoBehaviour` thành `public class AlienController : MonoBehaviour`, bạn **phải** đổi tên file thành `AlienController.cs` VÀ đổi tên `ChickenController.cs.meta` thành `AlienController.cs.meta` trong cùng một lượt thực thi (atomic operation).
3. **BẢO VỆ DỮ LIỆU INSPECTOR KHI ĐỔI TÊN BIẾN (`[FormerlySerializedAs]`):**
   * Khi Refactor/đổi tên bất kỳ biến `public` hoặc `[SerializeField] private` nào trong các Script đã gắn vào Prefab/Scene, bạn **BẮT BUỘC** phải thêm thuộc tính `[FormerlySerializedAs("oldVariableName")]` từ thư viện `UnityEngine.Serialization` để không làm mất cấu hình trên Inspector:
     ```csharp
     using UnityEngine;
     using UnityEngine.Serialization;

     // ĐÚNG KHI REFACTOR TỪ "eggPrefab" SANG "plasmaOrbPrefab":
     [FormerlySerializedAs("eggPrefab")]
     [SerializeField] private GameObject plasmaOrbPrefab;
     ```
4. **KHÔNG SỬA TRỰC TIẾP CẤU TRÚC YAML CỦA `.unity` (SCENE) VÀ `.prefab` TRỪ KHI ĐƯỢC YÊU CẦU RÕ RÀNG:**
   * Không dùng Regex thay thế hàng loạt bên trong các file `.unity` hoặc `.prefab` nếu không hiểu rõ cấu trúc YAML (`m_Script: {fileID: 11500000, guid: ..., type: 3}`). Mọi thay đổi về liên kết hình ảnh/âm thanh ưu tiên thực hiện thông qua việc ghi đè file tài nguyên gốc (giữ nguyên tên file và `.meta`) hoặc thông qua Editor Script.

---

## 2. CHUẨN KIẾN TRÚC PHẦN MỀM & SOLID (ARCHITECTURE STANDARDS)

Mọi đoạn code C# viết mới hoặc nâng cấp phải tuân thủ 100% kiến trúc hiện có của dự án:

### 2.1. Single Responsibility Principle (SRP - Đơn trách nhiệm)
* **CẤM** nhồi nhét mọi logic vào một "God Class". Hãy giữ đúng sự phân tách hiện tại của dự án:
  * `EnemyController`: Chỉ quản lý máu (`Health`), trạng thái sống/chết và nhận sát thương (`IDamageable`).
  * `EnemyMover`: Chỉ quản lý toán học di chuyển (Formation, Sine Arc, Diving, Cubic Bezier).
  * `EnemyShootingSimple` / `BossAttackController`: Chỉ quản lý logic xả đạn và nhịp bắn.
  * `WeaponHeatController`: Chỉ tính toán nhiệt độ súng của Player và bắn Event lên UI.

### 2.2. Object Pooling Mandate (Bắt buộc dùng PoolManager)
* **CẤM SỬ DỤNG `Instantiate()` VÀ `Destroy()`** trong lúc Gameplay đang diễn ra (`Update`, `FixedUpdate`, Coroutine chiến đấu) đối với: Đạn (Bullets/Plasma), Kẻ địch (Enemies), Vật phẩm rơi (Pickups/Crystals), Hiệu ứng nổ (VFX), và Icon UI động.
* Mọi đối tượng sinh ra nhiều lần **PHẢI** thực thi interface `IPoolable` và gọi qua `PoolManager.Instance`:
  * Khi lấy ra từ Pool: Khởi tạo lại trạng thái trong `OnSpawnFromPool()` (reset máu, vận tốc, bộ đếm thời gian).
  * Khi hủy: Gọi `PoolManager.Instance.ReturnToPool(...)` và dọn dẹp Coroutine/Event trong `OnReturnToPool()`.

### 2.3. Interface-Driven & Event-Driven (Giảm phụ thuộc - Decoupling)
* **Va chạm & Sát thương:** Khi đạn va chạm (`OnTriggerEnter2D`), chỉ kiểm tra qua interface `IDamageable`:
  ```csharp
  if (other.TryGetComponent<IDamageable>(out var damageable))
  {
      damageable.TakeDamage(damageAmount);
  }
  ```
* **Giao tiếp với UI và Audio:** Các lớp Gameplay (`Player`, `EnemyController`, `WaveDirector`) **không được** tham chiếu trực tiếp tới các thành phần `TextMeshProUGUI` hay `Slider` lẻ tẻ. Hãy sử dụng `C# Events` (`public event Action<float> OnHeatChanged;`) hoặc gọi qua Singleton chuẩn (`UIManager.Instance`, `AudioManager.Instance`).

---

## 3. QUY TẮC TỐI ƯU HIỆU NĂNG TRÊN UNITY 6 (PERFORMANCE RULES)

1. **Zero Garbage Allocation trong `Update()`:**
   * Không khởi tạo mảng mới (`new List<>()`, `new Vector3[]`), không nối chuỗi (`string + string`) trong hàm `Update()` hoặc `LateUpdate()`.
   * Sử dụng `TextMeshPro.SetText("{0}", score)` thay vì `.text = score.ToString()` khi cập nhật điểm số liên tục.
2. **Cache Component & Animator Hash:**
   * Luôn lưu đệm (`cache`) các tham chiếu `Transform`, `Rigidbody2D`, `SpriteRenderer`, `Animator` trong hàm `Awake()`.
   * **CẤM** truyền chuỗi trực tiếp vào Animator mỗi frame (ví dụ `anim.SetTrigger("Hit")`). Bắt buộc dùng `Animator.StringToHash`:
     ```csharp
     private static readonly int HitHash = Animator.StringToHash("Hit");
     // Sử dụng: animator.SetTrigger(HitHash);
     ```
3. **Chuẩn Unity 6 API:**
   * Sử dụng `Rigidbody2D.linearVelocity` (chuẩn mới của Unity 6) hoặc giữ thống nhất với hệ thống di chuyển `Transform` hiện có của project, tránh trộn lẫn vật lý và dịch chuyển `transform.position` gây giật hình (jitter).
   * Sử dụng `FindFirstObjectByType<T>()` thay cho hàm `FindObjectOfType<T>()` đã bị đánh dấu lỗi thời (Deprecated) trong Unity 6.

---

## 4. QUY TẮC PHỐI HỢP MULTI-AGENT (TEAMWORK PREVIEW LOCKS)

Khi chạy chế độ nhiều Agent cùng lúc trên Antigravity:
1. **`@REA-Analyst`:** Chỉ đọc mã nhị phân/tài liệu bên ngoài và ghi kết quả công thức toán học (LaTeX + Pseudocode) vào `Docs/REA_Specs/`. Không chạm vào thư mục `Assets/`.
2. **`@Unity-Coder`:** Chỉ sửa đổi các file `.cs` trong `Assets/Scripts/`. Trước khi sửa một file, phải kiểm tra xem có phá vỡ chữ ký hàm (Method Signature) mà các file khác đang gọi hay không.
3. **`@Asset-UI`:** Khi thay thế hình ảnh con gà/trứng gà bằng Tàu vũ trụ/Đạn Plasma, ưu tiên **ghi đè file ảnh mới vào đúng đường dẫn và tên file ảnh cũ** (giữ nguyên file `.meta`), hoặc tạo Editor Script để gán lại Sprite tự động nhằm tránh xung đột file `.prefab`.
4. **Kiểm tra Biên dịch (Compilation Check):** Sau mỗi Task chỉnh sửa C#, Agent phải kiểm tra không còn bất kỳ lỗi cú pháp, thiếu `using`, hay sai tên kiểu dữ liệu nào trước khi đánh dấu `[x]` hoàn thành trong `task.md`.