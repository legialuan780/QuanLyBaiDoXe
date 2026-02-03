# H??ng d?n Qu?n lý Nhân viên - VehicleShift

## ?? T?ng quan

Module Qu?n lý Nhân viên cung c?p các ch?c n?ng qu?n lý toàn di?n thông tin nhân viên, l?ch s? làm vi?c, và th?ng kê hi?u su?t.

## ?? Các ch?c n?ng chính

### 1. Danh sách Nhân viên (`/Admin/VehicleShift/EmployeeList`)

#### Th?ng kê t?ng quan
- **T?ng nhân viên**: T?ng s? nhân viên trong h? th?ng
- **?ang làm vi?c**: S? nhân viên ?ang ho?t ??ng
- **?ã ngh? vi?c**: S? nhân viên ?ã ngh?
- **Qu?n lý**: S? nhân viên c?p qu?n lý (ChucVu >= 3)

#### B? l?c
- **L?c theo ch?c v?**:
  - B?o v? (ChucVu = 1)
  - Thu ngân (ChucVu = 2)
  - Giám sát (ChucVu = 3)
  - Qu?n lý (ChucVu = 4)
  
- **L?c theo tr?ng thái**:
  - ?ang làm vi?c
  - ?ã ngh? vi?c

- **Tìm ki?m**: Tìm theo tên ho?c s? ?i?n tho?i

#### Hi?n th? danh sách
M?i th? nhân viên bao g?m:
- Avatar v?i ch? cái ??u tên
- Tr?ng thái làm vi?c (badge)
- H? tên và mã nhân viên
- Ch?c v?
- S? ?i?n tho?i
- Tu?i
- Ngày vào làm
- ??a ch? (n?u có)
- 2 nút hành ??ng:
  - **Xem chi ti?t**: Hi?n th? thông tin ??y ??
  - **L?ch làm vi?c**: Chuy?n ??n trang l?ch tu?n

### 2. Chi ti?t Nhân viên

Khi click "Xem chi ti?t", modal hi?n th?:

#### A. Thông tin cá nhân
- Avatar l?n
- H? tên ??y ??
- Mã nhân viên (format: NV0001)
- Tr?ng thái làm vi?c
- Ch?c v?
- Gi?i tính
- Ngày sinh và tu?i
- S? ?i?n tho?i
- Ngày vào làm
- ??a ch?

#### B. Th?ng kê làm vi?c

**Th?ng kê t?ng quan:**
- **T?ng ca**: T?ng s? ca ?ã làm
- **Ca ?ang tr?c**: S? ca ?ang trong tr?ng thái làm vi?c
- **T?ng gi? làm**: T?ng s? gi? làm vi?c
- **TB gi?/ca**: Trung bình gi? làm m?i ca

**Th?ng kê tháng hi?n t?i:**
- **Ca tháng này**: S? ca làm trong tháng
- **Gi? tháng này**: T?ng gi? làm trong tháng
- **Doanh thu**: T?ng doanh thu t? các ca

#### C. L?ch s? ca làm vi?c (10 ca g?n nh?t)
B?ng hi?n th?:
- Mã ca
- Gi? nh?n ca (format: dd/MM/yyyy HH:mm)
- Gi? giao ca (format: dd/MM/yyyy HH:mm)
- S? gi? làm
- Doanh thu
- Tr?ng thái (badge)

## ?? C?u trúc Code

### 1. Controller Actions

```csharp
// VehicleShiftController.cs

// Hi?n th? danh sách nhân viên
public async Task<IActionResult> EmployeeList()

// API: L?y chi ti?t nhân viên v?i th?ng kê
[HttpGet]
public async Task<IActionResult> GetEmployeeDetail(int id)
```

### 2. ViewModels

```csharp
// VehicleShiftViewModel.cs

// Thông tin c? b?n nhân viên
public class EmployeeViewModel
{
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; }
    public string? GioiTinh { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public int ChucVu { get; set; }
    public DateOnly? NgayVaoLam { get; set; }
    public bool TrangThaiLamViec { get; set; }
    
    // Computed properties
    public string ChucVuText { get; }
    public int Tuoi { get; }
}

// Chi ti?t nhân viên v?i th?ng kê
public class EmployeeDetailViewModel
{
    public EmployeeViewModel Employee { get; set; }
    public List<ShiftViewModel> RecentShifts { get; set; }
    public EmployeeStatsViewModel Stats { get; set; }
}

// Th?ng kê nhân viên
public class EmployeeStatsViewModel
{
    public int TotalShifts { get; set; }
    public int ActiveShifts { get; set; }
    public int CompletedShifts { get; set; }
    public decimal TotalWorkHours { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageShiftHours { get; set; }
    public int CurrentMonthShifts { get; set; }
    public decimal CurrentMonthHours { get; set; }
}
```

### 3. View - EmployeeList.cshtml

**Các components chính:**
- Stats cards (4 th? th?ng kê)
- Filter row (b? l?c)
- Employee grid (hi?n th? d?ng card grid)
- Employee detail modal (modal chi ti?t)

