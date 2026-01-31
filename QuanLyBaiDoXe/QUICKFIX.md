# ? QUICK FIX - L?i LichLamViec

## L?i g?p ph?i:
```
SqlException: Invalid object name 'LichLamViec'.
```

## Gi?i pháp nhanh (Ch?n 1):

### ? CÁCH 1: Batch File (Khuyên dùng - 5 giây)
1. Double-click file: `Database\RunAddLichLamViec.bat`
2. ??i "Hoàn t?t!"
3. F5 ch?y l?i app

### ? CÁCH 2: SQL th? công (1 phút)
1. M? SSMS
2. Connect: `(localdb)\MSSQLLocalDB`
3. Database: `QuanLyBaiDoXe`
4. M? file: `Database\Add_LichLamViec_Table.sql`
5. F5 Execute
6. F5 ch?y l?i app

### ? CÁCH 3: Copy-Paste (30 giây)
M? SSMS và ch?y:
```sql
USE QuanLyBaiDoXe;
CREATE TABLE LichLamViec (
    MaLich INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT,
    NgayLamViec DATE,
    CaLamViec INT,
    GhiChu NVARCHAR(255),
    CONSTRAINT FK_LichLamViec_NhanVien 
        FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);
CREATE INDEX IX_LichLamViec_MaNhanVien ON LichLamViec(MaNhanVien);
CREATE INDEX IX_LichLamViec_NgayLamViec ON LichLamViec(NgayLamViec);
```

## Done! ??
Ch?y l?i app và truy c?p `/Admin/VehicleShift/Schedule`

---
?? Chi ti?t: [FIX_LICHLLAMVIEC_ERROR.md](./FIX_LICHLLAMVIEC_ERROR.md)
