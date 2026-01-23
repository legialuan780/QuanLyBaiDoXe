# H??ng d?n s? d?ng ch?c n?ng ??ng ký

## T?ng quan
Ch?c n?ng ??ng ký cho phép ng??i dùng t?o tài kho?n khách hàng m?i v?i ??y ?? thông tin cá nhân.
**M?t kh?u ???c l?u d?ng plain text** (không mã hóa) ?? d? dàng s? d?ng và test.

## Tính n?ng chính

### 1. Form ??ng ký
- **Giao di?n**: T??ng t? trang ??ng nh?p v?i 2 ph?n (left/right)
- **Validation**: Ki?m tra d? li?u c? client-side và server-side
- **Real-time check**: Ki?m tra tên ??ng nh?p và s? ?i?n tho?i ?ã t?n t?i
- **Password**: L?u plain text (không mã hóa)

### 2. Thông tin yêu c?u

#### Thông tin b?t bu?c (*)
1. **Tên ??ng nh?p**
   - ?? dài: 3-50 ký t?
   - Unique: Không ???c trùng v?i tài kho?n khác
   - Real-time validation: Hi?n th? ? n?u có th? dùng, ? n?u ?ã t?n t?i

2. **M?t kh?u**
   - ?? dài t?i thi?u: 6 ký t?
   - **???c l?u d?ng plain text** (không mã hóa)

3. **Xác nh?n m?t kh?u**
   - Ph?i kh?p v?i m?t kh?u ?ã nh?p

4. **H? và tên**
   - ?? dài t?i ?a: 100 ký t?
   - S? d?ng ?? hi?n th? trong h? th?ng

5. **S? ?i?n tho?i**
   - Format: 0[3|5|7|8|9]XXXXXXXX (10 ch? s?)
   - Unique: Không ???c trùng
   - Real-time validation

#### Thông tin không b?t bu?c
1. **CCCD/CMND**
   - ?? dài t?i ?a: 20 ký t?
   - Unique n?u nh?p

2. **??a ch?**
   - ?? dài t?i ?a: 200 ký t?

3. **Bi?n s? xe m?c ??nh**
   - ?? dài t?i ?a: 20 ký t?
   - T? ??ng chuy?n thành ch? hoa
   - Có th? thêm sau

### 3. Flow ??ng ký

```
1. User ?i?n form
   ?
2. Client-side validation
   - Check format các tr??ng
   - Real-time check username/phone
   ?
3. Submit form
   ?
4. Server-side validation
   - Validate ModelState
   - Check username exists
   - Check phone exists
   - Check CCCD exists (n?u có)
   ?
5. T?o tài kho?n (Transaction)
   - Insert vào TaiKhoan (MatKhau = plain text)
   - Insert vào KhachHang
   ?
6. Redirect v? Login v?i thông báo thành công
```

## C?u trúc code

### 1. ViewModel - RegisterViewModel.cs
```csharp
public class RegisterViewModel
{
    [Required] public string Username { get; set; }
    [Required] public string Password { get; set; }
    [Required] public string ConfirmPassword { get; set; }
    [Required] public string FullName { get; set; }
    [Required] public string PhoneNumber { get; set; }
    public string? CCCD { get; set; }
    public string? Address { get; set; }
    public string? LicensePlate { get; set; }
}
```

### 2. Service Methods
```csharp
// IAuthService & AuthService
Task<(bool Success, string? ErrorMessage, int? CustomerId)> RegisterCustomerAsync(RegisterViewModel model);
Task<bool> UsernameExistsAsync(string username);
Task<bool> PhoneNumberExistsAsync(string phoneNumber);

// L?u m?t kh?u plain text
MatKhau = model.Password; // Plain text, không hash
```

### 3. Controller Actions
```csharp
// AccountController
[HttpGet] IActionResult Register()
[HttpPost] IActionResult Register(RegisterViewModel model)
[HttpGet] IActionResult CheckUsername(string username)
[HttpGet] IActionResult CheckPhoneNumber(string phoneNumber)
```

### 4. View - Register.cshtml
- Form v?i validation
- AJAX real-time check
- Auto uppercase cho bi?n s? xe
- Loading state khi submit

## API Endpoints

### 1. Ki?m tra tên ??ng nh?p
```
GET /Account/CheckUsername?username={username}

Response:
{
    "available": true/false
}
```

### 2. Ki?m tra s? ?i?n tho?i
```
GET /Account/CheckPhoneNumber?phoneNumber={phoneNumber}

Response:
{
    "available": true/false
}
```

## Database Changes

### B?ng TaiKhoan
```sql
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
VALUES ('{username}', '{plain_password}', 1);
-- L?u ý: MatKhau là plain text, không hash
```

### B?ng KhachHang
```sql
INSERT INTO KhachHang (
    MaTaiKhoan,
    SoDienThoai,
    HoTen,
    CCCD,
    DiaChi,
    BienSoXeMacDinh
)
VALUES (...);
```

## Validation Rules

### Client-side (HTML5 + jQuery)
- Required fields
- Min/Max length
- Pattern matching (phone number)
- Password confirmation
- Real-time availability check

### Server-side (Data Annotations + Custom Logic)
- Model validation
- Business rules:
  - Username unique
  - Phone unique
  - CCCD unique (if provided)
- Transaction safety

## Testing

### 1. Test case thành công
```
Input:
- Username: testuser
- Password: test123
- ConfirmPassword: test123
- FullName: Nguy?n Test
- PhoneNumber: 0912345678

Expected: 
- T?o tài kho?n thành công
- M?t kh?u l?u là "test123" (plain text)
- Redirect v? Login
- Có th? ??ng nh?p v?i username="testuser", password="test123"
```

