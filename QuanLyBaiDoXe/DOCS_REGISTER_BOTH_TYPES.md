# H??ng d?n ??ng ký tài kho?n Khách hàng và Nhân viên

## T?ng quan
H? th?ng cho phép ??ng ký **2 lo?i tài kho?n**:
1. **Khách hàng (Customer)**: S? d?ng d?ch v? g?i xe
2. **Nhân viên (Employee)**: Qu?n lý bãi ?? xe (Admin ho?c Nhân viên)

## Tính n?ng m?i

### 1. Account Type Selector
- Giao di?n ch?n lo?i tài kho?n tr?c quan v?i 2 nút:
  - ?? **Khách hàng**: S? d?ng d?ch v? g?i xe
  - ?? **Nhân viên**: Qu?n lý bãi ?? xe
- Form t? ??ng thay ??i theo lo?i tài kho?n ???c ch?n

### 2. Conditional Fields
Form hi?n th? các tr??ng khác nhau tùy lo?i tài kho?n:

#### Tr??ng chung (b?t bu?c)
- ? Tên ??ng nh?p
- ? M?t kh?u
- ? Xác nh?n m?t kh?u
- ? H? và tên
- ? S? ?i?n tho?i
- ? ??a ch? (không b?t bu?c)

#### Tr??ng cho Khách hàng
- ? CCCD/CMND (không b?t bu?c)
- ? Bi?n s? xe m?c ??nh (không b?t bu?c)

#### Tr??ng cho Nhân viên
- ? CCCD/CMND (B?T BU?C)
- ? Gi?i tính
- ? Ngày sinh
- ? Ch?c v? (Nhân viên / Qu?n lý)
- ? Ngày vào làm

### 3. Real-time Validation
- Username: Ki?m tra t?n t?i (?/?)
- Phone: Ki?m tra trong c? 2 b?ng KhachHang và NhanVien
- CCCD: Ki?m tra trong c? 2 b?ng KhachHang và NhanVien

## Flow ??ng ký

### ??ng ký Khách hàng
```
1. Ch?n lo?i: ?? Khách hàng
2. ?i?n thông tin b?t bu?c:
   - Username, Password, FullName, Phone
3. ?i?n thông tin b? sung (optional):
   - CCCD, Address, License Plate
4. Submit
5. T?o b?n ghi:
   - TaiKhoan (v?i MatKhau plain text)
   - KhachHang
6. Redirect v? Login v?i thông báo thành công
```

### ??ng ký Nhân viên
```
1. Ch?n lo?i: ?? Nhân viên
2. ?i?n thông tin b?t bu?c:
   - Username, Password, FullName, Phone
   - CCCD (b?t bu?c cho nhân viên)
3. ?i?n thông tin b? sung:
   - Gender, DateOfBirth
   - Position (Nhân viên = 1, Qu?n lý = 0)
   - StartDate (m?c ??nh hôm nay)
   - Address
4. Submit
5. T?o b?n ghi:
   - TaiKhoan (v?i MatKhau plain text)
   - NhanVien (v?i ChucVu, TrangThaiLamViec = true)
6. Redirect v? Login v?i thông báo thành công
```

## Database Schema

### B?ng TaiKhoan
```sql
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,  -- Plain text
    TrangThai BIT DEFAULT 1
);
```

### B?ng KhachHang
```sql
CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY,
    MaTaiKhoan INT UNIQUE,  -- FK to TaiKhoan
    SoDienThoai VARCHAR(15) UNIQUE NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(20) UNIQUE,
    DiaChi NVARCHAR(200),
    BienSoXeMacDinh VARCHAR(20)
);
```

### B?ng NhanVien
```sql
CREATE TABLE NhanVien (
    MaNhanVien INT PRIMARY KEY IDENTITY,
    MaTaiKhoan INT UNIQUE,  -- FK to TaiKhoan
    HoTen NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10),
    NgaySinh DATE,
    CCCD VARCHAR(20) UNIQUE NOT NULL,
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(200),
    ChucVu INT DEFAULT 1,  -- 0 = Admin, 1 = Employee
    NgayVaoLam DATE DEFAULT GETDATE(),
    TrangThaiLamViec BIT DEFAULT 1
);
```

## Ví d? s? d?ng

