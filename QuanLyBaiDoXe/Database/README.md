# ?? Database Scripts

Th? m?c này ch?a các SQL scripts ?? setup và maintain database cho h? th?ng QuanLyBaiDoXe.

---

## ?? Danh sách files:

### 1. **Add_LichLamViec_Table.sql**
- **M?c ?ích**: T?o b?ng `LichLamViec` (L?ch làm vi?c c?a nhân viên)
- **Khi nào dùng**: Khi g?p l?i "Invalid object name 'LichLamViec'"
- **N?i dung**:
  - T?o b?ng LichLamViec v?i c?u trúc ??y ??
  - T?o Foreign Key ??n b?ng NhanVien
  - T?o indexes ?? t?i ?u performance
  - (Optional) Thêm d? li?u m?u

### 2. **RunAddLichLamViec.bat**
- **M?c ?ích**: Ch?y SQL script Add_LichLamViec_Table.sql t? ??ng
- **Cách dùng**: Double-click file ho?c ch?y t? cmd
- **Yêu c?u**: sqlcmd ph?i ???c cài ??t (có s?n trong SQL Server)

### 3. **RunAddLichLamViec.ps1**
- **M?c ?ích**: PowerShell version c?a batch file
- **Cách dùng**: 
  ```powershell
  .\Database\RunAddLichLamViec.ps1
  ```
- **?u ?i?m**: Có thêm error handling và ??c connection string t? appsettings.json

### 4. **FIX_LICHLLAMVIEC_ERROR.md**
- **M?c ?ích**: H??ng d?n chi ti?t cách s?a l?i LichLamViec
- **N?i dung**: 3 cách s?a l?i + troubleshooting + ki?m tra

---

## ?? Quick Start:

### Cách 1: Dùng Batch File (D? nh?t)
```cmd
cd Database
RunAddLichLamViec.bat
```

### Cách 2: Dùng PowerShell
```powershell
cd Database
.\RunAddLichLamViec.ps1
```

### Cách 3: Ch?y SQL th? công
1. M? SSMS ho?c Azure Data Studio
2. Connect ??n `(localdb)\MSSQLLocalDB`
3. M? file `Add_LichLamViec_Table.sql`
4. Execute (F5)

---

## ?? L?u ý quan tr?ng:

1. **Backup database tr??c khi ch?y script**
   ```sql
   BACKUP DATABASE QuanLyBaiDoXe 
   TO DISK = 'C:\Backup\QuanLyBaiDoXe.bak'
   ```

2. **Ki?m tra LocalDB ?ang ch?y**
   ```cmd
   sqllocaldb info MSSQLLocalDB
   sqllocaldb start MSSQLLocalDB
   ```

3. **Connection String ph?i ?úng** trong appsettings.json:
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QuanLyBaiDoXe;..."
   ```

---

## ?? C?u trúc b?ng LichLamViec:

```sql
CREATE TABLE LichLamViec (
    MaLich         INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien     INT,                    -- FK to NhanVien
    NgayLamViec    DATE,                   -- Ngày làm vi?c
    CaLamViec      INT,                    -- 1=Sáng, 2=Chi?u, 3=?êm
    GhiChu         NVARCHAR(255),          -- Ghi chú
    
    CONSTRAINT FK_LichLamViec_NhanVien 
        FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);
```

### Indexes:
- `IX_LichLamViec_MaNhanVien`: T?ng t?c query theo nhân viên
- `IX_LichLamViec_NgayLamViec`: T?ng t?c query theo ngày

---

## ?? Ki?m tra sau khi ch?y:

```sql
-- Ki?m tra b?ng t?n t?i
SELECT * FROM sys.tables WHERE name = 'LichLamViec';

-- Xem c?u trúc
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'LichLamViec';

-- Xem d? li?u
SELECT * FROM LichLamViec;
```

---

## ?? Troubleshooting:

### L?i: "Cannot find the object 'NhanVien'"
? B?ng NhanVien ch?a t?n t?i. Ch?y full database script tr??c.

### L?i: "sqlcmd is not recognized"
? Cài SQL Server Command Line Utilities ho?c ch?y SQL th? công trong SSMS.

### L?i: "Login failed for user 'sa'"
? Ki?m tra password trong connection string và SQL script ph?i gi?ng nhau.

### B?ng ?ã t?n t?i nh?ng v?n l?i
? Restart Visual Studio và Rebuild solution.

---

## ?? Tài li?u liên quan:

- [TIMELINE_GUIDE.md](../Areas/Admin/Views/VehicleShift/TIMELINE_GUIDE.md) - H??ng d?n giao di?n Timeline
- [FIX_LICHLLAMVIEC_ERROR.md](./FIX_LICHLLAMVIEC_ERROR.md) - Chi ti?t cách s?a l?i

---

**Last Updated**: 2024
**Version**: 1.0
