# H??ng d?n s? d?ng ch?c n?ng ??ng nh?p

## T?ng quan
H? th?ng ??ng nh?p ?ã ???c cài ??t v?i các tính n?ng:
- Xác th?c ng??i dùng qua database
- Phân quy?n theo role (Admin, Employee, Customer)
- Cookie Authentication
- Session Management
- **M?t kh?u plain text (không mã hóa)** - D? dàng test và s? d?ng

## C?u trúc th?c thi

### 1. Services
- **IAuthService** và **AuthService**: X? lý logic xác th?c
  - AuthenticateAsync(): Xác th?c tài kho?n và m?t kh?u
  - HashPassword(): Tr? v? plain text (không mã hóa)
  - VerifyPassword(): So sánh plain text
  - ChangePasswordAsync(): ??i m?t kh?u

### 2. Controller
- **AccountController**:
  - GET Login: Hi?n th? trang ??ng nh?p
  - POST Login: X? lý ??ng nh?p
  - POST Logout: ??ng xu?t
  - GET AccessDenied: Trang t? ch?i truy c?p

### 3. Authentication Flow
1. Ng??i dùng nh?p username và password
2. AuthService ki?m tra trong database (b?ng TaiKhoan)
3. Ki?m tra tr?ng thái tài kho?n (TrangThai = true)
4. So sánh m?t kh?u tr?c ti?p (plain text)
5. Xác ??nh role:
   - N?u có NhanVien ? Admin ho?c Employee (tùy ChucVu)
   - N?u có KhachHang ? Customer
6. T?o Claims và Cookie
7. Redirect v? trang phù h?p

### 4. Phân quy?n
- **Admin/Employee**: Truy c?p ???c Area Admin
- **Customer**: Truy c?p ???c tính n?ng khách hàng
- Controllers trong Admin area c?n có attribute `[Authorize(Roles = "Admin,Employee")]`

## C?u hình Database

### B?ng TaiKhoan
- MaTaiKhoan (int): Primary Key
- TenDangNhap (varchar): Tên ??ng nh?p (unique)
- MatKhau (varchar): M?t kh?u **plain text** (không mã hóa)
- TrangThai (bit): true = ho?t ??ng, false = khóa

### B?ng NhanVien
- MaNhanVien (int): Primary Key
- MaTaiKhoan (int): Foreign Key ??n TaiKhoan
- HoTen (nvarchar): H? tên
- ChucVu (int): 0 = Admin, 1 = Nhân viên
- TrangThaiLamViec (bit): true = ?ang làm, false = ngh? vi?c

### B?ng KhachHang
- MaKhachHang (int): Primary Key
- MaTaiKhoan (int): Foreign Key ??n TaiKhoan
- HoTen (nvarchar): H? tên
- SoDienThoai (varchar): S? ?i?n tho?i

## Cách s? d?ng

### 1. T?o tài kho?n m?u (SQL)
```sql
-- T?o tài kho?n Admin
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
VALUES ('admin', 'admin123', 1);

DECLARE @MaTaiKhoan INT = SCOPE_IDENTITY();

INSERT INTO NhanVien (MaTaiKhoan, HoTen, ChucVu, TrangThaiLamViec, NgayVaoLam)
VALUES (@MaTaiKhoan, N'Qu?n tr? viên', 0, 1, GETDATE());

-- T?o tài kho?n Nhân viên
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
VALUES ('nhanvien', 'nv123', 1);

DECLARE @MaTaiKhoan2 INT = SCOPE_IDENTITY();

INSERT INTO NhanVien (MaTaiKhoan, HoTen, ChucVu, TrangThaiLamViec, NgayVaoLam)
VALUES (@MaTaiKhoan2, N'Nhân viên', 1, 1, GETDATE());

-- T?o tài kho?n Khách hàng
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
VALUES ('khachhang', 'kh123', 1);

DECLARE @MaTaiKhoan3 INT = SCOPE_IDENTITY();

INSERT INTO KhachHang (MaTaiKhoan, HoTen, SoDienThoai)
VALUES (@MaTaiKhoan3, N'Khách hàng', '0123456789');
```