### 2. Test case th?t b?i - Username ?ã t?n t?i
```
Input:
- Username: admin (?ã t?n t?i)

Expected:
- Hi?n th? l?i "Tên ??ng nh?p ?ã t?n t?i!"
- Form gi? l?i d? li?u ?ã nh?p
```

### 3. Test case th?t b?i - Phone ?ã t?n t?i
```
Input:
- PhoneNumber: 0912345678 (?ã t?n t?i)

Expected:
- Hi?n th? l?i "S? ?i?n tho?i ?ã ???c ??ng ký!"
```

### 4. Test case th?t b?i - Password không kh?p
```
Input:
- Password: test123
- ConfirmPassword: test456

Expected:
- Hi?n th? l?i "M?t kh?u xác nh?n không kh?p"
```

## Security

### 1. Password Storage
?? **QUAN TR?NG**: M?t kh?u ???c l?u d?ng **plain text** (không mã hóa)

**?u ?i?m**:
- ? D? test và debug
- ? Có th? xem m?t kh?u trong database
- ? ??ng nh?p ??n gi?n, không c?n lo hash

**Nh??c ?i?m**:
- ?? Không b?o m?t
- ?? Không phù h?p cho production
- ?? N?u database b? leak, t?t c? m?t kh?u b? l?

**Khuy?n ngh?**:
- Ch? dùng cho development/test
- Production c?n implement password hashing

### 2. CSRF Protection
- Anti-Forgery Token trong form
- ValidateAntiForgeryToken attribute

### 3. Input Validation
- XSS prevention (auto-encoded)
- SQL Injection prevention (EF Core parameterized queries)

### 4. Transaction Safety
- Database transaction cho insert operations
- Rollback n?u có l?i

## Usage

### 1. Truy c?p trang ??ng ký
```
URL: /Account/Register
ho?c
Click "??ng ký ngay" t? trang Login
```

### 2. ?i?n form
- Nh?p các thông tin b?t bu?c
- Ch? icon ? xu?t hi?n cho username và phone
- ?i?n thông tin b? sung n?u mu?n
- **L?u ý**: M?t kh?u s? ???c l?u chính xác nh? b?n nh?p (plain text)

### 3. Submit
- Click "??NG KÝ"
- Ch? x? lý (button s? hi?n th? loading)
- Redirect v? Login n?u thành công

### 4. ??ng nh?p
- S? d?ng username và password v?a t?o
- Nh?p **chính xác** password ?ã ??ng ký
- ??ng nh?p vào h? th?ng v?i role Customer

## Ví d? ??ng ký và ??ng nh?p

### ??ng ký
```
Username: nguyenvana
Password: 123456
ConfirmPassword: 123456
FullName: Nguy?n V?n A
PhoneNumber: 0912345678
```

### Sau ?ó ??ng nh?p
```
Username: nguyenvana
Password: 123456  (ph?i ?úng y nh? lúc ??ng ký)
```

### Ki?m tra trong database
```sql
SELECT TenDangNhap, MatKhau, TrangThai 
FROM TaiKhoan 
WHERE TenDangNhap = 'nguyenvana';

-- K?t qu? s? là:
-- TenDangNhap: nguyenvana
-- MatKhau: 123456  (plain text)
-- TrangThai: 1
```

## Customization

### 1. Thêm field m?i
```csharp
// 1. Thêm vào RegisterViewModel
public string? NewField { get; set; }

// 2. Thêm vào View
<div class="mb-3">
    <label asp-for="NewField" class="form-label">Label</label>
    <div class="input d-flex align-items-center">
        <span class="icon">??</span>
        <input asp-for="NewField" class="form-control border-0" />
    </div>
</div>

// 3. C?p nh?t AuthService.RegisterCustomerAsync
khachHang.NewField = model.NewField;
```

### 2. Thay ??i validation rules
```csharp
// Trong RegisterViewModel
[StringLength(20, MinimumLength = 8, ErrorMessage = "Custom message")]
[RegularExpression(@"pattern", ErrorMessage = "Custom message")]
```

## Troubleshooting

### L?i: "Tên ??ng nh?p ?ã t?n t?i"
- Ki?m tra database: `SELECT * FROM TaiKhoan WHERE TenDangNhap = '{username}'`
- Th? username khác

### L?i: "S? ?i?n tho?i ?ã ???c ??ng ký"
- Ki?m tra database: `SELECT * FROM KhachHang WHERE SoDienThoai = '{phone}'`
- S? d?ng s? khác

### Real-time check không ho?t ??ng
- Ki?m tra jQuery ?ã load
- Ki?m tra console browser có l?i không
- Verify API endpoints ho?t ??ng

### Form không submit
- Ki?m tra validation errors
- Check browser console
- Verify anti-forgery token

### Không ??ng nh?p ???c sau khi ??ng ký
- Ki?m tra TrangThai = 1 trong b?ng TaiKhoan
- Nh?p **chính xác** password ?ã ??ng ký (case-sensitive)
- Ki?m tra username không có kho?ng tr?ng th?a

## Notes

1. **Email không b?t bu?c**: H? th?ng dùng username thay vì email
2. **Role m?c ??nh**: T?t c? ??ng ký m?i ??u là Customer
3. **Admin/Employee**: Ph?i t?o th? công qua SQL ho?c admin panel
4. **Password policy**: Có th? custom trong RegisterViewModel
5. **Auto-activation**: Tài kho?n active ngay sau ??ng ký (TrangThai = true)
6. **Plain text password**: M?t kh?u l?u plain text, nh? chính xác ?? ??ng nh?p
