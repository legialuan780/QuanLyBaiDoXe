# Hướng dẫn Phân quyền Nhân viên

## Tổng quan
Hệ thống có 3 loại quyền chính:
- **Admin**: Toàn quyền quản lý hệ thống
- **Nhân viên (Employee)**: Quản lý vận hành bãi xe, soát vé
- **Khách hàng (Customer)**: Sử dụng dịch vụ gửi xe, đặt chỗ

## Phân quyền Nhân viên

### Chức năng ĐƯỢC truy cập:

#### 1. Vận hành (Operations)
- ✅ **Tổng quan** - Dashboard
- ✅ **Giám sát cổng** - VehicleEntry
- ✅ **Lịch sử gửi xe** - VehicleHistory
- ✅ **Giám sát bãi xe** - VehicleVision

#### 2. Thẻ & Khách hàng (Cards & Customers)
- ✅ **Danh sách thẻ** - Card
- ✅ **Vé tháng** - MonthlyTicket
- ✅ **Khách hàng** - Customer

#### 3. Tài chính (Finance)
- ✅ **Bảng giá** - Pricing
- ✅ **Loại xe** - VehicleType
- ✅ **Block thời gian** - Pricing/TimeBlocks
- ✅ **Ca làm việc** - VehicleShift
  - Danh sách ca
  - Lịch trực
  - Danh sách nhân viên

#### 4. Xử lý sự cố (Anomaly Handling) ⭐ MỚI
- ✅ **Quản lý sự cố** - VehicleAnomaly
  - Xem danh sách sự cố chưa được assign hoặc của mình
  - Nhận sự cố để xử lý (TakeAnomaly)
  - Cập nhật trạng thái sự cố của mình
  - Báo hoàn thành sự cố (CompleteAnomaly)
  - ❌ KHÔNG được: Tạo mới, Xóa, Điều phối nhân viên (chỉ Admin)

### Chức năng BỊ GIỚI HẠN (Chỉ Admin):

#### 1. Báo cáo (Reports)
- ❌ **Báo cáo doanh thu** - Report Controller
  - Theo ngày
  - Theo tháng
  - Theo loại xe
  - Theo cổng

#### 2. Điều phối sự cố (Anomaly Assignment)
- ❌ **Điều phối nhân viên xử lý sự cố** - VehicleAnomaly.AssignStaff
- ❌ **Tạo mới hoặc xóa sự cố**

#### 3. Đặt chỗ (Bookings)
- ❌ **Quản lý đặt chỗ** - Booking

#### 4. Hệ thống (System)
- ❌ **Quản lý người dùng** - VehicleUser

## Cấu hình Controllers

### Controllers cho cả Admin và Employee:
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin,Employee")]
public class DashboardController : Controller { }
public class VehicleEntryController : Controller { }
public class VehicleHistoryController : Controller { }
public class VehicleVisionController : Controller { }
public class CardController : Controller { }
public class MonthlyTicketController : Controller { }
public class CustomerController : Controller { }
public class PricingController : Controller { }
public class VehicleTypeController : Controller { }
public class VehicleShiftController : Controller { }
public class VehicleAnomalyController : Controller { } // MỚI - Với logic phân quyền bên trong
```

### Controllers chỉ cho Admin:
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReportController : Controller { }
public class BookingController : Controller { }
public class VehicleUserController : Controller { }
```

### Actions đặc biệt trong VehicleAnomalyController:

**Cho Employee:**
```csharp
[HttpPost]
[Authorize(Roles = "Employee")]
public async Task<IActionResult> TakeAnomaly(int id) { } // Nhận sự cố

[HttpPost]
[Authorize(Roles = "Employee")]
public async Task<IActionResult> CompleteAnomaly(int id, string solutionNote) { } // Hoàn thành sự cố
```

