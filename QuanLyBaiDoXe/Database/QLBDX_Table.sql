USE master;
GO

-- 1. XỬ LÝ DATABASE (Xóa nếu tồn tại để làm mới hoàn toàn)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyBaiDoXe')
BEGIN
    ALTER DATABASE QuanLyBaiDoXe SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyBaiDoXe;
END
GO

CREATE DATABASE QuanLyBaiDoXe;
GO
USE QuanLyBaiDoXe;
GO

----------------------------------------------------------
-- PHẦN 1: QUẢN LÝ CON NGƯỜI (TÀI KHOẢN, NHÂN VIÊN, KHÁCH HÀNG)
----------------------------------------------------------

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL, -- Khuyến nghị Hash mật khẩu ở Backend
    QuyenHan NVARCHAR(50) NOT NULL DEFAULT N'Khách hàng' CHECK (QuyenHan IN (N'Admin', N'Khách hàng', N'Nhân viên')),
    Email VARCHAR(100) UNIQUE,
    TrangThai BIT DEFAULT 1
);

CREATE TABLE NhanVien (
    MaNhanVien INT PRIMARY KEY IDENTITY(1,1),
    MaTaiKhoan INT UNIQUE REFERENCES TaiKhoan(MaTaiKhoan), 
    HoTen NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10),
    NgaySinh DATE,
    CCCD VARCHAR(20) UNIQUE,
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(200),
    ChucVu INT DEFAULT 1, -- 0: Admin, 1: Quản lý, 2: Bảo vệ, 3: Kỹ thuật, 4: Nhân viên
    NgayVaoLam DATE DEFAULT GETDATE(),
    TrangThaiLamViec BIT DEFAULT 1
);

CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    MaTaiKhoan INT UNIQUE REFERENCES TaiKhoan(MaTaiKhoan) NULL, 
    SoDienThoai VARCHAR(15) UNIQUE NOT NULL, 
    HoTen NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(20),
    DiaChi NVARCHAR(200),
    BienSoXeMacDinh VARCHAR(20)
);

----------------------------------------------------------
-- PHẦN 2: QUẢN LÝ VẬN HÀNH CA & TÀI CHÍNH (ĐỐI SOÁT)
----------------------------------------------------------

CREATE TABLE CaLamViec (
    MaCa INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT REFERENCES NhanVien(MaNhanVien),
    ThoiGianNhanCa DATETIME DEFAULT GETDATE(),
    ThoiGianGiaoCa DATETIME,
    TienDauCa DECIMAL(18,0) DEFAULT 0 CHECK (TienDauCa >= 0),
    TongTienHeThong DECIMAL(18,0) DEFAULT 0, 
    TienMatBanGiao DECIMAL(18,0) DEFAULT 0 CHECK (TienMatBanGiao >= 0),
    GhiChuBanGiao NVARCHAR(255),
    TrangThaiCa INT DEFAULT 0 -- 0: Đang trực, 1: Đã chốt
);

----------------------------------------------------------
-- ⭐ PHẦN 2.1: LỊCH LÀM VIỆC & ĐĂNG KÝ LỊCH (MỚI)
----------------------------------------------------------

CREATE TABLE LichLamViec (
    MaLich INT IDENTITY PRIMARY KEY,
    MaNhanVien INT NOT NULL REFERENCES NhanVien(MaNhanVien),
    MaCa INT NULL REFERENCES CaLamViec(MaCa),
    NgayLamViec DATE NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    LoaiCa INT DEFAULT 0, -- 0: Thường | 1: Tăng ca | 2: Đêm
    TrangThai INT DEFAULT 1, -- 0: Nghỉ | 1: Làm
    GhiChu NVARCHAR(255),
    CONSTRAINT CHK_GioLam CHECK (GioKetThuc > GioBatDau)
);