### Ví d? 1: ??ng ký Khách hàng
```
Lo?i tài kho?n: Khách hàng
Username: khachhang_test
Password: 123456
Confirm Password: 123456
H? tên: Nguy?n V?n Test
S? ?i?n tho?i: 0987654321
CCCD: (?? tr?ng)
??a ch?: Hà N?i
Bi?n s? xe: 30A-12345

? T?o:
  - TaiKhoan: username="khachhang_test", password="123456"
  - KhachHang: HoTen="Nguy?n V?n Test", SoDienThoai="0987654321"

? ??ng nh?p v?i role: Customer
```

### Ví d? 2: ??ng ký Nhân viên
```
Lo?i tài kho?n: Nhân viên
Username: nhanvien_test
Password: 123456
Confirm Password: 123456
H? tên: Tr?n Th? Test
S? ?i?n tho?i: 0123456789
CCCD: 001234567890 (B?T BU?C)
??a ch?: TP.HCM
Gi?i tính: N?
Ngày sinh: 1995-05-15
Ch?c v?: Nhân viên
Ngày vào làm: (?? tr?ng = hôm nay)

? T?o:
  - TaiKhoan: username="nhanvien_test", password="123456"
  - NhanVien: HoTen="Tr?n Th? Test", CCCD="001234567890", ChucVu=1

? ??ng nh?p v?i role: Employee
```

### Ví d? 3: ??ng ký Qu?n lý
```
Lo?i tài kho?n: Nhân viên
Username: admin_test
Password: admin123
Confirm Password: admin123
H? tên: Lê V?n Admin
S? ?i?n tho?i: 0999888777
CCCD: 001234567899 (B?T BU?C)
Ch?c v?: Qu?n lý  ? QUAN TR?NG

? T?o:
  - TaiKhoan: username="admin_test", password="admin123"
  - NhanVien: HoTen="Lê V?n Admin", ChucVu=0

? ??ng nh?p v?i role: Admin
```

## Phân quy?n sau khi ??ng ký

### Customer (Khách hàng)
```
Role: Customer
Quy?n:
- Truy c?p trang ch?
- ??t ch?
- Xem l?ch s? g?i xe
- Qu?n lý thông tin cá nhân

KHÔNG th?:
- Truy c?p /Admin/*
```

### Employee (Nhân viên - ChucVu = 1)
```
Role: Employee
Quy?n:
- Truy c?p /Admin/Dashboard
- Qu?n lý xe vào/ra
- Xem báo cáo
- X? lý s? c?

KHÔNG th?:
- Qu?n lý nhân viên
- C?u hình h? th?ng
```

### Admin (Qu?n lý - ChucVu = 0)
```
Role: Admin
Quy?n:
- T?T C? quy?n c?a Employee
- Qu?n lý nhân viên
- Qu?n lý khách hàng
- C?u hình giá
- Qu?n lý khu v?c
- Báo cáo toàn h? th?ng
```

## Validation Rules

### Username
- Required
- Length: 3-50 ký t?
- Unique trong b?ng TaiKhoan
- Real-time check

### Password
- Required
- Min length: 6 ký t?
- Must match ConfirmPassword

### Phone
- Required
- Format: 0[3|5|7|8|9]XXXXXXXX
- Unique trong c? KhachHang và NhanVien
- Real-time check

### CCCD
- **Customer**: Optional
- **Employee**: Required
- Unique trong c? KhachHang và NhanVien
- Real-time check

### Position (Ch?c v?)
- 0 = Admin (Qu?n lý)
- 1 = Employee (Nhân viên) - m?c ??nh

## API Endpoints

### Check Username
```
GET /Account/CheckUsername?username={username}
Response: { "available": true/false }
```

### Check Phone
```
GET /Account/CheckPhoneNumber?phoneNumber={phone}
Response: { "available": true/false }
```

### Check CCCD
```
GET /Account/CheckCCCD?cccd={cccd}
Response: { "available": true/false }
```

## Testing

### Test 1: ??ng ký Khách hàng thành công
```
Input:
- AccountType: Customer
- Username: test_customer
- Password: 123456
- Phone: 0912345678
- FullName: Test Customer

Expected:
? T?o TaiKhoan và KhachHang
? Redirect v? Login
? Có th? ??ng nh?p v?i role Customer
```

