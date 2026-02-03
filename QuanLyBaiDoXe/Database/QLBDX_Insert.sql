USE QuanLyBaiDoXe;
GO

----------------------------------------------------------
-- 0. DỌN DẸP DỮ LIỆU CŨ
----------------------------------------------------------
DELETE FROM DangKyLich;
DELETE FROM LichLamViec;
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
DELETE FROM TheXe;
DELETE FROM ChiTietGia;
DELETE FROM CauHinhGia;
DELETE FROM LoaiXe;
GO

DBCC CHECKIDENT ('TaiKhoan', RESEED, 0);
DBCC CHECKIDENT ('NhanVien', RESEED, 0);
DBCC CHECKIDENT ('KhachHang', RESEED, 0);
DBCC CHECKIDENT ('LoaiXe', RESEED, 0);
DBCC CHECKIDENT ('KhuVuc', RESEED, 0);
DBCC CHECKIDENT ('LichLamViec', RESEED, 0);
DBCC CHECKIDENT ('DangKyLich', RESEED, 0);
GO


----------------------------------------------------------
-- 1. TÀI KHOẢN (20)
----------------------------------------------------------
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, QuyenHan, TrangThai) VALUES
('admin',      'admin@123',  N'Admin',     1), -- MaTK: 1
('quanly01',   'ql01@123',   N'Nhân viên', 1), -- MaTK: 2
('quanly02',   'ql02@123',   N'Nhân viên', 1), -- MaTK: 3
('kythuat01',  'kt01@123',   N'Nhân viên', 1), -- MaTK: 4
('kythuat02',  'kt02@123',   N'Nhân viên', 1), -- MaTK: 5
('nhanvien01', 'nv01@123',   N'Nhân viên', 1), -- MaTK: 6
('nhanvien02', 'nv02@123',   N'Nhân viên', 1), -- MaTK: 7
('nhanvien03', 'nv03@123',   N'Nhân viên', 1), -- MaTK: 8
('nhanvien04', 'nv04@123',   N'Nhân viên', 1), -- MaTK: 9
('nhanvien05', 'nv05@123',   N'Nhân viên', 1), -- MaTK: 10
('khach01',    'kh01@123',   N'Khách hàng', 1), -- MaTK: 11
('khach02',    'kh02@123',   N'Khách hàng', 1), -- MaTK: 12
('khach03',    'kh03@123',   N'Khách hàng', 1), -- MaTK: 13
('khach04',    'kh04@123',   N'Khách hàng', 1), -- MaTK: 14
('khach05',    'kh05@123',   N'Khách hàng', 1), -- MaTK: 15
('guest01',    'guest01@123', N'Khách hàng', 0), -- MaTK: 16
('guest02',    'guest02@123', N'Khách hàng', 0), -- MaTK: 17
('guest03',    'guest03@123', N'Khách hàng', 0), -- MaTK: 18
('guest04',    'guest04@123', N'Khách hàng', 0), -- MaTK: 19
('guest05',    'guest05@123', N'Khách hàng', 0); -- MaTK: 20
GO

----------------------------------------------------------
-- 2. NHÂN VIÊN (9)
----------------------------------------------------------
INSERT INTO NhanVien (MaTaiKhoan, HoTen, GioiTinh, NgaySinh, CCCD, SoDienThoai, DiaChi, ChucVu, TrangThaiLamViec)
VALUES
(2, N'Nguyễn Văn Quản Lý 1', N'Nam', '1990-01-01', '001090000001', '0908123456', N'Hà Nội', 1, 1),
(3, N'Trần Thị Quản Lý 2', N'Nữ', '1991-02-02', '001091000002', '0976234567', N'Hà Nội', 1, 1),
(4, N'Lê Văn Kỹ Thuật 1', N'Nam', '1995-03-03', '001095000003', '0369123456', N'Hải Phòng', 3, 1),
(5, N'Phạm Văn Kỹ Thuật 2', N'Nam', '1996-04-04', '001096000004', '0388234567', N'Nam Định', 3, 1),
(6, N'Vũ Thị Nhân Viên 1', N'Nữ', '1998-05-05', '001098000005', '0917345678', N'Hà Nam', 4, 1),
(7, N'Hoàng Văn Nhân Viên 2', N'Nam', '1999-06-06', '001099000006', '0706456789', N'Hà Nội', 4, 1),
(8, N'Ngô Thị Nhân Viên 3', N'Nữ', '2000-07-07', '001100000007', '0815567890', N'Bắc Ninh', 4, 1),
(9, N'Đỗ Văn Nhân Viên 4', N'Nam', '2001-08-08', '001101000008', '0324678901', N'Hưng Yên', 2, 1),
(10, N'Lê Văn Nhân Viên 5', N'Nam', '2002-09-09', '001102000009', '0943789012', N'Hà Nội', 2, 1);
GO

