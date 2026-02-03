# Tính n?ng "Xem T?ng Quan Tu?n" - Weekly Overview

## ?? T?ng Quan

?ã thêm tính n?ng **"Xem t?ng quan tu?n"** vào trang Danh sách Nhân Viên v?i kh? n?ng t? ??ng ??ng b? l?ch làm vi?c khi l?p ca.

## ? Tính N?ng M?i

### 1. Nút "Xem T?ng Quan Tu?n"
- **V? trí**: ? header c?a trang EmployeeList, bên c?nh nút "Phân công qu?y"
- **Màu s?c**: Gradient purple-blue n?i b?t
- **Icon**: `fas fa-calendar-week`

### 2. Modal T?ng Quan L?ch Tu?n
Khi nh?n nút, hi?n th? modal v?i:

#### Navigation Tu?n
- Nút **"Tu?n tr??c"**: Chuy?n v? tu?n tr??c ?ó
- Nút **"Tu?n sau"**: Chuy?n sang tu?n k? ti?p
- Nút **"Tu?n hi?n t?i"**: Quay v? tu?n hi?n t?i
- Hi?n th? **kho?ng ngày** c?a tu?n ?ang xem

#### Grid L?ch Làm Vi?c (7 Ngày x 3 Ca)
- **Header**: 7 ngày t? Th? 2 ??n Ch? Nh?t, kèm ngày tháng c? th?
- **3 Ca làm vi?c**:
  - ?? **Ca Sáng** (6h - 14h)
  - ?? **Ca Chi?u** (14h - 22h)
  - ?? **Ca ?êm** (22h - 6h)
- **M?i ô hi?n th?**:
  - Tên nhân viên ???c phân ca (d?ng chip)
  - Màu xanh lá n?u có ng??i, vàng n?u tr?ng
  - "Ch?a có ca" n?u ch?a phân công

#### Danh Sách Nhân Viên R?nh
- Hi?n th? t?t c? nhân viên **ch?a có l?ch** trong tu?n
- Layout d?ng grid responsive
- Avatar tròn + tên nhân viên
- Hover effect ??p m?t

#### Th?ng Kê
4 stat boxes hi?n th?:
- **T?ng ca ?ã x?p**: S? l??ng ca ?ã phân công
- **Nhân viên có l?ch**: S? nhân viên ?ã ???c x?p ca
- **Nhân viên r?nh**: S? nhân viên ch?a có l?ch
- **Ca tr?ng**: S? ô ca ch?a ???c phân công

#### Nút In L?ch
- In toàn b? l?ch tu?n ?? l?u tr? ho?c treo t??ng

---

## ?? T? ??ng ??ng B? L?ch

### Khi Nào L?ch ???c T? ??ng T?o?

H? th?ng **T? ??NG** t?o record trong b?ng `LichLamViec` khi:

#### 1. **T?o Ca ??n** (`CreateShift`)
```csharp
// Khi m? ca m?i cho nhân viên
POST /Admin/VehicleShift/CreateShift
```
- T?o ca trong `CaLamViec`
- **T? ??NG** t?o l?ch trong `LichLamViec`
- Xác ??nh lo?i ca d?a vào gi? b?t ??u

#### 2. **T?o Nhi?u Ca** (`CreateMultipleShifts`)
```csharp
// Khi l?p nhi?u ca cùng lúc
POST /Admin/VehicleShift/CreateMultipleShifts
```
- T?o nhi?u ca trong `CaLamViec`
- **T? ??NG** t?o l?ch cho t?ng ca
- Phù h?p khi l?p ca cho c? ngày

#### 3. **Phân Công Qu?y** (`SaveCounterAssignments`)
```csharp
// Khi phân công nhân viên vào qu?y tính ti?n
POST /Admin/VehicleShift/SaveCounterAssignments
```
- Phân công và kh?i ??ng ca ngay
- **T? ??NG** t?o l?ch cho nhân viên ???c phân công
- L?u thông tin qu?y vào ghi chú

---

## ??? Logic T? ??ng Phân Ca

### Helper Method: `CreateScheduleFromShift()`

```csharp
private async Task CreateScheduleFromShift(CaLamViec shift)
```

**Ch?c n?ng**:
1. L?y th?i gian nh?n ca t? `CaLamViec`
2. Xác ??nh lo?i ca (Sáng/Chi?u/?êm) d?a trên gi?
3. Tính gi? k?t thúc d? ki?n
4. Ki?m tra xem ?ã có l?ch ch?a (tránh trùng)
5. T?o record m?i trong `LichLamViec`

**Xác ??nh lo?i ca**:
```csharp
private int DetermineShiftType(TimeOnly time)
{
    var hour = time.Hour;
    
    if (hour >= 6 && hour < 14)
        return 0; // Ca sáng
    else if (hour >= 14 && hour < 22)
        return 1; // Ca chi?u
    else
        return 2; // Ca ?êm
}
```

**Gi? k?t thúc m?c ??nh**:
- Ca Sáng: 6h ? 14h (8 gi?)
- Ca Chi?u: 14h ? 22h (8 gi?)
- Ca ?êm: 22h ? 6h sáng hôm sau (8 gi?)

---

## ?? C?u Trúc B?ng `LichLamViec`

```sql
CREATE TABLE LichLamViec (
    MaLich INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT NOT NULL,
    NgayLamViec DATE NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    LoaiCa INT, -- 0: Sáng, 1: Chi?u, 2: ?êm
    TrangThai INT, -- 1: ?ã xác nh?n, 0: Ch? xác nh?n
    GhiChu NVARCHAR(255)
);
```

