USE QuanLyBaiDoXe;
GO

DELETE FROM LuotGui;
DELETE FROM SuCo;
DELETE FROM DatCho;
DELETE FROM VeThang;
DELETE FROM LichSuGiaHanVe;
DELETE FROM CaLamViec;

DELETE FROM NhanVien;
DELETE FROM KhachHang;
DELETE FROM TaiKhoan;

DELETE FROM ViTriDo;
DELETE FROM KhuVuc;

DELETE FROM ChiTietGia;
DELETE FROM CauHinhGia;
DELETE FROM LoaiXe;
GO

DBCC CHECKIDENT ('TaiKhoan', RESEED, 0);
DBCC CHECKIDENT ('NhanVien', RESEED, 0);
DBCC CHECKIDENT ('KhachHang', RESEED, 0);
DBCC CHECKIDENT ('LoaiXe', RESEED, 0);
DBCC CHECKIDENT ('KhuVuc', RESEED, 0);
GO


----------------------------------------------------------
-- 1. TÀI KHOẢN
----------------------------------------------------------
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, TrangThai)
VALUES 
('admin', 'admin123', 1),
('baove1', '123456', 1),
('khachhangA', 'pass123', 1);
GO

----------------------------------------------------------
-- 2. NHÂN VIÊN
----------------------------------------------------------
INSERT INTO NhanVien (MaTaiKhoan, HoTen, GioiTinh, CCCD, SoDienThoai, DiaChi, ChucVu)
VALUES
(1, N'Nguyễn Quản Trị', N'Nam', '079123456789', '0909111222', N'Q.1, TP.HCM', 0),
(2, N'Trần Bảo Vệ',     N'Nam', '079987654321', '0908333444', N'Q.Bình Thạnh, TP.HCM', 1);
GO

----------------------------------------------------------
-- 3. KHÁCH HÀNG
----------------------------------------------------------
INSERT INTO KhachHang (MaTaiKhoan, HoTen, SoDienThoai, CCCD, BienSoXeMacDinh, DiaChi)
VALUES
(3, N'Lê Thị Khách', '0912345678', '079111222333', '59A-999.99', N'Q.3, TP.HCM');
GO

----------------------------------------------------------
-- 4. LOẠI XE
----------------------------------------------------------
INSERT INTO LoaiXe (TenLoaiXe, MoTa)
VALUES
(N'Xe Máy', N'2 bánh'),
(N'Ô tô 4 chỗ', N'Dưới 9 chỗ');
GO

----------------------------------------------------------
-- 5. CẤU HÌNH GIÁ
----------------------------------------------------------
INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, GioBatDau, GioKetThuc, LoaiGia)
VALUES
(N'Giá Ô tô Ngày', 2, '06:00:00', '18:00:00', 0);
GO

INSERT INTO ChiTietGia (MaCauHinh, ThuTuBlock, SoPhutCuaBlock, GiaTien, IsLuyTien)
VALUES
(1, 1, 120, 20000, 0),  -- 2 giờ đầu
(1, 2, 60, 10000, 1);   -- mỗi giờ sau
GO

----------------------------------------------------------
-- 6. KHU VỰC & VỊ TRÍ ĐỖ
----------------------------------------------------------
INSERT INTO KhuVuc (TenKhuVuc)
VALUES (N'Hầm B1');
GO

INSERT INTO ViTriDo (MaKhuVuc, TenViTri, TrangThai)
VALUES
(1, 'B1-01', 0),
(1, 'B1-02', 0),
(1, 'B1-03', 0);
GO

----------------------------------------------------------
-- 7. THẺ XE
----------------------------------------------------------
INSERT INTO TheXe (MaThe, MaLoaiXe, LoaiThe, TrangThai)
VALUES
('DAILY_XM_01',  1, 0, 1),
('DAILY_OTO_01', 2, 0, 1),
('MONTH_OTO_01', 2, 1, 1);
GO

----------------------------------------------------------
-- 8. VÉ THÁNG
----------------------------------------------------------
INSERT INTO VeThang (MaKhachHang, MaThe, NgayBatDau, NgayHetHan, SoTienDong, TrangThai)
VALUES
(1, 'MONTH_OTO_01', GETDATE(), DATEADD(MONTH, 1, GETDATE()), 1500000, 1);
GO

----------------------------------------------------------
-- 9. CA LÀM VIỆC (MẪU)
----------------------------------------------------------
INSERT INTO CaLamViec (MaNhanVien, TienDauCa, TrangThaiCa)
VALUES
(2, 500000, 0);
GO

PRINT N'✔ Đã insert seed data cho QuanLyBaiDoXe';