CREATE TABLE DangKyLich (
    MaDangKy INT IDENTITY PRIMARY KEY,
    MaNhanVien INT NOT NULL REFERENCES NhanVien(MaNhanVien),
    MaLich INT REFERENCES LichLamViec(MaLich),

    LoaiYeuCau INT NOT NULL,
    -- 0: Xin nghỉ
    -- 1: Đổi ca
    -- 2: Đổi giờ

    NgayYeuCau DATETIME DEFAULT GETDATE(),

    NgayLamMoi DATE NULL,
    GioBatDauMoi TIME NULL,
    GioKetThucMoi TIME NULL,

    LyDo NVARCHAR(500),

    TrangThaiDuyet INT DEFAULT 0,
    -- 0: Chờ duyệt | 1: Duyệt | 2: Từ chối | 3: Hủy

    MaNhanVienDuyet INT REFERENCES NhanVien(MaNhanVien),
    ThoiGianDuyet DATETIME,
    GhiChuDuyet NVARCHAR(255)
);

----------------------------------------------------------
-- PHẦN 3: CẤU HÌNH XE & GIÁ (BLOCK LŨY TIẾN)
----------------------------------------------------------

CREATE TABLE LoaiXe (
    MaLoaiXe INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiXe NVARCHAR(50), 
    MoTa NVARCHAR(100),
    GiaThang DECIMAL(18,0) CHECK (GiaThang >= 0)
);

CREATE TABLE CauHinhGia (
    MaCauHinh INT IDENTITY(1,1) PRIMARY KEY,
    TenCauHinh NVARCHAR(100) NOT NULL,
    MaLoaiXe INT NOT NULL REFERENCES LoaiXe(MaLoaiXe),
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    IsUuTien BIT DEFAULT 0 -- 1: Ưu tiên (Lễ / Đêm / Đặc biệt)
);

CREATE TABLE ChiTietGia (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),
    MaCauHinh INT REFERENCES CauHinhGia(MaCauHinh),
    ThuTuBlock INT, 
    SoPhutCuaBlock INT, 
    GiaTien DECIMAL(18,0) CHECK (GiaTien >= 0), 
    IsLuyTien BIT DEFAULT 0 
);

----------------------------------------------------------
-- PHẦN 4: HẠ TẦNG BÃI XE & ĐẶT CHỖ (SMART PARKING)
----------------------------------------------------------

CREATE TABLE KhuVuc (
    MaKhuVuc INT PRIMARY KEY IDENTITY(1,1),
    TenKhuVuc NVARCHAR(50)
);

CREATE TABLE ViTriDo (
    MaViTri INT PRIMARY KEY IDENTITY(1,1),
    MaKhuVuc INT REFERENCES KhuVuc(MaKhuVuc),
    TenViTri VARCHAR(20), 
    TrangThai INT DEFAULT 0 -- 0: Trống, 1: Có xe, 2: Đã đặt, 3: Bảo trì
);

CREATE TABLE DatCho (
    MaDatCho INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT REFERENCES KhachHang(MaKhachHang),
    MaViTri INT REFERENCES ViTriDo(MaViTri),
    ThoiGianDat DATETIME DEFAULT GETDATE(),
    ThoiGianDenDuKien DATETIME,
    ThoiGianHetHan DATETIME,
    TrangThaiDatCho INT DEFAULT 0 -- 0: Pending, 1: Completed, 2: Cancelled, 3: Expired
);

----------------------------------------------------------
-- PHẦN 5: THẺ XE & VÉ THÁNG (LỊCH SỬ DÒNG TIỀN)
----------------------------------------------------------

CREATE TABLE TheXe (
    MaThe VARCHAR(50) PRIMARY KEY,
    MaLoaiXe INT REFERENCES LoaiXe(MaLoaiXe),
    LoaiThe INT DEFAULT 0, -- 0: Vãng lai, 1: Thẻ tháng
    TrangThai INT DEFAULT 1 
);

CREATE TABLE TheThang (
    MaTheThang INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT REFERENCES KhachHang(MaKhachHang),
    MaThe VARCHAR(50) REFERENCES TheXe(MaThe),
    NgayBatDau DATE DEFAULT GETDATE(),
    NgayHetHan DATE,
    SoTienDong DECIMAL(18,0) CHECK (SoTienDong >= 0),
    TrangThai BIT DEFAULT 1
);

