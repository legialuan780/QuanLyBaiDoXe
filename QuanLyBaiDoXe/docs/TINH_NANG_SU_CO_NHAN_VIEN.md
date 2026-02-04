# Tính năng Xử lý Sự cố cho Nhân viên

## 🎯 Tổng quan

Nhân viên giờ đây có thể:
- ✅ Xem danh sách sự cố cần xử lý (chưa assign hoặc đã assign cho mình)
- ✅ Tự nhận sự cố để xử lý
- ✅ Cập nhật trạng thái sự cố
- ✅ Báo hoàn thành sau khi giải quyết
- ✅ Ghi chú giải pháp đã thực hiện

Admin có thể:
- ✅ Xem tất cả sự cố
- ✅ Chủ động điều phối nhân viên xử lý sự cố
- ✅ Xem chi tiết giải pháp nhân viên đã thực hiện

## 📋 Workflow

```
[Sự cố mới]
     ↓
     ├─→ Admin điều phối → Nhân viên nhận được (TrangThai = Đang xử lý)
     │
     └─→ Nhân viên tự nhận → Nhân viên xử lý (TrangThai = Đang xử lý)
              ↓
         [Nhân viên xử lý thực tế]
              ↓
         Nhân viên hoàn thành + Ghi chú giải pháp
              ↓
         (TrangThai = Đã xử lý)
              ↓
         Admin xem báo cáo & đánh giá
```

## 🔧 Các thay đổi đã thực hiện

### 1. Controller: `VehicleAnomalyController.cs`

#### a. Cho phép Employee truy cập
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin,Employee")] // Thay vì chỉ Admin
public class VehicleAnomalyController : Controller
```

#### b. Logic lọc dữ liệu theo role
**Admin**: Xem tất cả sự cố
```csharp
var query = _context.SuCos
    .Include(s => s.MaNhanVienNavigation)
    .AsQueryable();
```

**Employee**: Chỉ xem sự cố của mình hoặc chưa assign
```csharp
if (isEmployee && employeeId > 0)
{
    query = query.Where(s => s.MaNhanVien == null || s.MaNhanVien == employeeId);
}
```

#### c. Actions mới cho Employee

**1. TakeAnomaly - Nhận sự cố**
```csharp
[HttpPost]
[Authorize(Roles = "Employee")]
public async Task<IActionResult> TakeAnomaly(int id)
{
    // Kiểm tra sự cố chưa được assign
    if (anomaly.MaNhanVien != null)
        return Json(new { success = false, message = "Sự cố đã được nhân viên khác nhận" });
    
    // Gán cho nhân viên hiện tại
    anomaly.MaNhanVien = employeeId;
    anomaly.TrangThaiXuLy = 1; // Đang xử lý
    
    await _context.SaveChangesAsync();
    return Json(new { success = true, message = "Nhận sự cố thành công" });
}
```

**2. CompleteAnomaly - Hoàn thành sự cố**
```csharp
[HttpPost]
[Authorize(Roles = "Employee")]
public async Task<IActionResult> CompleteAnomaly(int id, string solutionNote)
{
    // Kiểm tra quyền
    if (anomaly.MaNhanVien != employeeId)
        return Json(new { success = false, message = "Bạn không có quyền hoàn thành sự cố này" });
    
    // Cập nhật trạng thái
    anomaly.TrangThaiXuLy = 2; // Đã xử lý
    
    // Thêm ghi chú giải pháp
    if (!string.IsNullOrEmpty(solutionNote))
    {
        anomaly.MoTaChiTiet += $"\n\n--- Giải pháp ({DateTime.Now:dd/MM/yyyy HH:mm}) ---\n{solutionNote}";
    }
    
    await _context.SaveChangesAsync();
    return Json(new { success = true, message = "Đã hoàn thành sự cố" });
}
```

**3. UpdateStatus - Cập nhật trạng thái**
```csharp
[HttpPost]
public async Task<IActionResult> UpdateStatus(int id, int status, string note)
{
    // Employee chỉ được cập nhật sự cố của mình
    if (User.IsInRole("Employee"))
    {
        if (anomaly.MaNhanVien != employeeId)
            return Json(new { success = false, message = "Bạn không có quyền cập nhật sự cố này" });
    }
    
    anomaly.TrangThaiXuLy = status;
    await _context.SaveChangesAsync();
}
```

#### d. Giữ lại quyền Admin

**AssignStaff - Điều phối nhân viên (chỉ Admin)**
```csharp
[HttpPost]
[Authorize(Roles = "Admin")] // CHỈ ADMIN
public async Task<IActionResult> AssignStaff(int id, int staffId)
{
    anomaly.MaNhanVien = staffId;
    anomaly.TrangThaiXuLy = 1; // Đang xử lý
    
    await _context.SaveChangesAsync();
    return Json(new { success = true, message = "Điều nhân viên thành công" });
}
```

### 2. Sidebar: `_AdminSidebar.cshtml`

Thêm menu "Sự cố" cho cả Admin và Employee:

```razor
<!-- Nhóm 4: Xử lý sự cố - CHO CẢ ADMIN VÀ EMPLOYEE -->
<div class="menu-category">Xử lý</div>
<ul class="sidebar-nav">
    <li class="sidebar-nav-item">
        <a href="@Url.Action("Index", "VehicleAnomaly", new { area = "Admin" })" class="sidebar-nav-link">
            <i class="fas fa-exclamation-triangle"></i>
            <span>Sự cố</span>
            @if (isEmployee)
            {
                <span class="badge bg-danger" id="myAnomalyBadge" style="margin-left: auto; display: none;">0</span>
            }
        </a>
    </li>
