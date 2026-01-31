# Ch?c n?ng Qu?n lý Nhân viên - VehicleShift Module

## ? T?ng quan

Tôi ?ã code hoàn ch?nh ch?c n?ng **Qu?n lý Nhân viên** v?i các tính n?ng:

### ? ?ã hoàn thành:

1. **Danh sách Nhân viên** (`/Admin/VehicleShift/EmployeeList`)
   - Hi?n th? grid card responsive
   - 4 th? th?ng kê t?ng quan
   - B? l?c theo ch?c v?, tr?ng thái
   - Tìm ki?m theo tên, S?T
   - Hi?n th? thông tin c? b?n trên card

2. **Chi ti?t Nhân viên** (Modal popup)
   - Thông tin cá nhân ??y ??
   - 7 th?ng kê làm vi?c v?i gradient colors
   - L?ch s? 10 ca làm vi?c g?n nh?t
   - UI hi?n ??i, responsive
   - Loading state v?i spinner

3. **Controller Actions**
   - `EmployeeList()`: Hi?n th? danh sách
   - `GetEmployeeDetail(int id)`: API l?y chi ti?t + stats
   - Tính toán th?ng kê t?ng th? và theo tháng

4. **ViewModels**
   - `EmployeeViewModel`: Thông tin c? b?n
   - `EmployeeDetailViewModel`: Chi ti?t + stats
   - `EmployeeStatsViewModel`: 8 metrics th?ng kê

5. **Styling**
   - CSS hoàn ch?nh trong `vehicle-shift.css`
   - Responsive breakpoints
   - Gradient backgrounds cho stat boxes
   - Hover effects và animations

## ?? Files ?ã thay ??i:

### 1. Controller
```
QuanLyBaiDoXe\Areas\Admin\Controllers\VehicleShiftController.cs
```
**Thêm m?i:**
- Action `GetEmployeeDetail(int id)` - API l?y chi ti?t nhân viên
- Action `PrintShiftReport(int id)` - In báo cáo ca (TODO)

### 2. ViewModels
```
QuanLyBaiDoXe\Areas\Admin\ViewModels\VehicleShiftViewModel.cs
```
**Thêm m?i:**
- `EmployeeDetailViewModel`
- `EmployeeStatsViewModel`

### 3. View
```
QuanLyBaiDoXe\Areas\Admin\Views\VehicleShift\EmployeeList.cshtml
```
**C?p nh?t:**
- JavaScript function `viewEmployeeDetail()` v?i AJAX call
- Render HTML ??ng cho modal
- X? lý loading state và error handling

### 4. CSS
```
QuanLyBaiDoXe\wwwroot\css\vehicle-shift.css
```
**Thêm m?i 300+ dòng CSS:**
- `.employee-avatar-xlarge`
- `.employee-detail-container`
- `.stat-box` v?i 7 variants màu
- `.employee-grid`, `.employee-card`
- `.info-item`, `.detail-item`
- Responsive styles

### 5. Documentation
```
QuanLyBaiDoXe\Areas\Admin\Views\VehicleShift\EMPLOYEE_MANAGEMENT_GUIDE.md
```
**Tài li?u ??y ??:**
- H??ng d?n s? d?ng
- C?u trúc code
- Business logic
- Testing guide

## ?? Cách s? d?ng:

### 1. Truy c?p trang danh sách:
```
URL: /Admin/VehicleShift/EmployeeList
```

### 2. Các thao tác:

**A. L?c và tìm ki?m:**
- Dropdown "Ch?n ch?c v?": L?c B?o v?, Thu ngân, Giám sát, Qu?n lý
- Dropdown "Tr?ng thái": ?ang làm/?ã ngh?
- Search box: Nh?p tên ho?c S?T

**B. Xem chi ti?t:**
1. Click button "Xem chi ti?t" trên card nhân viên
2. Modal hi?n th? v?i:
   - Thông tin cá nhân
   - 7 th?ng kê (stat boxes)
   - B?ng l?ch s? 10 ca g?n nh?t

**C. Xem l?ch làm vi?c:**
- Click button "L?ch làm vi?c"
- Chuy?n ??n trang WeeklySchedule c?a nhân viên ?ó

## ?? Th?ng kê hi?n th?:

### Trang danh sách:
1. **T?ng nhân viên**: Count all
2. **?ang làm vi?c**: TrangThaiLamViec = true
3. **?ã ngh? vi?c**: TrangThaiLamViec = false
4. **Qu?n lý**: ChucVu >= 3

### Modal chi ti?t (7 metrics):
1. **T?ng ca**: T?t c? ca ?ã làm
2. **Ca ?ang tr?c**: TrangThaiCa = 0
3. **T?ng gi? làm**: Sum c?a (GiaoCa - NhanCa)
4. **TB gi?/ca**: Trung bình
5. **Ca tháng này**: Ca trong tháng hi?n t?i
6. **Gi? tháng này**: Gi? làm trong tháng
7. **Doanh thu**: T?ng TongTienHeThong

## ?? UI Features:

### 1. Employee Cards
```
???????????????????????????
?   [Avatar]   [Status]   ?
?   Nguy?n V?n A          ?
?   Mã NV: NV0001         ?
?                         ?
? ?? Qu?n lý   ?? 09xxx   ?
? ?? 25 tu?i   ?? Vào làm?
? ?? ??a ch?...          ?
?                         ?
? [Xem chi ti?t] [L?ch]  ?
???????????????????????????
```