----------------------------------------------------------
-- 3. KHÁCH HÀNG (10)
----------------------------------------------------------
INSERT INTO KhachHang (MaTaiKhoan, SoDienThoai, HoTen, CCCD, DiaChi, BienSoXeMacDinh)
VALUES
(11, '0909123001', N'Nguyễn Văn Khách 1', '001103000011', N'Hà Nội', '30F-111.11'),
(12, '0978123002', N'Trần Thị Khách 2', '001104000012', N'Hải Phòng', '15A-222.22'),
(13, '0367123003', N'Lê Văn Khách 3', '001105000013', N'Nam Định', '18B-333.33'),
(14, '0388345004', N'Phạm Ngọc Khách 4', '001106000014', N'Hà Nam', '90C-444.44'),
(15, '0707456005', N'Vũ Thị Khách 5', '001107000015', N'Ninh Bình', '35D-555.55'),
(16, '0818567006', N'Khách Vãng Lai 1', '001108000016', N'Hà Nội', '29A-666.66'),
(17, '0329678007', N'Khách Vãng Lai 2', '001109000017', N'Thái Bình', '17C-777.77'),
(18, '0943789008', N'Khách Vãng Lai 3', '001110000018', N'Hưng Yên', '89B-888.88'),
(19, '0832890009', N'Khách Vãng Lai 4', '001111000019', N'Bắc Ninh', '99A-999.99'),
(20, '0909880010', N'Khách Vãng Lai 5', '001112000020', N'Hà Nội', '30H-000.00');
GO

----------------------------------------------------------
-- 4. CA LÀM VIỆC
----------------------------------------------------------
INSERT INTO CaLamViec (MaNhanVien, ThoiGianNhanCa, ThoiGianGiaoCa, TienDauCa, TongTienHeThong, TienMatBanGiao, GhiChuBanGiao, TrangThaiCa)
VALUES
(1, '2026-01-30 07:00:00', '2026-01-30 15:00:00', 2000000, 5500000, 5500000, N'Ca sáng ổn định', 1),
(2, '2026-01-30 15:00:00', '2026-01-30 23:00:00', 1500000, 4800000, 4800000, N'Không phát sinh lỗi', 1),
(3, '2026-01-31 07:00:00', '2026-01-31 15:00:00', 2000000, 6200000, 6200000, N'Khách đông buổi sáng', 1),
(4, '2026-01-31 15:00:00', '2026-01-31 23:00:00', 1800000, 5100000, 5100000, N'Bàn giao đầy đủ', 1),
(5, '2026-02-01 07:00:00', '2026-02-01 15:00:00', 2000000, 5900000, 5900000, N'Không thiếu tiền', 1),
(6, '2026-02-01 15:00:00', NULL, 1500000, 2100000, 0, N'Ca chiều đang trực', 0),
(7, '2026-02-02 07:00:00', NULL, 2000000, 1800000, 0, N'Ca sáng đang trực', 0),
(8, '2026-02-02 15:00:00', NULL, 1500000, 950000, 0, N'Khách chưa đông', 0),
(9, '2026-02-03 07:00:00', NULL, 2000000, 1300000, 0, N'Ca mới nhận', 0),
(3, '2026-02-03 15:00:00', NULL, 1800000, 600000, 0, N'Nhân viên tăng ca', 0);
GO