</ul>
```

**Lưu ý:**
- Badge đỏ cho Employee hiển thị số sự cố cần xử lý (có thể implement bằng SignalR)
- Menu này hiển thị cho cả Admin và Employee
- Đã xóa menu Sự cố cũ trong phần Báo cáo (chỉ Admin)

### 3. Tài liệu: `PHAN_QUYEN_NHAN_VIEN.md`

Cập nhật đầy đủ:
- Danh sách chức năng Employee được phép
- Workflow xử lý sự cố chi tiết
- Quy tắc phân quyền trong code
- Ví dụ cách sử dụng

## 🔒 Bảo mật

### 1. Phân quyền Controller level
```csharp
[Authorize(Roles = "Admin,Employee")] // Controller level
```

### 2. Phân quyền Action level
```csharp
[Authorize(Roles = "Admin")] // Chỉ Admin
[Authorize(Roles = "Employee")] // Chỉ Employee
```

### 3. Phân quyền Data level
```csharp
// Trong code: Lọc dữ liệu theo role
if (isEmployee)
    query = query.Where(s => s.MaNhanVien == null || s.MaNhanVien == employeeId);
```

### 4. Kiểm tra quyền trong Action
```csharp
// Kiểm tra nhân viên chỉ được xử lý sự cố của mình
if (anomaly.MaNhanVien != employeeId)
    return Json(new { success = false, message = "Bạn không có quyền..." });
```

## 📊 Database: Trường `SuCo`

```sql
CREATE TABLE SuCo (
    MaSuCo INT PRIMARY KEY IDENTITY(1,1),
    ThoiGianGhiNhan DATETIME,
    MaNhanVien INT NULL,                  -- NULL = chưa assign
    LoaiSuCo NVARCHAR(50),                -- "Khẩn cấp", "Xe mất thẻ", "Lỗi camera"
    MaThe NVARCHAR(50),
    MaViTri INT,
    MoTaChiTiet NVARCHAR(MAX),            -- Bao gồm cả giải pháp (append khi hoàn thành)
    TrangThaiXuLy INT                     -- 0: Chưa xử lý, 1: Đang xử lý, 2: Đã xử lý
)
```

## 🎨 UI/UX Suggestions (Chưa implement)

### Trang Index cho Employee
```html
<!-- Sự cố chưa được assign -->
<div class="card">
    <div class="card-header">
        <h4>Sự cố cần xử lý</h4>
    </div>
    <div class="card-body">
        <!-- Danh sách sự cố chưa assign -->
        <button class="btn btn-primary" onclick="takeAnomaly(123)">
            <i class="fas fa-hand-paper"></i> Nhận xử lý
        </button>
    </div>
</div>

<!-- Sự cố của tôi -->
<div class="card mt-3">
    <div class="card-header">
        <h4>Sự cố của tôi</h4>
    </div>
    <div class="card-body">
        <!-- Danh sách sự cố đang xử lý -->
        <button class="btn btn-success" onclick="completeAnomaly(123)">
            <i class="fas fa-check"></i> Hoàn thành
        </button>
    </div>
</div>
```

### Modal hoàn thành sự cố
```html
<div class="modal" id="completeAnomalyModal">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5>Hoàn thành sự cố</h5>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label>Giải pháp đã thực hiện:</label>
                    <textarea class="form-control" id="solutionNote" rows="5" 
                              placeholder="Mô tả chi tiết cách bạn đã xử lý sự cố này..."></textarea>
                </div>
            </div>
            <div class="modal-footer">
                <button class="btn btn-secondary" data-dismiss="modal">Hủy</button>
                <button class="btn btn-success" onclick="submitComplete()">
                    <i class="fas fa-check"></i> Xác nhận hoàn thành
                </button>
            </div>
        </div>
    </div>