CREATE TABLE LichSuGiaHanThe (
    MaGiaHan INT PRIMARY KEY IDENTITY(1,1),
    MaTheThang INT REFERENCES TheThang(MaTheThang),
    NgayGiaHan DATETIME DEFAULT GETDATE(),
    ThoiHanCu DATE,
    ThoiHanMoi DATE,
    SoTien DECIMAL(18,0) CHECK (SoTien >= 0),
    MaNhanVienThucHien INT REFERENCES NhanVien(MaNhanVien)
);

----------------------------------------------------------
-- PHẦN 6: VẬN HÀNH LƯỢT GỬI & SỰ CỐ
----------------------------------------------------------

CREATE TABLE LuotGui (
    MaLuotGui BIGINT PRIMARY KEY IDENTITY(1,1),
    MaThe VARCHAR(50) REFERENCES TheXe(MaThe),
    MaDatCho INT REFERENCES DatCho(MaDatCho) NULL,
    MaCaVao INT REFERENCES CaLamViec(MaCa),
    ThoiGianVao DATETIME NOT NULL,
    BienSoVao VARCHAR(20),
    HinhAnhVao VARCHAR(500) NULL,
    MaViTri INT REFERENCES ViTriDo(MaViTri),
    MaCaRa INT REFERENCES CaLamViec(MaCa),
    ThoiGianRa DATETIME,
    BienSoRa VARCHAR(20),
    HinhAnhRa VARCHAR(500) NULL,
    TongTien DECIMAL(18,0) DEFAULT 0 CHECK (TongTien >= 0),
    TrangThai INT DEFAULT 0, -- 0: Trong bãi, 1: Đã ra
    CONSTRAINT CHK_ThoiGian CHECK (ThoiGianRa >= ThoiGianVao OR ThoiGianRa IS NULL)
);

CREATE TABLE SuCo (
    MaSuCo INT PRIMARY KEY IDENTITY(1,1),
    ThoiGianGhiNhan DATETIME DEFAULT GETDATE(),
    MaNhanVien INT REFERENCES NhanVien(MaNhanVien), 
    LoaiSuCo NVARCHAR(50), 
    MaThe VARCHAR(50) NULL, 
    MaViTri INT NULL, 
    MoTaChiTiet NVARCHAR(500),
    TrangThaiXuLy INT DEFAULT 0 
);
GO

----------------------------------------------------------
-- PHẦN 7: LOGIC TỰ ĐỘNG (FUNCTIONS, TRIGGERS, PROCEDURES)
----------------------------------------------------------

-- 1. Function kiểm tra hiệu lực vé tháng
CREATE FUNCTION dbo.fn_KiemTraTheThangHieuLuc (@MaThe VARCHAR(50))
RETURNS BIT
AS
BEGIN
    DECLARE @HopLe BIT = 0;
    IF EXISTS (SELECT 1 FROM TheThang WHERE MaThe = @MaThe AND NgayHetHan >= CAST(GETDATE() AS DATE) AND TrangThai = 1)
        SET @HopLe = 1;
    RETURN @HopLe;
END;
GO

-- 2. Trigger tự động cập nhật vị trí sang "Có xe" khi Check-in
CREATE TRIGGER Trg_UpdateViTri_CheckIn
ON LuotGui AFTER INSERT AS
BEGIN
    UPDATE ViTriDo SET TrangThai = 1 FROM ViTriDo v JOIN inserted i ON v.MaViTri = i.MaViTri;
END;
GO

-- 3. Trigger tự động giải phóng vị trí khi Check-out
CREATE TRIGGER Trg_UpdateViTri_CheckOut
ON LuotGui AFTER UPDATE AS
BEGIN
    IF (SELECT TrangThai FROM inserted) = 1
    BEGIN
        UPDATE ViTriDo SET TrangThai = 0 FROM ViTriDo v JOIN inserted i ON v.MaViTri = i.MaViTri;
    END
END;
GO

