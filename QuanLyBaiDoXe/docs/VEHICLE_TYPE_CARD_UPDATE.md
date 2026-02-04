# ? C?P NH?T: HI?N TH? LO?I XE D?NG CARD

## ?? Thay ??i

?ã c?i thi?n ph?n ch?n lo?i xe trong form ??t ch? ?? hi?n th? ??y ?? thông tin t? b?ng `LoaiXe`:

### **Tr??c:**
- Dropdown ??n gi?n ch? hi?n th? tên lo?i xe
- Không có thông tin Mô t? và Giá tháng

### **Sau:**
- ? **Card-based selection** ??p m?t
- ? Hi?n th? **Tên lo?i xe**
- ? Hi?n th? **Mô t?** (MoTa)
- ? Hi?n th? **Giá vé tháng** (GiaThang)
- ? Icon phù h?p (?? Ô tô, ??? Xe máy, ?? Xe khác)
- ? Hi?u ?ng hover và selected ??p
- ? Tick (?) khi card ???c ch?n

---

## ?? Files ?ã thay ??i

### 1. **`ReservationViewModel.cs`**
```csharp
public class LoaiXeDto
{
    public int MaLoaiXe { get; set; }
    public string? TenLoaiXe { get; set; }
    public string? MoTa { get; set; }        // ? NEW
    public decimal? GiaThang { get; set; }   // ? NEW
}
```

### 2. **`ReservationController.cs`** (Create action)
```csharp
var loaiXes = await _context.LoaiXes
    .Select(lx => new LoaiXeDto
    {
        MaLoaiXe = lx.MaLoaiXe,
        TenLoaiXe = lx.TenLoaiXe,
        MoTa = lx.MoTa,           // ? NEW
        GiaThang = lx.GiaThang    // ? NEW
    })
    .ToListAsync();
```

### 3. **`Create.cshtml`** - UI C?i ti?n

#### **CSS m?i:**
```css
.vehicle-type-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 16px;
}

.vehicle-type-card {
    background: white;
    border: 3px solid #e2e8f0;
    border-radius: 12px;
    padding: 20px;
    cursor: pointer;
    transition: all 0.3s;
}

.vehicle-type-card:hover {
    border-color: #667eea;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
}

.vehicle-type-card.selected {
    border-color: #667eea;
    background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
}

.vehicle-type-card.selected::before {
    content: '?';
    position: absolute;
    top: 12px;
    right: 12px;
    width: 28px;
    height: 28px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 50%;
    font-weight: 700;
}
```

#### **HTML m?i:**
```html
<div class="vehicle-type-grid">
    @foreach (var loaiXe in ViewBag.LoaiXes)
    {
        <div class="vehicle-type-card" data-loaixe="@loaiXe.MaLoaiXe" onclick="selectVehicleType(@loaiXe.MaLoaiXe, '@loaiXe.TenLoaiXe')">
            <div class="vehicle-type-icon">
                <i class="fas @icon"></i>
            </div>
            <div class="vehicle-type-name">@loaiXe.TenLoaiXe</div>
            <div class="vehicle-type-desc">@loaiXe.MoTa</div>
            <div class="vehicle-type-price">
                <span class="price-label">Giá vé tháng:</span>
                <span class="price-value">@loaiXe.GiaThang.ToString("N0") ?/tháng</span>
            </div>
        </div>
    }
</div>
```

#### **JavaScript m?i:**
```javascript
function selectVehicleType(maLoaiXe, tenLoaiXe) {
    // Remove selected class from all cards
    document.querySelectorAll('.vehicle-type-card').forEach(card => {
        card.classList.remove('selected');
    });

    // Add selected class to clicked card
    const selectedCard = document.querySelector(`[data-loaixe="${maLoaiXe}"]`);
    if (selectedCard) {
        selectedCard.classList.add('selected');
    }

    // Update hidden input
    document.getElementById('maLoaiXe').value = maLoaiXe;
    selectedLoaiXe = maLoaiXe;

    // Update summary and load parking spots
    updateSummary();
    loadParkingSpots();
}
```

---

## ?? UI Preview

### **Card Layout:**
```
???????????????????????????????????
?          [? Selected]           ?  ? Checkmark khi ch?n
?                                 ?
?           ?? (Icon)              ?
?                                 ?
?      Xe ô tô 4 ch?              ?  ? Tên lo?i xe
?                                 ?
?   Phù h?p cho xe sedan,         ?  ? Mô t?
?   SUV, và xe gia ?ình           ?
?                                 ?
? ??????????????????????????????? ?
? Giá vé tháng: 500,000 ?/tháng  ?  ? Giá tháng
???????????????????????????????????
```

### **States:**
- **Default:** Border xám (#e2e8f0)
- **Hover:** Border tím (#667eea) + nâng lên
- **Selected:** Border tím + background gradient + tick (?)

---

## ?? Features

### **1. T? ??ng phát hi?n icon:**
```csharp
var icon = loaiXe.TenLoaiXe?.Contains("Ô tô") == true ? "fa-car" :
           loaiXe.TenLoaiXe?.Contains("Xe máy") == true ? "fa-motorcycle" :
           "fa-truck";
```

### **2. Fallback cho mô t?:**
```csharp
@(loaiXe.MoTa ?? "Phù h?p cho nhu c?u g?i xe hàng ngày")
```

### **3. Format giá tháng:**
```csharp
@((loaiXe.GiaThang ?? 0).ToString("N0")) ?/tháng
```

---

## ? Tham kh?o t? VehicleEntry

?ã tham kh?o cách VehicleEntry load và hi?n th? lo?i xe:
- Load t? `_context.LoaiXes`
- S? d?ng entity `LoaiXe` tr?c ti?p v?i ??y ?? properties
- Áp d?ng vào Reservation v?i UI c?i ti?n h?n (Card thay vì Dropdown)

---

## ?? So sánh

| Tính n?ng | Tr??c | Sau |
|-----------|-------|-----|
| UI | Dropdown ??n gi?n | Card ??p, tr?c quan |
| Thông tin | Ch? tên | Tên + Mô t? + Giá |
| T??ng tác | Click dropdown | Click card (d? h?n) |
| Visual feedback | Text highlight | Border + Background + Checkmark |
| Responsive | ? | ? |
| Mobile-friendly | ?? | ? Better |

---

## ?? K?t qu?

? Build successful  
? UI ??p h?n, d? s? d?ng h?n  
? Hi?n th? ??y ?? thông tin t? b?ng LoaiXe  
? T??ng thích v?i flow hi?n t?i  
? Không breaking changes

---

## ?? Ghi chú

- **Icon mapping** có th? c?u hình thêm trong database (thêm c?t `Icon` vào b?ng `LoaiXe`)
- **Giá tháng** hi?n th? ?? user tham kh?o (cho ??t h?n l?ch thì không liên quan, nh?ng cho ??t trong ngày thì ti?n c?c = 50% giá 1 gi?)
- **Mô t?** nên ?i?n ??y ?? trong database ?? UX t?t h?n

---

**Updated with ?? by GitHub Copilot**