### 2. Detail Modal
```
????????????????????????????????????????
?  [X] Thông tin nhân viên             ?
????????????????????????????????????????
?                                      ?
?  [Large Avatar]    Thông tin cá nhân?
?  Nguy?n V?n A     • Ch?c v?: Qu?n lý ?
?  NV0001           • Gi?i tính: Nam   ?
?  [Active]         • Ngày sinh: ...   ?
?                   • S?T: ...         ?
?                   • ??a ch?: ...     ?
?                                      ?
?  ?? Th?ng kê làm vi?c                ?
?  [100 ca] [5 ca] [250h] [2.5h/ca]   ?
?  [20 ca] [50h] [1,000,000?]         ?
?                                      ?
?  ?? L?ch s? ca làm vi?c (10 ca)     ?
?  ??????????????????????????????    ?
?  ? Mã  ?Nh?n ?Giao ?Gi?  ? TT ?    ?
?  ??????????????????????????????    ?
?                                      ?
????????????????????????????????????????
?          [?óng]  [Ch?nh s?a]        ?
????????????????????????????????????????
```

## ?? API Endpoints:

### GET /Admin/VehicleShift/GetEmployeeDetail
**Parameters:**
- `id` (int): Mã nhân viên

**Response:**
```json
{
  "success": true,
  "data": {
    "employee": {
      "maNhanVien": 1,
      "hoTen": "Nguy?n V?n A",
      "chucVu": 1,
      "chucVuText": "Qu?n lý",
      "tuoi": 25,
      ...
    },
    "recentShifts": [
      {
        "maCa": 123,
        "thoiGianNhanCa": "2024-01-15T08:00:00",
        "thoiGianGiaoCa": "2024-01-15T16:00:00",
        "soGioLam": 8.0,
        "tongTienHeThong": 500000,
        "trangThaiCa": 1
      }
    ],
    "stats": {
      "totalShifts": 100,
      "activeShifts": 5,
      "completedShifts": 95,
      "totalWorkHours": 800.5,
      "totalRevenue": 50000000,
      "averageShiftHours": 8.4,
      "currentMonthShifts": 20,
      "currentMonthHours": 160.0
    }
  }
}
```

## ?? Code Examples:

### Controller - GetEmployeeDetail
```csharp
[HttpGet]
public async Task<IActionResult> GetEmployeeDetail(int id)
{
    // 1. Query employee info
    var employee = await _context.NhanViens
        .Where(nv => nv.MaNhanVien == id)
        .Select(nv => new EmployeeViewModel { ... })
        .FirstOrDefaultAsync();
    
    // 2. Get 10 recent shifts
    var recentShifts = await _context.CaLamViecs
        .Where(c => c.MaNhanVien == id)
        .OrderByDescending(c => c.ThoiGianNhanCa)
        .Take(10)
        .Select(c => new ShiftViewModel { ... })
        .ToListAsync();
    
    // 3. Calculate stats
    var stats = new EmployeeStatsViewModel {
        TotalShifts = allShifts.Count,
        TotalWorkHours = (decimal)allShifts.Sum(...),
        AverageShiftHours = ...,
        // ...
    };
    
    return Json(new { success = true, data = ... });
}
```

### JavaScript - View Detail
```javascript
function viewEmployeeDetail(employeeId) {
    $('#employeeDetailModal').modal('show');
    $('#employeeDetailContent').html('Loading...');
    
    $.get('/Admin/VehicleShift/GetEmployeeDetail', 
          { id: employeeId }, 
          function(response) {
        if (response.success) {
            // Render HTML with response.data
            $('#employeeDetailContent').html(html);
        }
    });
}
```

## ?? CSS Classes:

### Stat Box Colors:
```css
.stat-box.bg-teal     /* #21A691 - Teal */
.stat-box.bg-success  /* #28a745 - Green */
.stat-box.bg-info     /* #17a2b8 - Cyan */
.stat-box.bg-orange   /* #fd7e14 - Orange */
.stat-box.bg-purple   /* #6f42c1 - Purple */
.stat-box.bg-blue     /* #0d6efd - Blue */
.stat-box.bg-green    /* #198754 - Green */
```

## ?? Responsive:

### Desktop (>768px)
- Grid: 3-4 columns
- Stat boxes: 3-4 per row
- Large modal

### Mobile (<768px)
- Grid: 1 column
- Stat boxes: 1-2 per row
- Full-width modal
- Smaller avatars

## ?? Known Issues:

1. ? Fixed: CS0111 duplicate EmployeeList action
2. ? TODO: Edit employee functionality
3. ? TODO: Add employee functionality
4. ? TODO: Print report implementation

## ?? Next Steps:

### Phase 2 - Enhancements:
1. **Ch?nh s?a nhân viên**
   - Edit modal v?i form validation
   - Update API endpoint
   
2. **Thêm nhân viên m?i**
   - Create modal
   - File upload cho avatar
   
3. **Export báo cáo**
   - Excel export
   - PDF report
   
4. **Phân quy?n**
   - Role-based access
   - Permission checks

## ?? Support:

- Documentation: `EMPLOYEE_MANAGEMENT_GUIDE.md`
- Code Location: `Areas/Admin/Controllers/VehicleShiftController.cs`
- View: `Areas/Admin/Views/VehicleShift/EmployeeList.cshtml`

---

**Status**: ? Completed  
**Build**: ? Successful  
**Testing**: ? Ready for QA  
**Version**: 1.0.0