-- 4. Procedure Khách đặt chỗ
CREATE PROCEDURE ThuTuc_KhachDatCho @MaKhachHang INT, @MaViTri INT, @ThoiGianDen DATETIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM ViTriDo WHERE MaViTri = @MaViTri AND TrangThai = 0)
    BEGIN
        INSERT INTO DatCho (MaKhachHang, MaViTri, ThoiGianDenDuKien, ThoiGianHetHan, TrangThaiDatCho)
        VALUES (@MaKhachHang, @MaViTri, @ThoiGianDen, DATEADD(MINUTE, 30, @ThoiGianDen), 0); 
        UPDATE ViTriDo SET TrangThai = 2 WHERE MaViTri = @MaViTri;
        SELECT 1 AS KetQua, N'Đặt thành công' AS ThongBao;
    END
    ELSE SELECT 0 AS KetQua, N'Vị trí không sẵn sàng' AS ThongBao;
END;
GO

-- 5. Procedure giải phóng chỗ đặt quá hạn
CREATE PROCEDURE ThuTuc_GiaiPhongDatChoQuaHan AS
BEGIN
    UPDATE DatCho SET TrangThaiDatCho = 3 WHERE TrangThaiDatCho = 0 AND ThoiGianHetHan < GETDATE();
    UPDATE ViTriDo SET TrangThai = 0 WHERE MaViTri IN (SELECT MaViTri FROM DatCho WHERE TrangThaiDatCho = 3) AND TrangThai = 2;
END;
GO

----------------------------------------------------------
-- PHẦN 8: SEED DATA (BẢN SỬA LỖI TRÙNG LẶP)
----------------------------------------------------------
-- Xóa sạch theo thứ tự để tránh lỗi khóa ngoại
DELETE FROM LuotGui; DELETE FROM SuCo; DELETE FROM DatCho; 
DELETE FROM TheThang; DELETE FROM LichSuGiaHanThe;
DELETE FROM NhanVien; DELETE FROM KhachHang; DELETE FROM TaiKhoan;
DELETE FROM ViTriDo; DELETE FROM KhuVuc; 
DELETE FROM ChiTietGia; DELETE FROM CauHinhGia; DELETE FROM LoaiXe;

-- Reset Identity
DBCC CHECKIDENT ('TaiKhoan', RESEED, 0);
DBCC CHECKIDENT ('NhanVien', RESEED, 0);
DBCC CHECKIDENT ('KhachHang', RESEED, 0);
GO

-- 1. TẠO TÀI KHOẢN & NHÂN VIÊN
-- A. Tạo Admin
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai) VALUES ('admin', 'admin123', 1);
DECLARE @IdTaiKhoanAdmin INT = SCOPE_IDENTITY(); -- Lấy ID vừa tạo

INSERT INTO NhanVien (MaTaiKhoan, HoTen, GioiTinh, CCCD, SoDienThoai, DiaChi, ChucVu)
VALUES (@IdTaiKhoanAdmin, N'Nguyễn Quản Trị', N'Nam', '079123456789', '0909111222', N'Q.1, TP.HCM', 0);

-- B. Tạo Bảo Vệ
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai) VALUES ('baove1', '123456', 1);
DECLARE @IdTaiKhoanBaoVe INT = SCOPE_IDENTITY();

INSERT INTO NhanVien (MaTaiKhoan, HoTen, GioiTinh, CCCD, SoDienThoai, DiaChi, ChucVu)
VALUES (@IdTaiKhoanBaoVe, N'Trần Bảo Vệ', N'Nam', '079987654321', '0908333444', N'Q.Bình Thạnh, TP.HCM', 1);


-- 2. TẠO TÀI KHOẢN & KHÁCH HÀNG
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai) VALUES ('khachhangA', 'pass123', 1);
DECLARE @IdTaiKhoanKhach INT = SCOPE_IDENTITY();

INSERT INTO KhachHang (MaTaiKhoan, HoTen, SoDienThoai, CCCD, BienSoXeMacDinh, DiaChi)
VALUES (@IdTaiKhoanKhach, N'Lê Thị Khách', '0912345678', '079111222333', '59A-999.99', N'Q.3, TP.HCM');

-- Lưu lại ID khách hàng vừa tạo để dùng cho việc mua vé tháng bên dưới
DECLARE @IdKhachHangA INT = SCOPE_IDENTITY(); 