</div>
```

### JavaScript calls
```javascript
// Nhận sự cố
function takeAnomaly(id) {
    if (confirm('Bạn có chắc muốn nhận sự cố này?')) {
        $.post('/Admin/VehicleAnomaly/TakeAnomaly', { id: id })
            .done(function(response) {
                if (response.success) {
                    toastr.success(response.message);
                    location.reload();
                } else {
                    toastr.error(response.message);
                }
            });
    }
}

// Hoàn thành sự cố
function completeAnomaly(id) {
    $('#completeAnomalyModal').data('anomaly-id', id).modal('show');
}

function submitComplete() {
    var id = $('#completeAnomalyModal').data('anomaly-id');
    var note = $('#solutionNote').val();
    
    $.post('/Admin/VehicleAnomaly/CompleteAnomaly', { 
        id: id, 
        solutionNote: note 
    })
    .done(function(response) {
        if (response.success) {
            toastr.success(response.message);
            $('#completeAnomalyModal').modal('hide');
            location.reload();
        } else {
            toastr.error(response.message);
        }
    });
}
```

## 🧪 Testing Checklist

### Test Employee:
- [ ] Đăng nhập với tài khoản Employee
- [ ] Sidebar có hiển thị menu "Sự cố"
- [ ] Vào trang Sự cố, chỉ thấy:
  - [ ] Sự cố chưa assign (MaNhanVien = null)
  - [ ] Sự cố của mình (MaNhanVien = employee's ID)
- [ ] Click "Nhận xử lý" một sự cố chưa assign
  - [ ] Kiểm tra DB: MaNhanVien = employee's ID, TrangThaiXuLy = 1
- [ ] Thử cập nhật trạng thái sự cố của mình → Thành công
- [ ] Thử cập nhật trạng thái sự cố của người khác → Lỗi
- [ ] Click "Hoàn thành" với ghi chú giải pháp
  - [ ] Kiểm tra DB: TrangThaiXuLy = 2
  - [ ] Kiểm tra DB: MoTaChiTiet có thêm giải pháp + timestamp
- [ ] Thử truy cập `/Admin/VehicleAnomaly/AssignStaff` → Access Denied

### Test Admin:
- [ ] Đăng nhập với tài khoản Admin
- [ ] Vào trang Sự cố, thấy TẤT CẢ sự cố
- [ ] Tạo sự cố mới
- [ ] Click "Điều phối", chọn nhân viên → Thành công
  - [ ] Kiểm tra DB: MaNhanVien được set
- [ ] Xem chi tiết sự cố đã hoàn thành
  - [ ] Thấy giải pháp nhân viên đã ghi

### Test Integration:
- [ ] Admin tạo sự cố → Employee nhận → Employee hoàn thành → Admin xem báo cáo
- [ ] Admin điều phối → Employee xem thấy trong "Sự cố của tôi" → Xử lý

## 📈 Mở rộng trong tương lai

### 1. Thông báo Real-time (SignalR)
- Admin tạo sự cố → Push notification đến tất cả Employee
- Employee nhận sự cố → Thông báo cho Admin
- Employee hoàn thành → Thông báo cho Admin

### 2. Badge số lượng
```javascript
// Trong sidebar, hiển thị số sự cố cần xử lý
$(document).ready(function() {
    $.get('/Admin/VehicleAnomaly/GetMyPendingCount')
        .done(function(count) {
            if (count > 0) {
                $('#myAnomalyBadge').text(count).show();
            }
        });
});
```

### 3. Lịch sử xử lý sự cố
- Tạo bảng `SuCoLichSu` để log mọi thay đổi
- Xem ai đã làm gì, khi nào

### 4. Đánh giá hiệu suất
- Thời gian xử lý trung bình
- Số sự cố hoàn thành mỗi nhân viên
- Report về hiệu quả xử lý

### 5. Ưu tiên sự cố
- Thêm trường `MucDoUuTien` (1-5)
- Sắp xếp theo độ ưu tiên
- Nhân viên thấy sự cố ưu tiên cao trước

## 🎉 Kết luận

Tính năng xử lý sự cố cho nhân viên đã được implement hoàn chỉnh với:

✅ **Bảo mật**: Phân quyền chặt chẽ ở mọi cấp độ
✅ **Workflow rõ ràng**: Admin điều phối HOẶC Employee tự nhận
✅ **Trách nhiệm rõ ràng**: Employee chỉ xử lý sự cố của mình
✅ **Minh bạch**: Ghi nhận đầy đủ giải pháp và thời gian
✅ **Dễ mở rộng**: Sẵn sàng thêm real-time notifications và reporting

Nhân viên giờ đây có thể chủ động trong việc xử lý sự cố, giảm tải cho Admin, và tăng hiệu quả vận hành bãi xe! 🚗✨
