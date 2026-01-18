USE master;
GO

-- Ngắt tất cả kết nối đang chạy vào DB này để tránh lỗi "Database in use"
ALTER DATABASE QuanLyBaiDoXe SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Xóa bay Database
DROP DATABASE QuanLyBaiDoXe;
GO

-- Tạo lại mới tinh
CREATE DATABASE QuanLyBaiDoXe;
GO
USE QuanLyBaiDoXe;
----------------------------------------------------------
-- PHẦN 1: QUẢN LÝ CON NGƯỜI (NHÂN VIÊN & KHÁCH HÀNG)
----------------------------------------------------------
-- 1. Bảng Tài Khoản (Chỉ chứa thông tin đăng nhập)
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    TrangThai BIT DEFAULT 1 -- 1: Active
);


-- 1. Bảng Nhân viên (Đã bổ sung đầy đủ thông tin)
CREATE TABLE NhanVien (
    MaNhanVien INT PRIMARY KEY IDENTITY(1,1),
    
    -- Thông tin đăng nhập
    MaTaiKhoan INT UNIQUE REFERENCES TaiKhoan(MaTaiKhoan), 
    
    -- Thông tin cá nhân chi tiết
    HoTen NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10), -- Nam/Nữ
    NgaySinh DATE,
    CCCD VARCHAR(20) UNIQUE, -- Căn cước công dân
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(200),
    
    -- Thông tin công việc
    ChucVu INT DEFAULT 1, -- 0: Quản lý, 1: Bảo vệ, 2: Kỹ thuật, 3: Kế toán
    NgayVaoLam DATE DEFAULT GETDATE(),
    TrangThaiLamViec BIT DEFAULT 1 -- 1: Đang làm, 0: Đã nghỉ
);

-- 2. Bảng Khách Hàng (Mới - Có tài khoản để đặt chỗ)
-- 2. Bảng Khách Hàng (Chứa thông tin định danh)
CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    
    -- LINK QUAN TRỌNG: Cho phép NULL
    -- Nếu NULL: Khách offline (chỉ dùng vé tháng, không có App)
    -- Nếu Có giá trị: Khách online (có thể dùng App đặt chỗ)
    MaTaiKhoan INT UNIQUE REFERENCES TaiKhoan(MaTaiKhoan) NULL, 
    
    -- Định danh duy nhất thực tế là Số điện thoại
    SoDienThoai VARCHAR(15) UNIQUE NOT NULL, 
    HoTen NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(20),
    DiaChi NVARCHAR(200),
    BienSoXeMacDinh VARCHAR(20)
);

----------------------------------------------------------
-- PHẦN 2: QUẢN LÝ LỊCH TRÌNH & CA KÍP
----------------------------------------------------------

-- 3. Bảng Lịch Làm Việc (Kế hoạch)
CREATE TABLE LichLamViec (
    MaLich INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT REFERENCES NhanVien(MaNhanVien),
    NgayLamViec DATE,
    CaLamViec INT, -- 1: Sáng (6-14h), 2: Chiều (14-22h), 3: Đêm (22-6h)
    GhiChu NVARCHAR(200)
);

-- 4. Bảng Ca Làm Việc Thực Tế (Chấm công & Doanh thu)
CREATE TABLE CaLamViec (
    MaCa INT PRIMARY KEY IDENTITY(1,1),
    MaNhanVien INT REFERENCES NhanVien(MaNhanVien),
    ThoiGianNhanCa DATETIME DEFAULT GETDATE(),
    ThoiGianGiaoCa DATETIME,
    
    -- Tiền nong
    TienDauCa DECIMAL(18,0) DEFAULT 0,
    TongTienThu DECIMAL(18,0) DEFAULT 0, -- Hệ thống tính

    TrangThaiCa INT DEFAULT 0 -- 0: Đang trực, 1: Đã chốt ca
);

----------------------------------------------------------
-- PHẦN 3: CẤU HÌNH GIÁ & TÀI SẢN
----------------------------------------------------------

