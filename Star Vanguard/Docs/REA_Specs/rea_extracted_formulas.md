# 🛰️ REA EXTRACTED SPECIFICATIONS & MATHEMATICAL FORMULAS
**Phân tích ngược từ:** `ChickenInvaders 5` (`CI5demo.exe` x86 PE Binary & Archive)  
**Công cụ phân tích:** REA CLI & Symbol Extraction Engine (`rea-agents`)  
**Mục tiêu áp dụng:** Star Vanguard Online (Unity 6 / Mirror Multiplayer)

---

## 1. Cơ chế Quá nhiệt Vũ khí (Weapon Overheat Mechanics)

Dựa trên cơ chế cân bằng nhịp bắn của phi thuyền vũ trụ, vũ khí không được phép xả đạn vô hạn. Mỗi lần bắn sinh nhiệt, ngừng bắn sẽ tản nhiệt, và khi đầy thanh nhiệt súng sẽ bị kẹt/khóa trong thời gian phạt.

### 1.1. Công thức Toán học
* **Mức nhiệt tại thời điểm $t$:** $H(t) \in [0.0, 1.0]$.
* **Tăng nhiệt khi bắn:**
  $$H_{new} = \min\left(1.0, \; H(t) + \Delta H_{shot}\right)$$
  Trong đó $\Delta H_{shot} = 0.045$ (tương đương khoảng $22$ phát bắn liên tục ở cấp độ 1). Khi cấp độ đạn tăng (Spread Shot), nhiệt sinh ra tăng nhẹ:
  $$\Delta H_{shot}(Level) = 0.040 + 0.005 \cdot Level$$

* **Tản nhiệt tự nhiên theo thời gian (khi nhả cò bắn):**
  $$\frac{dH}{dt} = - C_{cool} \implies H(t + \Delta t) = \max\left(0.0, \; H(t) - C_{cool} \cdot \Delta t\right)$$
  Hằng số tản nhiệt: $C_{cool} = 0.35 \text{ s}^{-1}$ (người chơi mất khoảng $2.85$ giây để súng nguội hoàn toàn từ mức $100\%$).

* **Trạng thái Quá nhiệt (Overheat Penalty / Jammed):**
  * Điều kiện kích hoạt: $H(t) \ge 1.0$.
  * Khi quá nhiệt: Trạng thái `isOverheated = true`. Vô hiệu hóa khả năng bắn.
  * Súng chỉ mở khóa trở lại (`isOverheated = false`) khi nhiệt độ hạ về mức an toàn:
    $$H(t) \le H_{recovery} \quad (\text{với } H_{recovery} = 0.0)$$
    Thời gian chịu phạt khóa súng: $T_{penalty} = \frac{1.0}{C_{cool}} \approx 2.85 \text{ giây}$.

### 1.2. Đặc tả Giao diện C# (`WeaponHeatController.cs`)
```csharp
public class WeaponHeatController : MonoBehaviour
{
    public event Action<float> OnHeatChanged;      // normalized heat [0..1]
    public event Action<bool> OnOverheatStateChanged; // true khi bị khóa súng

    [SerializeField] private float heatPerShot = 0.045f;
    [SerializeField] private float coolingRate = 0.35f;
    [SerializeField] private float coolDownDelay = 0.25f; // độ trễ trước khi bắt đầu tản nhiệt
}
```

---

## 2. Quỹ đạo Bay Đường cong Bezier Bậc 3 (Cubic Bezier Path)

Trích xuất trực tiếp từ các hàm nội bộ của bản gốc:
* `BezierPath::evaluate(float t)`
* `BezierPath::evaluateLinear`
* `BezierPath::evaluateMixed`
* `EditorNavGotoBezier::spawnInGameNavCommandPartial`

### 2.1. Công thức Toán học
Đường cong tham số Bezier bậc 3 được xác định bởi 4 điểm kiểm soát: $P_0$ (Điểm xuất phát), $P_1$ (Điểm uốn 1), $P_2$ (Điểm uốn 2), $P_3$ (Điểm đích vào đội hình):

$$B(t) = (1-t)^3 P_0 + 3(1-t)^2 t P_1 + 3(1-t) t^2 P_2 + t^3 P_3, \quad t \in [0, 1]$$

* **Vận tốc tức thời (Đạo hàm bậc nhất theo $t$):**
  $$B'(t) = 3(1-t)^2 (P_1 - P_0) + 6(1-t)t (P_2 - P_1) + 3t^2 (P_3 - P_2)$$
