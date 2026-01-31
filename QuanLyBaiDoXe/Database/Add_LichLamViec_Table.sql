-- Script thêm b?ng LichLamViec vào database
-- Ch?y script này n?u b?ng LichLamViec ch?a t?n t?i

USE QuanLyBaiDoXe;
GO

-- Ki?m tra và t?o b?ng LichLamViec n?u ch?a t?n t?i
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LichLamViec]') AND type in (N'U'))
BEGIN
    CREATE TABLE LichLamViec (
        MaLich INT PRIMARY KEY IDENTITY(1,1),
        MaNhanVien INT,
        NgayLamViec DATE,
        CaLamViec INT, -- 1: Ca sáng (6h-14h), 2: Ca chi?u (14h-22h), 3: Ca ?êm (22h-6h)
        GhiChu NVARCHAR(255),
        
        CONSTRAINT FK_LichLamViec_NhanVien FOREIGN KEY (MaNhanVien) 
            REFERENCES NhanVien(MaNhanVien) ON DELETE CASCADE
    );
    
    PRINT N'? ?ã t?o b?ng LichLamViec';
END
ELSE
BEGIN
    PRINT N'! B?ng LichLamViec ?ã t?n t?i';
END
GO

-- T?o index ?? t?ng t?c query
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LichLamViec_MaNhanVien' AND object_id = OBJECT_ID('LichLamViec'))
BEGIN
    CREATE INDEX IX_LichLamViec_MaNhanVien ON LichLamViec(MaNhanVien);
    PRINT N'? ?ã t?o index IX_LichLamViec_MaNhanVien';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LichLamViec_NgayLamViec' AND object_id = OBJECT_ID('LichLamViec'))
BEGIN
    CREATE INDEX IX_LichLamViec_NgayLamViec ON LichLamViec(NgayLamViec);
    PRINT N'? ?ã t?o index IX_LichLamViec_NgayLamViec';
END
GO

-- Thêm d? li?u m?u (optional - comment out n?u không c?n)
/*
-- L?y ID c?a nhân viên ?? thêm l?ch m?u
DECLARE @MaNhanVien1 INT = (SELECT TOP 1 MaNhanVien FROM NhanVien WHERE TrangThaiLamViec = 1 ORDER BY MaNhanVien);
DECLARE @MaNhanVien2 INT = (SELECT TOP 1 MaNhanVien FROM NhanVien WHERE TrangThaiLamViec = 1 AND MaNhanVien != @MaNhanVien1 ORDER BY MaNhanVien);

IF @MaNhanVien1 IS NOT NULL
BEGIN
    -- Thêm l?ch tu?n này cho nhân viên 1
    INSERT INTO LichLamViec (MaNhanVien, NgayLamViec, CaLamViec, GhiChu)
    VALUES 
        (@MaNhanVien1, CAST(GETDATE() AS DATE), 1, N'Ca sáng'),
        (@MaNhanVien1, CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), 1, N'Ca sáng'),
        (@MaNhanVien1, CAST(DATEADD(DAY, 2, GETDATE()) AS DATE), 2, N'Ca chi?u');
    
    PRINT N'? ?ã thêm l?ch m?u cho nhân viên ' + CAST(@MaNhanVien1 AS NVARCHAR(10));
END

IF @MaNhanVien2 IS NOT NULL
BEGIN
    -- Thêm l?ch tu?n này cho nhân viên 2
    INSERT INTO LichLamViec (MaNhanVien, NgayLamViec, CaLamViec, GhiChu)
    VALUES 
        (@MaNhanVien2, CAST(GETDATE() AS DATE), 2, N'Ca chi?u'),
        (@MaNhanVien2, CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), 3, N'Ca ?êm'),
        (@MaNhanVien2, CAST(DATEADD(DAY, 3, GETDATE()) AS DATE), 1, N'Ca sáng');
    
    PRINT N'? ?ã thêm l?ch m?u cho nhân viên ' + CAST(@MaNhanVien2 AS NVARCHAR(10));
END
*/

PRINT N'';
PRINT N'=================================';
PRINT N'Hoàn t?t! B?ng LichLamViec ?ã s?n sàng.';
PRINT N'=================================';
GO