-- 3. CẤU HÌNH XE & GIÁ
INSERT INTO LoaiXe (TenLoaiXe, MoTa) VALUES (N'Xe Máy', N'2 bánh'), (N'Ô tô 4 chỗ', N'Dưới 9 chỗ');

-- Cấu hình giá Ô tô ngày
INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, GioBatDau, GioKetThuc) 
VALUES (N'Giá Ô tô Ngày', 2, '06:00:00', '18:00:00');
DECLARE @MaCauHinhOto INT = SCOPE_IDENTITY();

-- Block 1: 2 tiếng đầu 20k
INSERT INTO ChiTietGia (MaCauHinh, ThuTuBlock, SoPhutCuaBlock, GiaTien, IsLuyTien) 
VALUES (@MaCauHinhOto, 1, 120, 20000, 0);
-- Block 2: Mỗi 1 tiếng sau 10k
INSERT INTO ChiTietGia (MaCauHinh, ThuTuBlock, SoPhutCuaBlock, GiaTien, IsLuyTien) 
VALUES (@MaCauHinhOto, 2, 60, 10000, 1);


-- 4. CẤU HÌNH BÃI XE (KHU VỰC & VỊ TRÍ)
INSERT INTO KhuVuc (TenKhuVuc) VALUES (N'Hầm B1');
DECLARE @MaKhuVuc INT = SCOPE_IDENTITY();

INSERT INTO ViTriDo (MaKhuVuc, TenViTri, TrangThai) VALUES 
(@MaKhuVuc, 'B1-01', 0), 
(@MaKhuVuc, 'B1-02', 0), 
(@MaKhuVuc, 'B1-03', 0);


-- 5. TẠO THẺ XE (RFID)
-- A. Thẻ Vãng Lai (LoaiThe = 0)
INSERT INTO TheXe (MaThe, MaLoaiXe, LoaiThe, TrangThai) VALUES 
('DAILY_XM_01', 1, 0, 1), -- Thẻ xe máy vãng lai
('DAILY_OTO_01', 2, 0, 1); -- Thẻ ô tô vãng lai

-- B. Thẻ Vé Tháng (LoaiThe = 1)
INSERT INTO TheXe (MaThe, MaLoaiXe, LoaiThe, TrangThai) VALUES 
('MONTH_OTO_01', 2, 1, 1); -- Thẻ tháng Ô tô


-- 6. ĐĂNG KÝ VÉ THÁNG (Liên kết Khách - Thẻ - Hạn dùng)
-- Đăng ký cho khách hàng 'Lê Thị Khách' (@IdKhachHangA) dùng thẻ 'MONTH_OTO_01'
INSERT INTO TheThang (MaKhachHang, MaThe, NgayBatDau, NgayHetHan, SoTienDong, TrangThai)
VALUES (
    @IdKhachHangA, 
    'MONTH_OTO_01', 
    GETDATE(), 
    DATEADD(MONTH, 1, GETDATE()), -- Hết hạn sau 1 tháng
    1500000,
    1
);


-- Script thêm cột HinhAnhVao và HinhAnhRa vào bảng LuotGui
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

-- Thêm cột HinhAnhVao
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LuotGui]') AND name = 'HinhAnhVao')
BEGIN
    ALTER TABLE [dbo].[LuotGui] ADD [HinhAnhVao] VARCHAR(500) NULL;
    PRINT N'Đã thêm cột HinhAnhVao';
END
ELSE
BEGIN
    PRINT N'Cột HinhAnhVao đã tồn tại';
END

-- Thêm cột HinhAnhRa
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LuotGui]') AND name = 'HinhAnhRa')
BEGIN
    ALTER TABLE [dbo].[LuotGui] ADD [HinhAnhRa] VARCHAR(500) NULL;
    PRINT N'Đã thêm cột HinhAnhRa';
END
ELSE
BEGIN
    PRINT N'Cột HinhAnhRa đã tồn tại';
END

PRINT N'Hoàn tất cập nhật bảng LuotGui';



PRINT N'Hoàn tất thiết lập Database 10/10!';