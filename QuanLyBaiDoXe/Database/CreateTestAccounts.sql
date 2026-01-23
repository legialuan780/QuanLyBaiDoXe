-- =============================================
-- Script t?o tài kho?n test cho h? th?ng
-- L?U Ý: M?t kh?u l?u d?ng PLAIN TEXT (không mã hóa)
-- =============================================

USE QuanLyBaiDoXe;
GO

PRINT N'==============================================';
PRINT N'  T?O TÀI KHO?N TEST - PLAIN TEXT PASSWORD';
PRINT N'==============================================';
PRINT N'';

-- =============================================
-- 1. T?o tài kho?n Admin
-- =============================================
DECLARE @AdminTaiKhoanId INT;

-- Ki?m tra tài kho?n admin ?ã t?n t?i ch?a
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'admin')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('admin', 'admin123', 1);  -- Plain text password
    
    SET @AdminTaiKhoanId = SCOPE_IDENTITY();
    
    -- T?o nhân viên admin
    IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaTaiKhoan = @AdminTaiKhoanId)
    BEGIN
        INSERT INTO NhanVien (
            MaTaiKhoan, 
            HoTen, 
            GioiTinh, 
            NgaySinh, 
            CCCD, 
            SoDienThoai, 
            DiaChi, 
            ChucVu, 
            NgayVaoLam, 
            TrangThaiLamViec
        )
        VALUES (
            @AdminTaiKhoanId, 
            N'Qu?n Tr? Viên', 
            N'Nam', 
            '1990-01-01', 
            '001234567890', 
            '0901234567', 
            N'Hà N?i', 
            0,  -- 0 = Admin
            GETDATE(), 
            1
        );
        
        PRINT N'? ?ã t?o tài kho?n Admin';
        PRINT N'  Username: admin';
        PRINT N'  Password: admin123';
        PRINT N'  Role: Admin';
    END
END
ELSE
BEGIN
    PRINT N'! Tài kho?n admin ?ã t?n t?i';
    -- Hi?n th? thông tin
    SELECT 'admin' AS Username, MatKhau AS Password 
    FROM TaiKhoan WHERE TenDangNhap = 'admin';
END
GO

-- =============================================
-- 2. T?o tài kho?n Nhân viên
-- =============================================
DECLARE @NhanVienTaiKhoanId INT;

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'nhanvien')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('nhanvien', 'nv123', 1);  -- Plain text password
    
    SET @NhanVienTaiKhoanId = SCOPE_IDENTITY();
    
    -- T?o nhân viên
    IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaTaiKhoan = @NhanVienTaiKhoanId)
    BEGIN
        INSERT INTO NhanVien (
            MaTaiKhoan, 
            HoTen, 
            GioiTinh, 
            NgaySinh, 
            CCCD, 
            SoDienThoai, 
            DiaChi, 
            ChucVu, 
            NgayVaoLam, 
            TrangThaiLamViec
        )
        VALUES (
            @NhanVienTaiKhoanId, 
            N'Nguy?n V?n A', 
            N'Nam', 
            '1995-05-15', 
            '001234567891', 
            '0901234568', 
            N'H? Chí Minh', 
            1,  -- 1 = Nhân viên
            GETDATE(), 
            1
        );
        
        PRINT N'? ?ã t?o tài kho?n Nhân viên';
        PRINT N'  Username: nhanvien';
        PRINT N'  Password: nv123';
        PRINT N'  Role: Employee';
    END
END
ELSE
BEGIN
    PRINT N'! Tài kho?n nhanvien ?ã t?n t?i';
    SELECT 'nhanvien' AS Username, MatKhau AS Password 
    FROM TaiKhoan WHERE TenDangNhap = 'nhanvien';
END
GO

-- =============================================
-- 3. T?o tài kho?n Nhân viên 2
-- =============================================
DECLARE @NhanVien2TaiKhoanId INT;

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'nhanvien2')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('nhanvien2', 'nv123', 1);  -- Plain text password
    
    SET @NhanVien2TaiKhoanId = SCOPE_IDENTITY();
    
    -- T?o nhân viên
    IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaTaiKhoan = @NhanVien2TaiKhoanId)
    BEGIN
        INSERT INTO NhanVien (
            MaTaiKhoan, 
            HoTen, 
            GioiTinh, 
            NgaySinh, 
            CCCD, 
            SoDienThoai, 
            DiaChi, 
            ChucVu, 
            NgayVaoLam, 
            TrangThaiLamViec
        )
        VALUES (
            @NhanVien2TaiKhoanId, 
            N'Tr?n Th? B', 
            N'N?', 
            '1998-08-20', 
            '001234567892', 
            '0901234569', 
            N'?à N?ng', 
            1,  -- 1 = Nhân viên
            GETDATE(), 
            1
        );
        
        PRINT N'? ?ã t?o tài kho?n Nhân viên 2';
        PRINT N'  Username: nhanvien2';
        PRINT N'  Password: nv123';
        PRINT N'  Role: Employee';
    END
END
ELSE
BEGIN
    PRINT N'! Tài kho?n nhanvien2 ?ã t?n t?i';
    SELECT 'nhanvien2' AS Username, MatKhau AS Password 
    FROM TaiKhoan WHERE TenDangNhap = 'nhanvien2';
END
GO

