# ? FIXES: L?i LINQ + UI Improvements

## ?? L?i ?ã s?a

### **1. L?i LINQ Translation**

**L?i:**
```
The LINQ expression '.Count(e => e.MaKhuVuc == (int?)StructuralTypeShaperExpression:
QuanLyBaiDoXe.Models.Entities.KhuVuc ValueBufferExpression:
ProjectionBindingExpression: Outer IsNullable: False .MaKhuVuc)' could not be translated.
```

**Nguyên nhân:**
- Trong `ReservationController.GetViTriTrong()`, dòng 156
- C? g?ng s? d?ng bi?n `viTriTrong` (?ã load tr??c) trong LINQ query ph?c t?p
- EF Core không th? translate query này sang SQL

**Code c? (SAI):**
```csharp
var khuVucs = await _context.KhuVucs
    .Include(kv => kv.MaLoaiXeNavigation)
    .Where(kv => kv.MaLoaiXe == null || kv.MaLoaiXe == maLoaiXe)
    .Select(kv => new KhuVucDto
    {
        MaKhuVuc = kv.MaKhuVuc,
        TenKhuVuc = kv.TenKhuVuc,
        MaLoaiXe = kv.MaLoaiXe,
        TenLoaiXe = kv.MaLoaiXeNavigation != null ? kv.MaLoaiXeNavigation.TenLoaiXe : null,
        SoChoTrong = viTriTrong.Count(vt => vt.MaKhuVuc == kv.MaKhuVuc), // ? L?I
        TongSoCho = kv.ViTriDos.Count()
    })
    .ToListAsync();
```

**Code m?i (?ÚNG):**
```csharp
// Load khu v?c tr??c
var khuVucs = await _context.KhuVucs
    .Include(kv => kv.MaLoaiXeNavigation)
    .Include(kv => kv.ViTriDos)
    .Where(kv => kv.MaLoaiXe == null || kv.MaLoaiXe == maLoaiXe)
    .ToListAsync(); // ? Load v? memory tr??c

// Tính toán trong memory ?? tránh l?i LINQ translation
var khuVucDtos = khuVucs.Select(kv => new KhuVucDto
{
    MaKhuVuc = kv.MaKhuVuc,
    TenKhuVuc = kv.TenKhuVuc,
    MaLoaiXe = kv.MaLoaiXe,
    TenLoaiXe = kv.MaLoaiXeNavigation?.TenLoaiXe,
    SoChoTrong = viTriTrong.Count(vt => vt.MaKhuVuc == kv.MaKhuVuc), // ? OK trong memory
    TongSoCho = kv.ViTriDos.Count()
}).ToList();
```

**Gi?i pháp:**
1. Load `KhuVucs` v? memory tr??c b?ng `.ToListAsync()`
2. Sau ?ó m?i tính toán `SoChoTrong` trong memory
3. Nh? v?y không còn l?i translation

---

## ?? UI Improvements

### **2. ?n giá vé tháng trong card lo?i xe**

**Yêu c?u:** Không c?n hi?n th? giá vé tháng trong form ??t ch?

**Thay ??i:**

**Tr??c:**
```html
<div class="vehicle-type-card">
    <div class="vehicle-type-icon">??</div>
    <div class="vehicle-type-name">Xe ô tô 4 ch?</div>
    <div class="vehicle-type-desc">Phù h?p cho xe sedan, SUV</div>
    <div class="vehicle-type-price">
        <span class="price-label">Giá vé tháng:</span>
        <span class="price-value">500,000 ?/tháng</span>  ? XÓA
    </div>
</div>
```

**Sau:**
```html
<div class="vehicle-type-card">
    <div class="vehicle-type-icon">??</div>
    <div class="vehicle-type-name">Xe ô tô 4 ch?</div>
    <div class="vehicle-type-desc">Phù h?p cho xe sedan, SUV</div>
</div>
```

**CSS ?ã xóa:**
- `.vehicle-type-price`
- `.price-label`
- `.price-value`

---

### **3. ??i t? Grid Card ? Dropdown Menu cho v? trí**

**Yêu c?u:** Ch?n v? trí b?ng dropdown thay vì grid card

**Tr??c (Grid Card):**
```
????????????????????????????????????????????
?  [A1]  [A2]  [A3]  [A4]  [A5]           ?
?  [B1]  [B2]  [B3]  [B4]  [B5]           ?
?  [C1]  [C2]  [C3]  [C4]  [C5]           ?
????????????????????????????????????????????
```

**Sau (Dropdown):**
```
????????????????????????????????????????????
?  V? trí ?? xe *                          ?
?  ??????????????????????????????????????  ?
?  ? -- Ch?n v? trí ?? xe --         ? ?  ?
?  ?                                    ?  ?
?  ? ? Khu v?c A                        ?  ?
?  ?   A1 - Khu v?c A                   ?  ?
?  ?   A2 - Khu v?c A                   ?  ?
?  ?   A3 - Khu v?c A                   ?  ?
?  ?                                    ?  ?
?  ? ? Khu v?c B                        ?  ?
?  ?   B1 - Khu v?c B                   ?  ?
?  ?   B2 - Khu v?c B                   ?  ?
?  ??????????????????????????????????????  ?
?                                          ?
?  ?? V? trí ?ã ch?n: A1 - Khu v?c A       ?
????????????????????????????????????????????
```