**Chỉ cho Admin:**
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AssignStaff(int id, int staffId) { } // Điều phối nhân viên
```

## Workflow xử lý sự cố

### 1. Admin tạo/phát hiện sự cố
- Admin ghi nhận sự cố trong hệ thống
- Trạng thái: **Chưa xử lý** (TrangThaiXuLy = 0)
- MaNhanVien = null

### 2. Cách giao sự cố cho nhân viên

#### Cách 1: Admin chủ động điều phối
- Admin vào trang Sự cố
- Click "Điều phối" trên sự cố cần xử lý
- Chọn nhân viên từ danh sách
- Action: `AssignStaff(int id, int staffId)`
- Kết quả: 
  - MaNhanVien = staffId
  - TrangThaiXuLy = 1 (Đang xử lý)

#### Cách 2: Nhân viên tự nhận sự cố
- Nhân viên vào trang Sự cố
- Thấy danh sách sự cố chưa được assign (MaNhanVien = null)
- Click nút "Nhận xử lý"
- Action: `TakeAnomaly(int id)`
- Kết quả:
  - MaNhanVien = employee's ID
  - TrangThaiXuLy = 1 (Đang xử lý)

### 3. Nhân viên xử lý sự cố
- Nhân viên xem chi tiết sự cố
- Thực hiện xử lý thực tế (sửa barrier, tìm thẻ, kiểm tra camera...)
- Có thể cập nhật trạng thái trong quá trình xử lý

### 4. Nhân viên hoàn thành sự cố
- Nhân viên click "Hoàn thành"
- Nhập ghi chú về giải pháp đã thực hiện
- Action: `CompleteAnomaly(int id, string solutionNote)`
- Kết quả:
  - TrangThaiXuLy = 2 (Đã xử lý)
  - MoTaChiTiet được append thêm giải pháp và timestamp

### 5. Admin xem báo cáo
- Admin xem tất cả sự cố
- Xem chi tiết giải pháp nhân viên đã thực hiện
- Đánh giá hiệu quả xử lý

## Quy tắc phân quyền trong VehicleAnomalyController

### Employee:
- ✅ Xem: Chỉ sự cố của mình HOẶC chưa được assign
  ```csharp
  query = query.Where(s => s.MaNhanVien == null || s.MaNhanVien == employeeId);
  ```
- ✅ Nhận: Chỉ sự cố chưa được assign (MaNhanVien == null)
- ✅ Cập nhật: Chỉ sự cố của mình (MaNhanVien == employeeId)
- ✅ Hoàn thành: Chỉ sự cố của mình (MaNhanVien == employeeId)
- ❌ Điều phối: KHÔNG được phép

### Admin:
- ✅ Xem: TẤT CẢ sự cố
- ✅ Điều phối: Giao sự cố cho bất kỳ nhân viên nào
- ✅ Cập nhật: BẤT KỲ sự cố nào
- ✅ Tạo mới/Xóa: Quản lý toàn bộ

## Cách thức hoạt động

### 1. Sidebar động
File `_AdminSidebar.cshtml` sử dụng:
```csharp
@using System.Security.Claims
@{
    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
    var isAdmin = userRole == "Admin";
    var isEmployee = userRole == "Employee";
}

// Chỉ hiển thị menu cho Admin
@if (isAdmin)
{
    <div class="menu-category">Báo cáo</div>
    // ... menu items
}
```

### 2. Authorization Attribute
Controllers được bảo vệ bởi `[Authorize(Roles = "...")]`:
- Nếu nhân viên cố truy cập URL bị cấm → Redirect đến `/Account/AccessDenied`
- Trang hiển thị thông báo và nút quay về Dashboard

### 3. Claims trong Authentication
Khi đăng nhập, hệ thống tạo Claims:
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, account.MaTaiKhoan.ToString()),
    new Claim(ClaimTypes.Name, account.TenDangNhap),
    new Claim(ClaimTypes.Role, role) // "Admin" hoặc "Employee"
};
```

## Kiểm tra quyền trong Code

### Trong View (Razor):
```csharp
@if (User.IsInRole("Admin"))
{
    <button>Chức năng chỉ Admin</button>
}
```

### Trong Controller (C#):
```csharp
if (User.IsInRole("Admin"))
{
    // Logic chỉ cho Admin
}
```

### Kiểm tra nhiều roles:
```csharp
if (User.IsInRole("Admin") || User.IsInRole("Employee"))
{
    // Logic cho Admin và Employee
}
```

## Trang Access Denied

Đường dẫn: `/Account/AccessDenied`

Hiển thị khi:
- User cố truy cập trang không có quyền
- Thông báo role hiện tại của user
- Nút quay về Dashboard phù hợp với role
- Nút đăng xuất

## Testing

### Test Nhân viên:
1. Đăng nhập với tài khoản Nhân viên
2. Sidebar chỉ hiển thị các menu được phép
3. Thử truy cập URL bị cấm:
   - `/Admin/Report/Daily`
   - `/Admin/VehicleAnomaly`
   - `/Admin/Booking`
   - `/Admin/VehicleUser`
4. Kết quả: Redirect đến trang Access Denied

### Test Admin:
1. Đăng nhập với tài khoản Admin
2. Sidebar hiển thị đầy đủ tất cả menu
3. Có thể truy cập mọi trang

## Mở rộng

### Thêm controller mới:
1. Xác định ai được truy cập (Admin hoặc Admin+Employee)
2. Thêm `[Authorize(Roles = "...")]` attribute
3. Thêm menu vào `_AdminSidebar.cshtml` với điều kiện `@if (isAdmin)` nếu cần

### Phân quyền chi tiết hơn:
```csharp
[Authorize(Roles = "Admin")]
public IActionResult Create() { }

[Authorize(Roles = "Admin,Employee")]
public IActionResult Index() { }

[Authorize(Roles = "Admin,Employee")]
public IActionResult Details(int id) { }
```

## Lưu ý

1. **Bảo mật URL**: Authorization Attribute bảo vệ cả khi user biết URL trực tiếp
2. **UI/UX**: Sidebar ẩn menu để không gây nhầm lẫn cho nhân viên
3. **Thông báo rõ ràng**: Trang Access Denied giải thích tại sao không truy cập được
4. **Consistency**: Tất cả controllers đều có authorization, không có lỗ hổng

## Kết luận

Hệ thống phân quyền đảm bảo:
- ✅ Nhân viên có đủ quyền để vận hành bãi xe
- ✅ Chỉ Admin mới quản lý: Báo cáo, Sự cố, Đặt chỗ, Người dùng
- ✅ Bảo mật vừa UI (sidebar) vừa backend (authorization)
- ✅ Trải nghiệm người dùng tốt với thông báo rõ ràng