----------------------------------------------------------
-- 5. LOẠI XE
----------------------------------------------------------
INSERT INTO LoaiXe (TenLoaiXe, MoTa) VALUES
(N'Xe máy', N'Xe máy, xe tay ga'),
(N'Ô tô 4 chỗ', N'Xe ô tô con 4–5 chỗ'),
(N'Ô tô 7 chỗ', N'Xe ô tô gia đình 7 chỗ'),
(N'Xe tải', N'Xe tải nhỏ và trung bình');
GO

----------------------------------------------------------
-- 6. CẤU HÌNH GIÁ
----------------------------------------------------------
-- XE MÁY
INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, GioBatDau, GioKetThuc) VALUES (N'Giá Xe máy Ngày', 1, '06:00:00', '18:00:00');
DECLARE @XeMayNgay INT = SCOPE_IDENTITY();
INSERT INTO ChiTietGia (MaCauHinh, ThuTuBlock, SoPhutCuaBlock, GiaTien, IsLuyTien) VALUES (@XeMayNgay, 1, 60, 5000, 0), (@XeMayNgay, 2, 60, 3000, 1);

-- Ô TÔ 4 CHỖ
INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, GioBatDau, GioKetThuc) VALUES (N'Giá Ô tô 4 chỗ Ngày', 2, '06:00:00', '18:00:00');
DECLARE @Oto4Ngay INT = SCOPE_IDENTITY();
INSERT INTO ChiTietGia VALUES (@Oto4Ngay, 1, 120, 20000, 0), (@Oto4Ngay, 2, 60, 10000, 1);

-- Ô TÔ 7 CHỖ
INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, GioBatDau, GioKetThuc) VALUES (N'Giá Ô tô 7 chỗ Ngày', 3, '06:00:00', '18:00:00');
DECLARE @Oto7Ngay INT = SCOPE_IDENTITY();
INSERT INTO ChiTietGia VALUES (@Oto7Ngay, 1, 120, 30000, 0), (@Oto7Ngay, 2, 60, 15000, 1);

-- XE TẢI
INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, GioBatDau, GioKetThuc) VALUES (N'Giá Xe tải Ngày', 4, '06:00:00', '18:00:00');
DECLARE @XeTaiNgay INT = SCOPE_IDENTITY();
INSERT INTO ChiTietGia VALUES (@XeTaiNgay, 1, 120, 40000, 0), (@XeTaiNgay, 2, 60, 20000, 1);
GO

----------------------------------------------------------
-- 7. KHU VỰC & VỊ TRÍ ĐỖ
----------------------------------------------------------
INSERT INTO KhuVuc (TenKhuVuc) VALUES (N'Khu A'), (N'Khu B'), (N'Khu C'), (N'Khu D'), (N'Khu E');
GO

INSERT INTO ViTriDo (MaKhuVuc, TenViTri, TrangThai) VALUES
(1, 'A01', 0), (1, 'A02', 1), (1, 'A03', 0), (1, 'A04', 2),
(2, 'B01', 0), (2, 'B02', 3), (2, 'B03', 1), (2, 'B04', 0),
(3, 'C01', 0), (3, 'C02', 0), (3, 'C03', 2),
(4, 'D01', 0), (4, 'D02', 1), (4, 'D03', 0),
(5, 'E01', 0), (5, 'E02', 3), (5, 'E03', 0), (5, 'E04', 1);
GO

INSERT INTO DatCho (MaKhachHang, MaViTri, ThoiGianDat, ThoiGianDenDuKien, ThoiGianHetHan, TrangThaiDatCho) VALUES
(1, 4, GETDATE(), DATEADD(MINUTE, 15, GETDATE()), DATEADD(MINUTE, 30, GETDATE()), 0),
(2, 11, GETDATE(), DATEADD(MINUTE, 10, GETDATE()), DATEADD(MINUTE, 25, GETDATE()), 1),
(3, 14, GETDATE(), DATEADD(MINUTE, 20, GETDATE()), DATEADD(MINUTE, 40, GETDATE()), 0),
(1, 17, GETDATE(), DATEADD(MINUTE, 30, GETDATE()), DATEADD(MINUTE, 60, GETDATE()), 2);
GO