**Tr??ng quan tr?ng**:
- `MaNhanVien`: ID nhân viên
- `NgayLamViec`: Ngày làm vi?c (DATE)
- `LoaiCa`: Lo?i ca (0/1/2)
- `TrangThai`: 1 = ?ã xác nh?n (vì ?ã b?t ??u ca)
- `GhiChu`: Ghi chú t? ??ng (ví d?: "T? ??ng t?o t? ca #123")

---

## ?? Backend API

### Action: `GetWeeklyOverview`
```csharp
[HttpGet]
public async Task<IActionResult> GetWeeklyOverview(string weekStart)
```

**Input**: 
- `weekStart`: ISO date string c?a th? 2 trong tu?n

**Output**:
```json
{
  "success": true,
  "data": {
    "schedules": [
      {
        "maLich": 1,
        "maNhanVien": 5,
        "tenNhanVien": "Nguy?n V?n A",
        "ngayLamViec": "2024-01-15",
        "caLamViec": 0,
        "ghiChu": "T? ??ng t?o t? ca #45"
      }
    ],
    "freeEmployees": [
      {
        "maNhanVien": 8,
        "hoTen": "Tr?n Th? B",
        "chucVu": 2
      }
    ],
    "weekStart": "2024-01-15T00:00:00",
    "weekEnd": "2024-01-21T00:00:00"
  }
}
```

---

## ?? Giao Di?n

### CSS Classes Quan Tr?ng

```css
/* Grid Container */
.weekly-grid-container
.weekly-grid-header
.shift-row
.shift-cell

/* Employee Chips */
.employee-chip
.free-employee-chip

/* Status Colors */
.shift-cell.empty     /* Vàng - ch?a có ca */
.shift-cell.filled    /* Xanh - ?ã có ca */

/* Button */
.btn-info-admin       /* Purple gradient */
```

---

## ? L?i Ích

### Cho Qu?n Lý
? **Nhìn t?ng quan** l?ch c? tu?n trong 1 màn hình  
? **D? dàng phát hi?n** ca tr?ng ho?c nhân viên r?nh  
? **In l?ch** ?? treo t??ng ho?c l?u tr?  
? **Chuy?n tu?n** d? dàng ?? xem l?ch s? và t??ng lai  

### Cho H? Th?ng
? **T? ??ng ??ng b?** - không c?n nh?p 2 l?n  
? **Tránh trùng l?p** - ki?m tra tr??c khi t?o  
? **Linh ho?t** - h? tr? nhi?u cách t?o ca  
? **D? b?o trì** - logic t?p trung trong helper method  

---

## ?? Cách S? D?ng

### Quy Trình Làm Vi?c

1. **M? trang Danh Sách Nhân Viên**
   ```
   /Admin/VehicleShift/EmployeeList
   ```

2. **Nh?n "Xem t?ng quan tu?n"**
   - Modal hi?n th? l?ch tu?n hi?n t?i

3. **Xem l?ch và nhân viên r?nh**
   - Grid hi?n th? ai làm ca nào
   - Danh sách nhân viên r?nh ? d??i

4. **Chuy?n tu?n n?u c?n**
   - Nh?n "Tu?n tr??c" ho?c "Tu?n sau"
   - Nh?n "Tu?n hi?n t?i" ?? quay v?

5. **In l?ch** (tùy ch?n)
   - Nh?n "In l?ch tu?n" ?? in ra

---

## ?? L?u Ý K? Thu?t

### X? Lý L?i
- Helper method `CreateScheduleFromShift()` **không throw exception**
- Ch? log error ?? không ?nh h??ng vi?c t?o ca
- N?u t?o l?ch th?t b?i, ca v?n ???c t?o bình th??ng

### Performance
- Query ch? l?y d? li?u 1 tu?n (7 ngày)
- S? d?ng `DateOnly` và `TimeOnly` cho hi?u su?t t?t
- Index trên `LichLamViec(NgayLamViec, MaNhanVien)` ???c khuy?n ngh?

### T??ng Thích
- .NET 8.0+
- SQL Server 2019+
- Bootstrap 5 cho modal
- jQuery cho AJAX

---

## ?? Nâng C?p T??ng Lai

### Có Th? Thêm
- [ ] Click vào ô ?? **ch?nh s?a ca**
- [ ] Drag & drop ?? **di chuy?n ca**
- [ ] **Export Excel** l?ch tu?n
- [ ] **Thông báo** cho nhân viên v? l?ch làm
- [ ] **L?c** theo nhân viên ho?c lo?i ca
- [ ] **So sánh** nhi?u tu?n c?nh nhau
- [ ] **Tính l??ng** d?a trên l?ch tu?n

---

## ?? H? Tr?

N?u có v?n ??:
1. Ki?m tra Console log trong trình duy?t
2. Ki?m tra b?ng `LichLamViec` có d? li?u không
3. Verify r?ng action `GetWeeklyOverview` ho?t ??ng
4. Ki?m tra permission c?a user hi?n t?i

---

## ?? Version History

### v1.0.0 - 2024
- ? T?o modal xem t?ng quan tu?n
- ? Grid 7 ngày x 3 ca
- ? Danh sách nhân viên r?nh
- ? Th?ng kê ca làm vi?c
- ? T? ??ng ??ng b? l?ch khi t?o ca
- ? Helper methods cho logic phân ca

---

**Tác gi?**: GitHub Copilot  
**Ngày t?o**: 2024  
**Phiên b?n**: 1.0.0
