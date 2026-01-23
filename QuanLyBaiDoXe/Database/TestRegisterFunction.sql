-- =============================================
-- Script ki?m tra và test ch?c n?ng ??ng ký
-- =============================================

USE QuanLyBaiDoXe;
GO

-- =============================================
-- 1. Ki?m tra các ràng bu?c c?a b?ng
-- =============================================
PRINT N'=== KI?M TRA CONSTRAINTS ===';
PRINT N'';

-- Ki?m tra unique constraint cho TenDangNhap
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    type_desc AS ConstraintType
FROM sys.objects
WHERE type_desc LIKE '%CONSTRAINT%'
    AND OBJECT_NAME(parent_object_id) IN ('TaiKhoan', 'KhachHang')
ORDER BY TableName, name;

PRINT N'';
PRINT N'=== KI?M TRA INDEXES ===';
PRINT N'';

-- Ki?m tra index unique
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.is_unique AS IsUnique,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('TaiKhoan', 'KhachHang')
    AND i.is_unique = 1
ORDER BY t.name, i.name;

-- =============================================
-- 2. Test t?o tài kho?n h?p l?
-- =============================================
PRINT N'';
PRINT N'=== TEST 1: T?O TÀI KHO?N H?P L? ===';
PRINT N'';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Xóa test data c? n?u có
    DELETE FROM KhachHang WHERE SoDienThoai = '0987654321';
    DELETE FROM TaiKhoan WHERE TenDangNhap = 'testuser1';
    
    -- T?o tài kho?n m?i
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('testuser1', 'hashed_password_here', 1);
    
    DECLARE @TaiKhoanId INT = SCOPE_IDENTITY();
    
    -- T?o khách hàng
    INSERT INTO KhachHang (MaTaiKhoan, SoDienThoai, HoTen, CCCD, DiaChi, BienSoXeMacDinh)
    VALUES (@TaiKhoanId, '0987654321', N'Nguy?n Test User', '001234567899', N'Hà N?i', '30A-12345');
    
    COMMIT TRANSACTION;
    PRINT N'? Test 1 PASSED: T?o tài kho?n thành công';
    
    -- Hi?n th? thông tin v?a t?o
    SELECT 
        tk.MaTaiKhoan,
        tk.TenDangNhap,
        tk.TrangThai,
        kh.MaKhachHang,
        kh.HoTen,
        kh.SoDienThoai,
        kh.CCCD,
        kh.DiaChi,
        kh.BienSoXeMacDinh
    FROM TaiKhoan tk
    INNER JOIN KhachHang kh ON tk.MaTaiKhoan = kh.MaTaiKhoan
    WHERE tk.TenDangNhap = 'testuser1';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'? Test 1 FAILED: ' + ERROR_MESSAGE();
END CATCH
GO

-- =============================================
-- 3. Test trùng tên ??ng nh?p
-- =============================================
PRINT N'';
PRINT N'=== TEST 2: TRÙNG TÊN ??NG NH?P ===';
PRINT N'';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Th? t?o tài kho?n v?i username ?ã t?n t?i
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('testuser1', 'another_password', 1);
    
    COMMIT TRANSACTION;
    PRINT N'? Test 2 FAILED: Không phát hi?n trùng username';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    IF ERROR_NUMBER() = 2601 OR ERROR_NUMBER() = 2627
        PRINT N'? Test 2 PASSED: Phát hi?n trùng username';
    ELSE
        PRINT N'? Test 2 FAILED: ' + ERROR_MESSAGE();
END CATCH
GO

-- =============================================
-- 4. Test trùng s? ?i?n tho?i
-- =============================================
PRINT N'';
PRINT N'=== TEST 3: TRÙNG S? ?I?N THO?I ===';
PRINT N'';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- T?o tài kho?n m?i
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('testuser2', 'password', 1);
    
    DECLARE @TaiKhoanId2 INT = SCOPE_IDENTITY();
    
    -- Th? t?o khách hàng v?i s? ?i?n tho?i ?ã t?n t?i
    INSERT INTO KhachHang (MaTaiKhoan, SoDienThoai, HoTen)
    VALUES (@TaiKhoanId2, '0987654321', N'Test User 2');
    
    COMMIT TRANSACTION;
    PRINT N'? Test 3 FAILED: Không phát hi?n trùng s? ?i?n tho?i';
    
    -- Cleanup
    DELETE FROM KhachHang WHERE MaTaiKhoan = @TaiKhoanId2;
    DELETE FROM TaiKhoan WHERE MaTaiKhoan = @TaiKhoanId2;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    IF ERROR_NUMBER() = 2601 OR ERROR_NUMBER() = 2627
        PRINT N'? Test 3 PASSED: Phát hi?n trùng s? ?i?n tho?i';
    ELSE
        PRINT N'? Test 3 FAILED: ' + ERROR_MESSAGE();
END CATCH
GO