----------------------------------------------------------
-- 8. THẺ XE & VÉ THÁNG
----------------------------------------------------------
INSERT INTO TheXe (MaThe, MaLoaiXe, LoaiThe, TrangThai) VALUES
('THE-XM-001', 1, 1, 1), ('THE-XM-002', 1, 1, 1),
('THE-OTO4-001', 2, 1, 1), ('THE-OTO7-001', 3, 1, 1),
('THE-OTO7-002', 3, 0, 1), ('THE-TAI-001', 4, 1, 1),
('THE-VL-001', 1, 0, 1), ('THE-VL-002', 2, 0, 1);
GO

INSERT INTO VeThang (MaKhachHang, MaThe, NgayBatDau, NgayHetHan, SoTienDong, TrangThai) VALUES
(1, 'THE-XM-001', '2025-01-01', '2025-01-31', 150000, 1),
(2, 'THE-XM-002', '2025-01-05', '2025-02-04', 150000, 1),
(1, 'THE-OTO4-001', '2025-01-01', '2025-01-31', 800000, 1),
(3, 'THE-OTO7-001', '2025-01-10', '2025-02-09', 1000000, 1),
(2, 'THE-TAI-001', '2025-01-15', '2025-02-14', 1200000, 1);
GO

INSERT INTO LichSuGiaHanVe (MaVeThang, NgayGiaHan, ThoiHanCu, ThoiHanMoi, SoTien, MaNhanVienThucHien) VALUES
(2, GETDATE(), '2025-01-31', '2025-02-28', 150000, 1),
(3, GETDATE(), '2025-01-31', '2025-02-28', 800000, 2),
(4, GETDATE(), '2025-02-09', '2025-03-10', 1000000, 1);
GO

----------------------------------------------------------
-- 9. LƯỢT GỬI & SỰ CỐ
----------------------------------------------------------
INSERT INTO LuotGui (MaThe, MaDatCho, MaCaVao, ThoiGianVao, BienSoVao, MaViTri, MaCaRa, ThoiGianRa, BienSoRa, TongTien, TrangThai) VALUES
('THE-XM-001', NULL, 7, DATEADD(MINUTE, -30, GETDATE()), '30F-123.45', 4, NULL, NULL, NULL, 0, 0),
('THE-OTO4-001', 1, 7, DATEADD(HOUR, -3, GETDATE()), '29A-456.78', 6, 8, DATEADD(HOUR, -1, GETDATE()), '29A-456.78', 30000, 1),
('THE-XM-002', NULL, 8, DATEADD(HOUR, -5, GETDATE()), '99B-888.99', 8, 9, DATEADD(HOUR, -2, GETDATE()), '99B-888.99', 0, 1);
GO

INSERT INTO SuCo (ThoiGianGhiNhan, MaNhanVien, LoaiSuCo, MaThe, MaViTri, MoTaChiTiet, TrangThaiXuLy) VALUES
(GETDATE(), 1, N'Xe mất thẻ', 'THE-XM-001', 1, N'Khách làm mất thẻ xe máy khi ra cổng', 0),
(GETDATE(), 2, N'Xe mất thẻ', 'THE-OTO4-001', 4, N'Khách không tìm thấy thẻ gửi xe ô tô', 1),
(GETDATE(), 3, N'Lỗi barrier', NULL, NULL, N'Barrier không tự động mở khi quét thẻ', 0),
(GETDATE(), 1, N'Lỗi barrier', NULL, NULL, N'Barrier bị kẹt, cần bảo trì gấp', 2),
(GETDATE(), 2, N'Lỗi camera', NULL, 7, N'Camera khu C không ghi nhận được biển số', 1);
GO