### Test 2: ??ng ký Nhân viên thành công
```
Input:
- AccountType: Employee
- Username: test_employee
- Password: 123456
- Phone: 0987654321
- FullName: Test Employee
- CCCD: 001234567890

Expected:
? T?o TaiKhoan và NhanVien
? Redirect v? Login
? Có th? ??ng nh?p v?i role Employee
? Truy c?p ???c /Admin/Dashboard
```

### Test 3: ??ng ký Admin thành công
```
Input:
- AccountType: Employee
- Position: Qu?n lý (0)
- CCCD: 001234567891

Expected:
? NhanVien.ChucVu = 0
? ??ng nh?p v?i role Admin
? Full quy?n truy c?p
```

### Test 4: Nhân viên không có CCCD
```
Input:
- AccountType: Employee
- CCCD: (empty)

Expected:
? Hi?n th? l?i: "CCCD/CMND là b?t bu?c ??i v?i nhân viên!"
```

### Test 5: Trùng s? ?i?n tho?i gi?a Customer và Employee
```
Input:
- Phone: 0912345678 (?ã dùng b?i Customer)
- AccountType: Employee

Expected:
? Hi?n th? l?i: "S? ?i?n tho?i ?ã ???c ??ng ký!"
```

## Security Notes

1. **Plain Text Password**: 
   - M?t kh?u l?u plain text
   - Ch? phù h?p development/test
   - Production c?n hash

2. **Role Assignment**:
   - Customer: T? ??ng t? ??ng ký
   - Employee: T? ch?n ch?c v? khi ??ng ký
   - Admin: Ch?n "Qu?n lý" khi ??ng ký

3. **Data Integrity**:
   - Phone unique across both tables
   - CCCD unique across both tables
   - MaTaiKhoan unique (1-1 relationship)

## Troubleshooting

### ??ng ký nhân viên nh?ng không vào ???c Admin
**Nguyên nhân**: TrangThaiLamViec = false ho?c ChucVu không ?úng

**Gi?i pháp**:
```sql
-- Ki?m tra
SELECT nv.*, tk.TenDangNhap 
FROM NhanVien nv
JOIN TaiKhoan tk ON nv.MaTaiKhoan = tk.MaTaiKhoan
WHERE tk.TenDangNhap = 'username';

-- Fix n?u c?n
UPDATE NhanVien 
SET TrangThaiLamViec = 1, ChucVu = 0 
WHERE MaTaiKhoan = {id};
```

### Real-time check không ho?t ??ng
- Ki?m tra jQuery loaded
- Check browser console
- Verify API endpoints

### Form không submit
- Check validation errors
- Verify all required fields filled
- Check CCCD for Employee

## SQL Scripts

### Ki?m tra tài kho?n v?a t?o
```sql
-- Ki?m tra Khách hàng
SELECT tk.TenDangNhap, tk.MatKhau, kh.*
FROM TaiKhoan tk
LEFT JOIN KhachHang kh ON tk.MaTaiKhoan = kh.MaTaiKhoan
WHERE tk.TenDangNhap = 'username';

-- Ki?m tra Nhân viên
SELECT tk.TenDangNhap, tk.MatKhau, nv.*
FROM TaiKhoan tk
LEFT JOIN NhanVien nv ON tk.MaTaiKhoan = nv.MaTaiKhoan
WHERE tk.TenDangNhap = 'username';
```

### Chuy?n Nhân viên thành Admin
```sql
UPDATE NhanVien
SET ChucVu = 0
WHERE MaTaiKhoan = (
    SELECT MaTaiKhoan 
    FROM TaiKhoan 
    WHERE TenDangNhap = 'username'
);
```

### Chuy?n khách thành nhân viên (Migrate)
```sql
-- L?U Ý: Ph?c t?p, c?n xem xét k?
-- 1. L?y thông tin khách hàng
-- 2. T?o b?n ghi NhanVien
-- 3. Xóa b?n ghi KhachHang (ho?c gi? l?i)
```

## Best Practices

1. **??ng ký Khách hàng**: Thông tin t?i thi?u
2. **??ng ký Nhân viên**: Ph?i có CCCD
3. **Admin**: Ch?n rõ ràng "Qu?n lý" trong Ch?c v?
4. **Testing**: Dùng s? ?i?n tho?i khác nhau
5. **Production**: Implement password hashing
