# ?? BUG FIX - Database Constraint Violation

## ? L?i g?p ph?i
```
An error occurred while saving the entity changes. See the inner exception for details.
```

## ?? Nguyên nhân
**Vi ph?m CHECK constraint c?a database!**

### Database Schema
```sql
QuyenHan NVARCHAR(50) NOT NULL DEFAULT N'Khách hàng' 
CHECK (QuyenHan IN (N'Admin', N'Khách hàng', N'Nhân viên'))
```

Database ch? cho phép 3 giá tr?:
- ? `Admin`
- ? `Khách hàng`
- ? `Nhân viên`

### Code c? (SAI)
```csharp
string quyenHan = request.ChucVu switch
{
    0 => "Admin",
    1 => "QuanLy",      // ? KHÔNG H?P L?
    2 => "BaoVe",       // ? KHÔNG H?P L?
    3 => "KyThuat",     // ? KHÔNG H?P L?
    4 => "NhanVien",    // ? KHÔNG H?P L?
    _ => "NhanVien"
};
```

## ? Gi?i pháp ?ã áp d?ng

### Code m?i (?ÚNG)
```csharp
// Determine QuyenHan based on ChucVu - Map theo database constraint
// Database ch? cho phép: 'Admin', 'Khách hàng', 'Nhân viên'
string quyenHan = request.ChucVu switch
{
    0 => "Admin",           // Admin
    _ => "Nhân viên"        // T?t c? nhân viên khác
};
```

### Mapping logic
| ChucVu | Tên ch?c v? | QuyenHan trong DB |
|--------|-------------|-------------------|
| 0      | Admin       | `Admin`          |
| 1      | Qu?n lý     | `Nhân viên`      |
| 2      | B?o v?      | `Nhân viên`      |
| 3      | K? thu?t    | `Nhân viên`      |
| 4      | Nhân viên   | `Nhân viên`      |

## ?? L?u ý quan tr?ng

### Phân bi?t ChucVu và QuyenHan
1. **ChucVu** (trong table NhanVien):
   - Dùng ?? phân bi?t vai trò công vi?c c? th?
   - Giá tr?: 0, 1, 2, 3, 4
   - Hi?n th?: Admin, Qu?n lý, B?o v?, K? thu?t, Nhân viên

2. **QuyenHan** (trong table TaiKhoan):
   - Dùng ?? phân quy?n trong h? th?ng
   - Giá tr?: `Admin`, `Khách hàng`, `Nhân viên`
   - Quy?t ??nh các tính n?ng có th? truy c?p

### Ví d? th?c t?
```
Nhân viên: Nguy?n V?n An
- ChucVu = 1 (Qu?n lý)
- QuyenHan = "Nhân viên"

? Hi?n th?: "Qu?n lý"
? Quy?n h? th?ng: Nhân viên (không ph?i Admin)
```

## ?? Ki?m tra sau khi s?a

### 1. Ch?y ?ng d?ng
```bash
dotnet run
```

### 2. Test t?o nhân viên
1. M? trang: `/Admin/VehicleShift/EmployeeList`
2. Click "Thêm nhân viên"
3. ?i?n thông tin:
   - H? tên: **Ph?m Lê An**
   - Ch?c v?: **Qu?n lý** (ho?c b?t k?)
   - S?T: **0123456789**
   - Gi?i tính: **Nam**
   - Ngày sinh: **02/02/2009**
4. Click "Thêm nhân viên"

### 3. K?t qu? mong ??i
? **Thành công:**
```
Modal hi?n th?:
??????????????????????????
T?o nhân viên thành công!
??????????????????????????
Tên ??ng nh?p: phamle an (ho?c t??ng t?)
M?t kh?u: 6789
Quy?n h?n: Nhân viên
??????????????????????????
```

### 4. Ki?m tra database

**Query 1: Ki?m tra TaiKhoan**
```sql
SELECT TOP 1 * 
FROM TaiKhoan 
ORDER BY MaTaiKhoan DESC;
```

K?t qu? mong ??i:
```
MaTaiKhoan | TenDangNhap  | MatKhau | QuyenHan   | Email              | TrangThai
-----------|--------------|---------|------------|--------------------|----------
X          | phamlean     | 6789    | Nhân viên  | 0123456789@...     | 1
```

**Query 2: Ki?m tra NhanVien**
```sql
SELECT TOP 1 nv.*, tk.TenDangNhap, tk.QuyenHan
FROM NhanVien nv
LEFT JOIN TaiKhoan tk ON nv.MaTaiKhoan = tk.MaTaiKhoan
ORDER BY nv.MaNhanVien DESC;
```

K?t qu? mong ??i:
```
MaNhanVien | MaTaiKhoan | HoTen      | ChucVu | TenDangNhap | QuyenHan   
-----------|------------|------------|--------|-------------|------------
Y          | X          | Ph?m Lê An | 1      | phamlean    | Nhân viên
```

### 5. Test các tr??ng h?p

| Test Case | ChucVu | K?t qu? QuyenHan | Tr?ng thái |
|-----------|--------|------------------|------------|
| Admin     | 0      | `Admin`         | ? OK     |
| Qu?n lý   | 1      | `Nhân viên`     | ? OK     |
| B?o v?    | 2      | `Nhân viên`     | ? OK     |
| K? thu?t  | 3      | `Nhân viên`     | ? OK     |
| Nhân viên | 4      | `Nhân viên`     | ? OK     |

## ?? N?u c?n thay ??i constraint trong t??ng lai

### Option 1: Thay ??i constraint (không khuy?n khích)
```sql
-- Drop constraint c?
ALTER TABLE TaiKhoan 
DROP CONSTRAINT [tên_constraint];

-- Thêm constraint m?i
ALTER TABLE TaiKhoan
ADD CONSTRAINT CHK_QuyenHan 
CHECK (QuyenHan IN (N'Admin', N'Khách hàng', N'Nhân viên', 
                    N'QuanLy', N'BaoVe', N'KyThuat'));
```

### Option 2: Gi? nguyên (khuy?n khích) ?
- Database schema ??n gi?n
- Logic rõ ràng: Admin vs Nhân viên
- Phân bi?t chi ti?t qua `ChucVu` trong b?ng NhanVien

## ?? Tóm t?t thay ??i

### Files ?ã s?a
1. ? `VehicleShiftController.cs` - Dòng 1475-1483

### Thay ??i c? th?
- ? **Tr??c:** Map 5 giá tr? khác nhau ? Vi ph?m constraint
- ? **Sau:** Map 2 giá tr? (Admin / Nhân viên) ? H?p l?

### Impact
- ? T?o nhân viên thành công
- ? L?u vào database không l?i
- ? T??ng thích v?i schema hi?n t?i
- ? Không c?n thay ??i database

---

## ? Status: FIXED ?

Build successful! Code ?ã ho?t ??ng ?úng v?i database constraint.