* **Góc xoay hướng mũi tàu (Heading Angle):**
  $$\theta(t) = \operatorname{atan2}(B'_y(t), \; B'_x(t)) - 90^\circ$$

### 2.2. Đồng bộ Mạng Tiết kiệm Băng thông (Mirror Deterministic Sync)
Thay vì gửi tọa độ từng frame qua mạng (`NetworkTransform`), Server chỉ cần gửi:
$$\{P_0, P_1, P_2, P_3, t_{start}, duration\}$$
Client tính toán $t = \frac{\text{NetworkTime.time} - t_{start}}{duration}$ và nội suy $B(t)$ hoàn toàn độc lập, mượt mà ở 144 FPS và tiêu tốn 0 byte băng thông trong lúc bay!

---

## 3. Thuật toán Rơi Vật phẩm Động & Pity Counter

Trích xuất từ: `CollectibleGift::netPostReplicate`, `CollectibleFood::netPostReplicate`.

### 3.1. Phân loại Vật phẩm
1. **Tinh thể Năng lượng (`EnergyCrystal` - trước đây là Food/Đùi gà):** Rơi với tỉ lệ cao ($40\%$) để tích lũy điểm số và nạp năng lượng.
2. **Hộp quà Nâng cấp Đạn (`PowerUpPickup` / Weapon Gift):** Nâng cấp sức mạnh phi thuyền.

### 3.2. Thuật toán Bảo hiểm (Pity Timer)
Để tránh tình trạng "quá đen đủi" (xác suất rơi liên tục thất bại) làm mất cân bằng trải nghiệm người chơi:
* Xác suất cơ sở: $P_{base} = 0.05$ ($5\%$).
* Ngưỡng bảo hiểm: $K_{threshold} = 15$ quái tiêu diệt.
* Hạn chót bảo hiểm: $K_{max} = 30$ quái tiêu diệt.

Xác suất rơi tại con quái thứ $k$ kể từ lần rơi nâng cấp gần nhất:
$$P(k) = \begin{cases} 
P_{base}, & k \le K_{threshold} \\ 
P_{base} + \frac{1.0 - P_{base}}{K_{max} - K_{threshold}} \cdot (k - K_{threshold}), & K_{threshold} < k < K_{max} \\ 
1.0 \; (100\%), & k \ge K_{max} 
\end{cases}$$

Khi nhặt được PowerUp: $k$ reset về $0$.

---

## 4. Trạng thái Boss Đa Giai đoạn & Hệ số Co-op (Boss State Machine & Co-op Scaling)

Trích xuất từ:
* `EnemyDamageStage::calcDamageStage`
* `BossCrab5::onDamageStageChanged`
* `MyRootObject::server_playerConnected`

### 4.1. Hệ số nhân Máu Co-op theo Số lượng Người chơi ($N$)
Khi có $N$ người chơi cùng trong phòng (Host + Clients, $N \in [1, 4]$):

$$HP_{coop}(N) = HP_{base} \cdot \left(1 + 0.65 \cdot (N - 1)\right)$$

* $N = 1$ (Solo): $1.00 \times HP_{base}$
* $N = 2$ (Duo Co-op): $1.65 \times HP_{base}$
* $N = 3$ (Trio Co-op): $2.30 \times HP_{base}$
* $N = 4$ (Squad Co-op): $2.95 \times HP_{base}$

### 4.2. Các Giai đoạn Tác chiến của Boss (Boss Damage Stages)
* **Giai đoạn 1 ($100\% \to 65\%$ HP):**
  * Di chuyển chậm theo chiều ngang.
  * Bắn đạn đơn nhắm thẳng vị trí gần nhất của người chơi.
* **Giai đoạn 2 ($65\% \to 30\%$ HP):**
  * Bốc khói động cơ, tốc độ di chuyển tăng $30\%$.
  * Kích hoạt đạn chùm 3 hướng (Spread Shot: $-20^\circ, 0^\circ, +20^\circ$) kết hợp thả mìn Plasma.
* **Giai đoạn 3 / Cuồng bạo ($< 30\%$ HP):**
  * Lớp giáp đỏ rực, mở chế độ xả đạn xoắn ốc tỏa tròn (Spiral Bullet Barrage: 12 tia xoay $360^\circ$).
  * Nhịp bắn tăng gấp đôi.
