# ?? H??ng d?n S?a L?i "Invalid object name 'LichLamViec'"

## ? L?i g?p ph?i:
```
SqlException: Invalid object name 'LichLamViec'.
```

L?i này x?y ra vì b?ng `LichLamViec` ch?a t?n t?i trong database c?a b?n.

---

## ? Gi?i pháp (Ch?n 1 trong 3 cách):

### ?? **Cách 1: S? d?ng PowerShell Script (Khuyên dùng)**

1. M? PowerShell t?i th? m?c g?c project
2. Ch?y l?nh:
   ```powershell
   .\Database\RunAddLichLamViec.ps1
   ```
3. ??i script ch?y xong
4. F5 ?? ch?y l?i ?ng d?ng

---

### ??? **Cách 2: Ch?y SQL Script th? công**

1. M? **SQL Server Management Studio** (SSMS) ho?c **Azure Data Studio**
2. K?t n?i ??n server: `(localdb)\MSSQLLocalDB`
3. Ch?n database: `QuanLyBaiDoXe`
4. M? file: `Database\Add_LichLamViec_Table.sql`
5. Click **Execute** (F5)
6. Ki?m tra k?t qu? - Ph?i hi?n th?: "? ?ã t?o b?ng LichLamViec"

---

### ? **Cách 3: Ch?y SQL tr?c ti?p (Nhanh nh?t)**

Copy ?o?n SQL này và ch?y trong SSMS:

```sql
USE QuanLyBaiDoXe;
GO

CREATE TABLE LichLamViec (
    MaLich INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT,
    NgayLamViec DATE,
    CaLamViec INT,
    GhiChu NVARCHAR(255),
    
    CONSTRAINT FK_LichLamViec_NhanVien FOREIGN KEY (MaNhanVien) 
        REFERENCES NhanVien(MaNhanVien) ON DELETE CASCADE
);

CREATE INDEX IX_LichLamViec_MaNhanVien ON LichLamViec(MaNhanVien);
CREATE INDEX IX_LichLamViec_NgayLamViec ON LichLamViec(NgayLamViec);

PRINT N'? ?ã t?o b?ng LichLamViec thành công!';
```

---

## ?? Ki?m tra b?ng ?ã ???c t?o:

Ch?y query này ?? ki?m tra:

```sql
USE QuanLyBaiDoXe;
GO

-- Ki?m tra b?ng LichLamViec
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LichLamViec]') AND type in (N'U'))
    PRINT N'? B?ng LichLamViec ?ã t?n t?i'
ELSE
    PRINT N'? B?ng LichLamViec CH?A t?n t?i'
GO

-- Xem c?u trúc b?ng
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LichLamViec'
ORDER BY ORDINAL_POSITION;
```

---

## ?? C?u trúc b?ng LichLamViec:

| C?t | Ki?u | Mô t? |
|-----|------|-------|
| `MaLich` | INT (PK, Identity) | Mã l?ch làm vi?c |
| `MaNhanVien` | INT (FK) | Mã nhân viên |
| `NgayLamViec` | DATE | Ngày làm vi?c |
| `CaLamViec` | INT | Ca làm vi?c (1=Sáng, 2=Chi?u, 3=?êm) |
| `GhiChu` | NVARCHAR(255) | Ghi chú |

---

## ?? Thêm d? li?u m?u (Optional):

```sql
-- L?y ID nhân viên
DECLARE @MaNhanVien INT = (SELECT TOP 1 MaNhanVien FROM NhanVien WHERE TrangThaiLamViec = 1);

-- Thêm l?ch m?u
INSERT INTO LichLamViec (MaNhanVien, NgayLamViec, CaLamViec, GhiChu)
VALUES 
    (@MaNhanVien, CAST(GETDATE() AS DATE), 1, N'Ca sáng'),
    (@MaNhanVien, CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), 2, N'Ca chi?u'),
    (@MaNhanVien, CAST(DATEADD(DAY, 2, GETDATE()) AS DATE), 3, N'Ca ?êm');

SELECT * FROM LichLamViec;
```

---

## ?? L?u ý:

1. **Backup database** tr??c khi ch?y script
2. ??m b?o b?n có quy?n CREATE TABLE
3. Database `QuanLyBaiDoXe` ph?i t?n t?i
4. B?ng `NhanVien` ph?i t?n t?i tr??c (?? t?o Foreign Key)

---

## ?? N?u v?n g?p l?i:

1. **Ki?m tra connection string** trong `appsettings.json`:
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QuanLyBaiDoXe;..."
   ```

2. **Ki?m tra LocalDB ?ang ch?y**:
   ```cmd
   sqllocaldb info MSSQLLocalDB
   sqllocaldb start MSSQLLocalDB
   ```

3. **Xem log l?i chi ti?t** trong Output window c?a Visual Studio

4. **Rebuild solution**:
   ```
   Build ? Rebuild Solution
   ```

---

## ? Sau khi s?a l?i:

1. ? B?ng `LichLamViec` ?ã ???c t?o
2. ? F5 ?? ch?y l?i ?ng d?ng
3. ? Truy c?p `/Admin/VehicleShift/Schedule`
4. ? Xem giao di?n Timeline m?i

---

**Developed with ?? for QuanLyBaiDoXe**