-- 5. Bảng Loại Xe
CREATE TABLE LoaiXe (
    MaLoaiXe INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiXe NVARCHAR(50), -- Xe máy, Ô tô 4 chỗ...
    MoTa NVARCHAR(100)
);

-- 6. Bảng Cấu Hình Giá (Header)
CREATE TABLE CauHinhGia (
    MaCauHinh INT PRIMARY KEY IDENTITY(1,1),
    TenCauHinh NVARCHAR(100), 
    MaLoaiXe INT REFERENCES LoaiXe(MaLoaiXe),
    GioBatDau TIME, 
    GioKetThuc TIME, 
    IsUuTien BIT DEFAULT 0 
);

-- 7. Bảng Chi Tiết Giá (Các block tính tiền)
CREATE TABLE ChiTietGia (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),
    MaCauHinh INT REFERENCES CauHinhGia(MaCauHinh),
    ThuTuBlock INT, 
    SoPhutCuaBlock INT, 
    GiaTien DECIMAL(18,0), 
    IsLuyTien BIT DEFAULT 0 
);

----------------------------------------------------------
-- PHẦN 4: QUẢN LÝ BÃI XE & ĐẶT CHỖ (SMART PARKING)
----------------------------------------------------------

-- 8. Bảng Khu vực
CREATE TABLE KhuVuc (
    MaKhuVuc INT PRIMARY KEY IDENTITY(1,1),
    TenKhuVuc NVARCHAR(50) -- Tầng hầm B1, Khu A
);

-- 9. Bảng Vị Trí Đỗ
CREATE TABLE ViTriDo (
    MaViTri INT PRIMARY KEY IDENTITY(1,1),
    MaKhuVuc INT REFERENCES KhuVuc(MaKhuVuc),
    TenViTri VARCHAR(20), -- A-01, A-02
    
    -- 0: Trống (Available)
    -- 1: Có xe (Occupied)
    -- 2: Đã đặt trước (Reserved - Cho khách hàng đặt qua App)
    -- 3: Sự cố/Bảo trì (Broken)
    TrangThai INT DEFAULT 0 
);

-- 10. Bảng Đặt Chỗ (Booking - MỚI)
CREATE TABLE DatCho (
    MaDatCho INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT REFERENCES KhachHang(MaKhachHang),
    MaViTri INT REFERENCES ViTriDo(MaViTri),
    
    ThoiGianDat DATETIME DEFAULT GETDATE(), -- Lúc bấm nút đặt
    ThoiGianDenDuKien DATETIME, -- Khách hứa mấy giờ đến
    ThoiGianHetHan DATETIME, -- Nếu quá giờ này không đến thì hủy
    
    -- 0: Đang giữ chỗ (Pending)
    -- 1: Đã đến (Completed - Check in thành công)
    -- 2: Đã hủy (Cancelled)
    -- 3: Quá hạn (Expired)
    TrangThaiDatCho INT DEFAULT 0
);

----------------------------------------------------------
-- PHẦN 5: VẬN HÀNH & SỰ CỐ
----------------------------------------------------------

-- 11. Bảng Thẻ Xe
CREATE TABLE TheXe (
    MaThe VARCHAR(50) PRIMARY KEY, -- Mã chip RFID
    MaLoaiXe INT REFERENCES LoaiXe(MaLoaiXe),
    
    -- 0: Thẻ vãng lai (Lấy vào rồi trả ra ngay)
    -- 1: Thẻ tháng (Giao cho khách cầm về nhà)
    LoaiThe INT DEFAULT 0, 
    
    TrangThai INT DEFAULT 1 -- 1: Active, 0: Lost/Hỏng
);