-- =============================================
-- 5. Test trùng CCCD
-- =============================================
PRINT N'';
PRINT N'=== TEST 4: TRÙNG CCCD ===';
PRINT N'';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- T?o tài kho?n m?i
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('testuser3', 'password', 1);
    
    DECLARE @TaiKhoanId3 INT = SCOPE_IDENTITY();
    
    -- Th? t?o khách hàng v?i CCCD ?ã t?n t?i
    INSERT INTO KhachHang (MaTaiKhoan, SoDienThoai, HoTen, CCCD)
    VALUES (@TaiKhoanId3, '0988888888', N'Test User 3', '001234567899');
    
    COMMIT TRANSACTION;
    PRINT N'? Test 4 FAILED: Không phát hi?n trùng CCCD';
    
    -- Cleanup
    DELETE FROM KhachHang WHERE MaTaiKhoan = @TaiKhoanId3;
    DELETE FROM TaiKhoan WHERE MaTaiKhoan = @TaiKhoanId3;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    IF ERROR_NUMBER() = 2601 OR ERROR_NUMBER() = 2627
        PRINT N'? Test 4 PASSED: Phát hi?n trùng CCCD';
    ELSE
        PRINT N'? Test 4 WARNING: CCCD có th? ch?a có unique constraint';
END CATCH
GO

-- =============================================
-- 6. Test t?o tài kho?n không có thông tin b? sung
-- =============================================
PRINT N'';
PRINT N'=== TEST 5: T?O TÀI KHO?N T?I THI?U ===';
PRINT N'';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Xóa test data c? n?u có
    DELETE FROM KhachHang WHERE SoDienThoai = '0999999999';
    DELETE FROM TaiKhoan WHERE TenDangNhap = 'minimaluser';
    
    -- T?o tài kho?n v?i thông tin t?i thi?u
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('minimaluser', 'hashed_password', 1);
    
    DECLARE @TaiKhoanId5 INT = SCOPE_IDENTITY();
    
    -- T?o khách hàng ch? v?i các field b?t bu?c
    INSERT INTO KhachHang (MaTaiKhoan, SoDienThoai, HoTen)
    VALUES (@TaiKhoanId5, '0999999999', N'Minimal User');
    
    COMMIT TRANSACTION;
    PRINT N'? Test 5 PASSED: T?o tài kho?n v?i thông tin t?i thi?u thành công';
    
    -- Hi?n th?
    SELECT 
        tk.TenDangNhap,
        kh.HoTen,
        kh.SoDienThoai,
        kh.CCCD,
        kh.DiaChi,
        kh.BienSoXeMacDinh
    FROM TaiKhoan tk
    INNER JOIN KhachHang kh ON tk.MaTaiKhoan = kh.MaTaiKhoan
    WHERE tk.TenDangNhap = 'minimaluser';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'? Test 5 FAILED: ' + ERROR_MESSAGE();
END CATCH
GO

-- =============================================
-- 7. Hi?n th? t?ng k?t
-- =============================================
PRINT N'';
PRINT N'=== T?NG K?T ===';
PRINT N'';

-- ??m s? l??ng tài kho?n
SELECT 
    COUNT(*) AS TotalAccounts,
    SUM(CASE WHEN TrangThai = 1 THEN 1 ELSE 0 END) AS ActiveAccounts,
    SUM(CASE WHEN TrangThai = 0 THEN 1 ELSE 0 END) AS InactiveAccounts
FROM TaiKhoan;

-- ??m s? l??ng khách hàng
SELECT 
    COUNT(*) AS TotalCustomers,
    COUNT(CCCD) AS CustomersWithCCCD,
    COUNT(DiaChi) AS CustomersWithAddress,
    COUNT(BienSoXeMacDinh) AS CustomersWithLicensePlate
FROM KhachHang;

-- Hi?n th? danh sách test accounts
PRINT N'';
PRINT N'=== DANH SÁCH TÀI KHO?N TEST ===';
SELECT 
    tk.TenDangNhap,
    kh.HoTen,
    kh.SoDienThoai,
    kh.CCCD
FROM TaiKhoan tk
INNER JOIN KhachHang kh ON tk.MaTaiKhoan = kh.MaTaiKhoan
WHERE tk.TenDangNhap IN ('testuser1', 'minimaluser')
ORDER BY tk.TenDangNhap;

-- =============================================
-- 8. Cleanup (Optional)
-- =============================================
PRINT N'';
PRINT N'=== CLEANUP (COMMENTED) ===';
PRINT N'-- Uncomment ?? xóa test data';
PRINT N'';

/*
-- Xóa test accounts
DELETE FROM KhachHang WHERE SoDienThoai IN ('0987654321', '0999999999');
DELETE FROM TaiKhoan WHERE TenDangNhap IN ('testuser1', 'minimaluser');

PRINT N'? ?ã xóa test data';
*/

-- =============================================
-- 9. Queries h?u ích
-- =============================================
PRINT N'';
PRINT N'=== QUERIES H?U ÍCH ===';
PRINT N'';
PRINT N'-- Ki?m tra username ?ã t?n t?i:';
PRINT N'SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap = ''username''';
PRINT N'';
PRINT N'-- Ki?m tra s? ?i?n tho?i ?ã t?n t?i:';
PRINT N'SELECT COUNT(*) FROM KhachHang WHERE SoDienThoai = ''0912345678''';
PRINT N'';
PRINT N'-- Tìm khách hàng theo tài kho?n:';
PRINT N'SELECT * FROM KhachHang WHERE MaTaiKhoan = {id}';
PRINT N'';
PRINT N'-- Xem thông tin ??y ??:';
PRINT N'SELECT tk.*, kh.* FROM TaiKhoan tk LEFT JOIN KhachHang kh ON tk.MaTaiKhoan = kh.MaTaiKhoan';