### 2. ??ng nh?p
- URL: `/Account/Login` ho?c `/account/login`
- Nh?p:
  - Tên ??ng nh?p: admin, nhanvien, ho?c khachhang
  - M?t kh?u: t??ng ?ng (admin123, nv123, kh123)

**L?U Ý**: M?t kh?u ???c l?u d?ng plain text, nh?p ?úng nh? trong database

### 3. B?o v? Controller/Action
```csharp
[Authorize] // Yêu c?u ??ng nh?p
public IActionResult Index() { }

[Authorize(Roles = "Admin")] // Ch? Admin
public IActionResult AdminOnly() { }

[Authorize(Roles = "Admin,Employee")] // Admin ho?c Employee
public IActionResult Staff() { }
```

### 4. L?y thông tin ng??i dùng trong Controller
```csharp
// L?y user ID
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

// L?y username
var username = User.Identity?.Name;

// L?y role
var role = User.FindFirst(ClaimTypes.Role)?.Value;

// L?y thông tin custom
var fullName = User.FindFirst("FullName")?.Value;
var employeeId = User.FindFirst("EmployeeId")?.Value;
```

### 5. L?y thông tin ng??i dùng trong View
```razor
@if (User.Identity?.IsAuthenticated == true)
{
    <p>Xin chào, @User.Identity.Name</p>
    <p>Role: @User.FindFirst(ClaimTypes.Role)?.Value</p>
}

@if (User.IsInRole("Admin"))
{
    <a asp-area="Admin" asp-controller="Dashboard" asp-action="Index">Admin Panel</a>
}
```

## M?t kh?u Plain Text

**QUAN TR?NG**: H? th?ng s? d?ng m?t kh?u plain text (không mã hóa)

### ?u ?i?m
- ? D? test và debug
- ? Không c?n lo hash không kh?p
- ? ??ng nh?p ??n gi?n
- ? Có th? xem m?t kh?u tr?c ti?p trong database

### Nh??c ?i?m
- ?? Không b?o m?t cho production
- ?? Không nên s? d?ng cho h? th?ng th?c t?

### L?u ý b?o m?t
- Ch? phù h?p cho môi tr??ng development/test
- N?u deploy production, c?n implement password hashing
- Không expose database ra ngoài

## L?u ý
1. ??m b?o connection string trong appsettings.json ?úng
2. Session timeout: 2 gi?
3. Cookie timeout: 8 gi?
4. Ch? ?? Remember Me ?ã ???c b?t m?c ??nh
5. Anti-Forgery Token ???c s? d?ng ?? b?o v? CSRF attacks
6. **M?t kh?u plain text**: Nh?p ?úng nh? trong database ?? ??ng nh?p

## Testing
1. Ch?y script `CreateTestAccounts.sql` ?? t?o tài kho?n test
2. ??ng nh?p v?i:
   - **Admin**: username = `admin`, password = `admin123`
   - **Nhân viên**: username = `nhanvien`, password = `nv123`
   - **Khách hàng**: username = `khachhang`, password = `kh123`
3. Ki?m tra redirect ??n trang t??ng ?ng
4. ??ng xu?t và th? l?i v?i tài kho?n khác

## Troubleshooting

### Không ??ng nh?p ???c
1. Ki?m tra username có ?úng không (case-sensitive)
2. Ki?m tra password ph?i gi?ng **chính xác** trong database
3. Ki?m tra TrangThai trong b?ng TaiKhoan = 1
4. Ki?m tra TrangThaiLamViec (v?i nhân viên) = 1

### Ki?m tra m?t kh?u trong database
```sql
SELECT TenDangNhap, MatKhau, TrangThai 
FROM TaiKhoan 
WHERE TenDangNhap = 'admin';
```

### Reset m?t kh?u
```sql
UPDATE TaiKhoan 
SET MatKhau = 'newpassword' 
WHERE TenDangNhap = 'username';