-- =============================================
-- 4. T?o tài kho?n Khách hàng
-- =============================================
DECLARE @KhachHangTaiKhoanId INT;

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'khachhang')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('khachhang', 'kh123', 1);  -- Plain text password
    
    SET @KhachHangTaiKhoanId = SCOPE_IDENTITY();
    
    -- T?o khách hàng
    IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE MaTaiKhoan = @KhachHangTaiKhoanId)
    BEGIN
        INSERT INTO KhachHang (
            MaTaiKhoan, 
            SoDienThoai, 
            HoTen, 
            CCCD, 
            DiaChi, 
            BienSoXeMacDinh
        )
        VALUES (
            @KhachHangTaiKhoanId, 
            '0912345678', 
            N'Lê V?n C', 
            '001234567893', 
            N'Hà N?i', 
            '29A-12345'
        );
        
        PRINT N'? ?ã t?o tài kho?n Khách hàng';
        PRINT N'  Username: khachhang';
        PRINT N'  Password: kh123';
        PRINT N'  Role: Customer';
    END
END
ELSE
BEGIN
    PRINT N'! Tài kho?n khachhang ?ã t?n t?i';
    SELECT 'khachhang' AS Username, MatKhau AS Password 
    FROM TaiKhoan WHERE TenDangNhap = 'khachhang';
END
GO

-- =============================================
-- 5. T?o thêm m?t s? khách hàng khác
-- =============================================
DECLARE @KhachHang2TaiKhoanId INT;

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'khachhang2')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
    VALUES ('khachhang2', 'kh123', 1);  -- Plain text password
    
    SET @KhachHang2TaiKhoanId = SCOPE_IDENTITY();
    
    -- T?o khách hàng
    IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE MaTaiKhoan = @KhachHang2TaiKhoanId)
    BEGIN
        INSERT INTO KhachHang (
            MaTaiKhoan, 
            SoDienThoai, 
            HoTen, 
            CCCD, 
            DiaChi, 
            BienSoXeMacDinh
        )
        VALUES (
            @KhachHang2TaiKhoanId, 
            '0912345679', 
            N'Ph?m Th? D', 
            '001234567894', 
            N'C?n Th?', 
            '30B-67890'
        );
        
        PRINT N'? ?ã t?o tài kho?n Khách hàng 2';
        PRINT N'  Username: khachhang2';
        PRINT N'  Password: kh123';
        PRINT N'  Role: Customer';
    END
END
ELSE
BEGIN
    PRINT N'! Tài kho?n khachhang2 ?ã t?n t?i';
    SELECT 'khachhang2' AS Username, MatKhau AS Password 
    FROM TaiKhoan WHERE TenDangNhap = 'khachhang2';
END
GO

-- =============================================
-- 6. Hi?n th? danh sách t?t c? tài kho?n
-- =============================================
PRINT N'';
PRINT N'==============================================';
PRINT N'       DANH SÁCH TÀI KHO?N ?Ã T?O';
PRINT N'==============================================';
PRINT N'';

SELECT 
    tk.MaTaiKhoan,
    tk.TenDangNhap AS [Username],
    tk.MatKhau AS [Password (Plain Text)],
    CASE 
        WHEN nv.MaNhanVien IS NOT NULL THEN 
            CASE nv.ChucVu 
                WHEN 0 THEN N'Admin'
                ELSE N'Employee'
            END
        WHEN kh.MaKhachHang IS NOT NULL THEN N'Customer'
        ELSE N'Không xác ??nh'
    END AS [Role],
    COALESCE(nv.HoTen, kh.HoTen, N'') AS [H? Tên],
    CASE 
        WHEN tk.TrangThai = 1 THEN N'? Ho?t ??ng'
        ELSE N'? Khóa'
    END AS [Tr?ng Thái]
FROM TaiKhoan tk
LEFT JOIN NhanVien nv ON tk.MaTaiKhoan = nv.MaTaiKhoan
LEFT JOIN KhachHang kh ON tk.MaTaiKhoan = kh.MaTaiKhoan
ORDER BY 
    CASE 
        WHEN nv.ChucVu = 0 THEN 1
        WHEN nv.MaNhanVien IS NOT NULL THEN 2
        ELSE 3
    END,
    tk.TenDangNhap;

PRINT N'';
PRINT N'==============================================';
PRINT N'           H??NG D?N ??NG NH?P';
PRINT N'==============================================';
PRINT N'';
PRINT N'L?U Ý: M?t kh?u là PLAIN TEXT - nh?p chính xác nh? trong b?ng';
PRINT N'';
PRINT N'1. ADMIN:';
PRINT N'   URL: https://localhost:port/Account/Login';
PRINT N'   Username: admin';
PRINT N'   Password: admin123';
PRINT N'   ? Redirect: /Admin/Dashboard';
PRINT N'';
PRINT N'2. NHÂN VIÊN:';
PRINT N'   Username: nhanvien (ho?c nhanvien2)';
PRINT N'   Password: nv123';
PRINT N'   ? Redirect: /Admin/Dashboard';
PRINT N'';
PRINT N'3. KHÁCH HÀNG:';
PRINT N'   Username: khachhang (ho?c khachhang2)';
PRINT N'   Password: kh123';
PRINT N'   ? Redirect: /Home/Index';
PRINT N'';
PRINT N'==============================================';
PRINT N'';

-- =============================================
-- 7. Query ?? ki?m tra m?t kh?u
-- =============================================
PRINT N'Query ?? ki?m tra m?t kh?u:';
PRINT N'SELECT TenDangNhap, MatKhau FROM TaiKhoan WHERE TenDangNhap = ''username''';
PRINT N'';
PRINT N'Query ?? reset m?t kh?u:';
PRINT N'UPDATE TaiKhoan SET MatKhau = ''newpassword'' WHERE TenDangNhap = ''username''';
PRINT N'';