-- 11.1 Bảng Vé Tháng (MỚI - BẮT BUỘC PHẢI CÓ)
-- Bảng này liên kết Khách hàng - Thẻ xe - Thời hạn
CREATE TABLE VeThang (
    MaVeThang INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT REFERENCES KhachHang(MaKhachHang),
    MaThe VARCHAR(50) REFERENCES TheXe(MaThe),
    
    NgayBatDau DATE DEFAULT GETDATE(),
    NgayHetHan DATE, -- Quan trọng: Để kiểm tra xem vé còn hạn không
    SoTienDong DECIMAL(18,0),
    
    TrangThai BIT DEFAULT 1 -- 1: Đang dùng, 0: Đã hủy/Hết hạn
);

-- 12. Bảng Lượt Gửi Xe (Core)
CREATE TABLE LuotGui (
    MaLuotGui BIGINT PRIMARY KEY IDENTITY(1,1),
    MaThe VARCHAR(50) REFERENCES TheXe(MaThe),
    MaDatCho INT REFERENCES DatCho(MaDatCho), -- Link tới Booking nếu có
    
    -- Thông tin vào
    MaCaVao INT REFERENCES CaLamViec(MaCa),
    ThoiGianVao DATETIME,
    BienSoVao VARCHAR(20),
    MaViTri INT REFERENCES ViTriDo(MaViTri),
    
    -- Thông tin ra
    MaCaRa INT REFERENCES CaLamViec(MaCa),
    ThoiGianRa DATETIME,
    BienSoRa VARCHAR(20),
    
    TongTien DECIMAL(18,0) DEFAULT 0,
    TrangThai INT DEFAULT 0 -- 0: In, 1: Out
);

-- 13. Bảng Sự Cố
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
-- PHẦN 6: STORED PROCEDURES (LOGIC ĐẶT CHỖ)
----------------------------------------------------------

-- Procedure: Khách Hàng Đặt Chỗ (Booking)
-- Logic: Kiểm tra chỗ trống -> Tạo đơn đặt -> Đổi màu vị trí thành Vàng (2)
CREATE PROCEDURE ThuTuc_KhachDatCho
    @MaKhachHang INT,
    @MaViTri INT,
    @ThoiGianDen DATETIME
AS
BEGIN
    -- Kiểm tra vị trí có trống không (TrangThai = 0)
    IF EXISTS (SELECT 1 FROM ViTriDo WHERE MaViTri = @MaViTri AND TrangThai = 0)
    BEGIN
        -- 1. Tạo Booking
        INSERT INTO DatCho (MaKhachHang, MaViTri, ThoiGianDenDuKien, ThoiGianHetHan, TrangThaiDatCho)
        VALUES (@MaKhachHang, @MaViTri, @ThoiGianDen, DATEADD(MINUTE, 30, @ThoiGianDen), 0); 
        -- Giữ chỗ trong vòng 30 phút so với giờ hẹn

        -- 2. Cập nhật Vị trí thành "Đã đặt trước" (Trạng thái 2)
        UPDATE ViTriDo SET TrangThai = 2 WHERE MaViTri = @MaViTri;

        SELECT 1 AS KetQua, N'Đặt chỗ thành công!' AS ThongBao;
    END
    ELSE
    BEGIN
        SELECT 0 AS KetQua, N'Vị trí này đã có người hoặc đang bảo trì!' AS ThongBao;
    END
END
GO

----------------------------------------------------------
-- PHẦN 7: SEED DATA (DỮ LIỆU MẪU) - ĐÃ SỬA LỖI LOGIC
----------------------------------------------------------

-- Xóa dữ liệu cũ nếu có để tránh lỗi trùng lặp khi chạy lại
DELETE FROM VeThang;
DELETE FROM LuotGui;
DELETE FROM TheXe;
DELETE FROM DatCho;
DELETE FROM ViTriDo;
DELETE FROM KhuVuc;
DELETE FROM ChiTietGia;
DELETE FROM CauHinhGia;
DELETE FROM LoaiXe;
DELETE FROM KhachHang;
DELETE FROM NhanVien;
DELETE FROM TaiKhoan;
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
INSERT INTO VeThang (MaKhachHang, MaThe, NgayBatDau, NgayHetHan, SoTienDong, TrangThai)
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