**JavaScript functions:**
```javascript
filterEmployees()      // L?c theo ch?c v? và tr?ng thái
searchEmployees()      // Tìm ki?m theo text
viewEmployeeDetail(id) // Xem chi ti?t nhân viên (AJAX)
viewEmployeeSchedule(id) // Chuy?n ??n trang l?ch
editEmployee()         // Ch?nh s?a (TODO)
```

### 4. CSS Styles - vehicle-shift.css

**Classes m?i:**
- `.employee-grid`: Grid layout cho danh sách
- `.employee-card`: Th? nhân viên
- `.employee-avatar-xlarge`: Avatar l?n cho modal
- `.employee-detail-container`: Container chi ti?t
- `.stat-box`: Box th?ng kê v?i màu gradient
- `.info-item`: Item thông tin v?i icon
- Responsive breakpoints

## ?? Cách th?c ho?t ??ng

### Flow xem chi ti?t nhân viên:

1. User click "Xem chi ti?t" trên card nhân viên
2. JavaScript g?i `viewEmployeeDetail(employeeId)`
3. Modal hi?n th? v?i loading spinner
4. AJAX GET request ??n `/Admin/VehicleShift/GetEmployeeDetail?id={id}`
5. Controller th?c hi?n:
   - Query thông tin nhân viên t? `NhanViens`
   - Query 10 ca g?n nh?t t? `CaLamViecs`
   - Tính toán th?ng kê t? t?t c? ca làm vi?c
   - Tính th?ng kê tháng hi?n t?i
   - Tính trung bình gi? làm
6. Tr? v? JSON v?i structure:
```json
{
  "success": true,
  "data": {
    "employee": { ... },
    "recentShifts": [ ... ],
    "stats": { ... }
  }
}
```
7. JavaScript render HTML ??ng vào modal
8. Hi?n th? thông tin ??y ?? cho user

## ?? UI/UX Features

### 1. Employee Cards
- Hover effect: N?i lên v?i shadow
- Color-coded status badges
- Avatar v?i gradient background
- Layout responsive

### 2. Detail Modal
- Large modal (modal-lg)
- Organized sections v?i icons
- Color-coded stat boxes v?i gradients
- Responsive table cho l?ch s? ca

### 3. Stat Boxes
- 7 màu khác nhau cho các metrics
- Hover effect: Lift up
- Gradient backgrounds
- Icon và s? li?u rõ ràng

## ?? Tích h?p v?i các module khác

### 1. VehicleShift Index
- Dropdown "Ch?n nhân viên" khi m? ca m?i
- S? d?ng `GetAvailableEmployees()` API

### 2. WeeklySchedule
- Hi?n th? l?ch tu?n c?a nhân viên c? th?
- Link t? button "L?ch làm vi?c"

### 3. TimeSheet
- B?ng ch?m công chi ti?t theo tháng
- Tính công, tính l??ng

## ?? Business Logic

### Ch?c v? (ChucVu)
- 0: Admin
- 1: Qu?n lý
- 2: B?o v?
- 3: K? thu?t
- 4: Nhân viên

### Tr?ng thái ca (TrangThaiCa)
- 0: ?ang tr?c
- 1: ?ã ch?t
- 2: Ng?t ca (n?u có)

### Tính toán
- **S? gi? làm**: `ThoiGianGiaoCa - ThoiGianNhanCa`
- **Trung bình**: `TotalWorkHours / CompletedShifts`
- **Chênh l?ch**: `TienMatBanGiao - (TienDauCa + TongTienHeThong)`

## ?? Responsive Design

### Desktop (> 768px)
- Employee grid: 3-4 columns
- Modal: Large width
- Full feature display

### Tablet (768px)
- Employee grid: 2 columns
- Modal: Medium width
- Adjusted stat boxes

### Mobile (< 768px)
- Employee grid: 1 column
- Modal: Full width
- Stacked info items
- Smaller avatars và fonts

## ?? Future Enhancements

### Ch?c n?ng c?n b? sung:
1. ? **Xem chi ti?t**: ?ã hoàn thành
2. ? **Ch?nh s?a thông tin**: TODO
3. ? **Thêm nhân viên m?i**: TODO
4. ? **Export báo cáo**: TODO
5. ? **Phân quy?n**: TODO
6. ? **Qu?n lý tài kho?n**: TODO
7. ? **Upload ?nh ??i di?n**: TODO
8. ? **Tính l??ng t? ??ng**: TODO

## ?? Testing

### Test cases:
1. ? Hi?n th? danh sách nhân viên
2. ? L?c theo ch?c v?
3. ? L?c theo tr?ng thái
4. ? Tìm ki?m
5. ? Xem chi ti?t
6. ? Responsive trên mobile
7. ? X? lý l?i khi không tìm th?y nhân viên
8. ? X? lý l?i network

## ?? Notes

- **Performance**: S? d?ng `.Select()` thay vì `.ToList()` ?? optimize query
- **Security**: C?n thêm authorization cho sensitive data
- **Caching**: Consider caching employee list
- **Pagination**: Nên thêm pagination n?u s? l??ng nhân viên l?n

---

**Version**: 1.0  
**Last Updated**: 2024  
**Developer**: Team QuanLyBaiDoXe