**HTML m?i:**
```html
<div class="form-group">
    <label class="form-label">
        <i class="fas fa-parking"></i>
        V? trí ?? xe <span class="required-mark">*</span>
    </label>
    <select class="form-select" id="viTriSelect" name="maViTri" required onchange="selectViTriFromDropdown()">
        <option value="">-- Ch?n v? trí ?? xe --</option>
    </select>
    <small class="text-muted mt-1 d-block">Vui lòng ch?n lo?i xe tr??c ?? xem v? trí tr?ng</small>
</div>

<!-- Info hi?n th? v? trí ?ã ch?n -->
<div id="viTriInfo" class="alert alert-info" style="display: none;">
    <i class="fas fa-info-circle me-2"></i>
    <strong>V? trí ?ã ch?n:</strong> <span id="viTriInfoText"></span>
</div>
```

**JavaScript m?i:**
```javascript
function renderViTriDropdown() {
    const viTriSelect = document.getElementById('viTriSelect');
    
    if (viTriList.length === 0) {
        viTriSelect.innerHTML = '<option value="">-- Không có v? trí tr?ng --</option>';
        viTriSelect.disabled = true;
        return;
    }

    viTriSelect.disabled = false;
    viTriSelect.innerHTML = '<option value="">-- Ch?n v? trí ?? xe --</option>';

    // Group by khu v?c
    const grouped = {};
    viTriList.forEach(viTri => {
        const khuVuc = viTri.tenKhuVuc || 'Khác';
        if (!grouped[khuVuc]) {
            grouped[khuVuc] = [];
        }
        grouped[khuVuc].push(viTri);
    });

    // Render options grouped by khu v?c
    Object.keys(grouped).sort().forEach(khuVuc => {
        const optgroup = document.createElement('optgroup');
        optgroup.label = khuVuc;
        
        grouped[khuVuc].forEach(viTri => {
            const option = document.createElement('option');
            option.value = viTri.maViTri;
            option.textContent = `${viTri.tenViTri} - ${viTri.tenKhuVuc}`;
            option.setAttribute('data-khuvuc', viTri.tenKhuVuc);
            optgroup.appendChild(option);
        });

        viTriSelect.appendChild(optgroup);
    });
}

function selectViTriFromDropdown() {
    const viTriSelect = document.getElementById('viTriSelect');
    const selectedValue = viTriSelect.value;

    if (selectedValue) {
        selectedViTri = parseInt(selectedValue);
        
        // Hi?n th? thông tin v? trí ?ã ch?n
        const viTri = viTriList.find(v => v.maViTri === selectedViTri);
        if (viTri) {
            const viTriInfo = document.getElementById('viTriInfo');
            const viTriInfoText = document.getElementById('viTriInfoText');
            viTriInfoText.textContent = `${viTri.tenViTri} - ${viTri.tenKhuVuc}`;
            viTriInfo.style.display = 'block';
        }
    } else {
        selectedViTri = null;
        document.getElementById('viTriInfo').style.display = 'none';
    }

    updateSummary();
}
```

**CSS ?ã xóa:**
- `.parking-grid` và các styles liên quan
- `.parking-spot` và các states (hover, selected, occupied)
- `.khu-vuc-filter` và `.khu-vuc-btn`
- `.spot-name`, `.spot-zone`, `.spot-status`

---

## ?? Tóm t?t Files ?ã thay ??i

| File | Thay ??i |
|------|----------|
| `ReservationController.cs` | ? S?a l?i LINQ trong `GetViTriTrong()` |
| `Create.cshtml` (HTML) | ? Xóa giá vé tháng trong card lo?i xe<br>? ??i grid card ? dropdown cho v? trí |
| `Create.cshtml` (CSS) | ? Xóa `.vehicle-type-price` và related styles<br>? Xóa `.parking-grid` và related styles |
| `Create.cshtml` (JS) | ? Xóa `renderKhuVucFilter()`, `filterByZone()`, `renderParkingSpots()`, `selectParkingSpot()`<br>? Thêm `renderViTriDropdown()`, `selectViTriFromDropdown()` |

---

## ? K?t qu?

### **Before:**
```
? L?i LINQ khi load v? trí
? Hi?n th? giá vé tháng (không c?n)
? Grid card ph?c t?p cho v? trí
```

### **After:**
```
? Load v? trí không l?i
? Card lo?i xe g?n gàng h?n (không có giá)
? Dropdown ??n gi?n, d? ch?n v? trí
? Group theo khu v?c trong dropdown
? Hi?n th? thông báo v? trí ?ã ch?n
```

---

## ?? Benefits

1. **Performance:** Query nhanh h?n (load v? memory tr??c r?i tính toán)
2. **UX:** Dropdown d? s? d?ng h?n grid (??c bi?t trên mobile)
3. **Clean UI:** Card lo?i xe g?n gàng h?n, t?p trung vào thông tin chính
4. **Accessibility:** Dropdown t?t h?n cho screen reader

---

## ?? Testing

```sh
dotnet run
```

1. ? ??ng nh?p v?i role Customer
2. ? Vào "??t ch?" ? "??t ch? m?i"
3. ? Ch?n lo?i xe ? Không có giá vé tháng
4. ? Dropdown v? trí hi?n th? (grouped by khu v?c)
5. ? Ch?n v? trí ? Hi?n th? thông báo xác nh?n
6. ? Submit form ? Không l?i LINQ

---

**Fixed with ?? by GitHub Copilot**