----------------------------------------------------------
-- 10. LỊCH LÀM VIỆC (Đã fix để đảm bảo có MaLich từ 1-10)
----------------------------------------------------------
INSERT INTO LichLamViec (MaNhanVien, MaCa, NgayLamViec, GioBatDau, GioKetThuc, LoaiCa, TrangThai, GhiChu)
VALUES
(1, 1, '2026-01-30', '07:00:00', '15:00:00', 0, 1, N'Ca sáng'),
(2, 2, '2026-01-30', '15:00:00', '23:00:00', 0, 1, N'Ca chiều'),
(3, 3, '2026-01-31', '07:00:00', '15:00:00', 0, 1, N'Ca sáng'),
(4, 4, '2026-01-31', '15:00:00', '23:00:00', 0, 1, N'Ca chiều'),
(5, 5, '2026-02-01', '07:00:00', '15:00:00', 0, 1, N'Trực lễ'),
(6, 6, '2026-02-01', '15:00:00', '23:00:00', 0, 1, N'Trực chiều'),
(7, 7, '2026-02-02', '07:00:00', '15:00:00', 0, 1, N'Thứ 2 sáng'),
(8, 8, '2026-02-02', '15:00:00', '23:00:00', 0, 1, N'Thứ 2 chiều'),
(3, 10, '2026-02-03', '15:00:00', '21:00:00', 1, 1, N'Tăng ca'),
(9, 9, '2026-02-03', '07:00:00', '15:00:00', 0, 1, N'Thứ 3 sáng');
GO

----------------------------------------------------------
-- 11. ĐĂNG KÝ LỊCH (Sử dụng MaLich chắc chắn tồn tại)
----------------------------------------------------------
-- Lưu ý: Kiểm tra xem bảng LichLamViec đã có dữ liệu chưa trước khi chạy dòng này
INSERT INTO DangKyLich 
(MaNhanVien, MaLich, LoaiYeuCau, NgayYeuCau, NgayLamMoi, GioBatDauMoi, GioKetThucMoi, LyDo, TrangThaiDuyet, MaNhanVienDuyet, ThoiGianDuyet, GhiChuDuyet)
VALUES
(5, 5, 0, GETDATE(), NULL, NULL, NULL, N'Ốm đột xuất', 1, 1, GETDATE(), N'Đã xác nhận'),
(6, 6, 0, GETDATE(), NULL, NULL, NULL, N'Việc gia đình', 2, 2, GETDATE(), N'Không người thay'),
(7, 7, 0, GETDATE(), NULL, NULL, NULL, N'Đi khám bệnh', 0, NULL, NULL, NULL),
(1, 1, 1, GETDATE(), '2026-02-05', '07:00:00', '15:00:00', N'Đổi ca đi học', 1, 1, GETDATE(), N'Đồng ý'),
(2, 2, 1, GETDATE(), '2026-02-06', '15:00:00', '23:00:00', N'Đổi với NV khác', 0, NULL, NULL, NULL),
(3, 3, 2, GETDATE(), '2026-01-31', '08:00:00', '16:00:00', N'Đi muộn 1 tiếng', 1, 2, GETDATE(), N'Chấp nhận'),
(4, 4, 2, GETDATE(), '2026-01-31', '16:00:00', '00:00:00', N'Làm bù giờ', 3, NULL, NULL, N'Hủy'),
(8, 8, 0, GETDATE(), NULL, NULL, NULL, N'Nghỉ phép năm', 0, NULL, NULL, NULL),
(9, 9, 1, GETDATE(), '2026-02-04', '07:00:00', '15:00:00', N'Đổi ca trực', 1, 1, GETDATE(), N'Ok'),
(4, 4, 2, GETDATE(), '2026-01-31', '14:00:00', '22:00:00', N'Vào sớm', 2, 1, GETDATE(), N'Từ chối');
GO

PRINT N'✔ Đã insert FULL seed data cho QuanLyBaiDoXe';