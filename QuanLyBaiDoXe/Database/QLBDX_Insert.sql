USE QuanLyBaiDoXe;
GO

-- Xóa dữ liệu cũ để tránh lỗi UNIQUE/ID
DELETE FROM SuCo; DELETE FROM LuotGui; DELETE FROM DatCho; 
DELETE FROM LichSuGiaHanVe; DELETE FROM VeThang; DELETE FROM TheXe;
DELETE FROM ViTriDo; DELETE FROM KhuVuc; DELETE FROM ChiTietGia; 
DELETE FROM CauHinhGia; DELETE FROM LoaiXe; DELETE FROM CaLamViec;
DELETE FROM NhanVien; DELETE FROM KhachHang; DELETE FROM TaiKhoan;

DBCC CHECKIDENT ('TaiKhoan', RESEED, 0);
DBCC CHECKIDENT ('NhanVien', RESEED, 0);
DBCC CHECKIDENT ('KhachHang', RESEED, 0);
DBCC CHECKIDENT ('CaLamViec', RESEED, 0);
DBCC CHECKIDENT ('LoaiXe', RESEED, 0);
DBCC CHECKIDENT ('CauHinhGia', RESEED, 0);
DBCC CHECKIDENT ('ChiTietGia', RESEED, 0);
DBCC CHECKIDENT ('KhuVuc', RESEED, 0);
DBCC CHECKIDENT ('ViTriDo', RESEED, 0);
DBCC CHECKIDENT ('DatCho', RESEED, 0);
DBCC CHECKIDENT ('VeThang', RESEED, 0);
DBCC CHECKIDENT ('LichSuGiaHanVe', RESEED, 0);
DBCC CHECKIDENT ('SuCo', RESEED, 0);
GO

----------------------------------------------------------
-- 1. BẢNG TÀI KHOẢN
----------------------------------------------------------
INSERT INTO TaiKhoan (TenDangNhap, MatKhau) VALUES 
('admin', 'pass123'), ('baove01', 'pass123'), ('baove02', 'pass123'), 
('kythuat01', 'pass123'), ('ketoan01', 'pass123'), ('khach01', 'pass123'), 
('khach02', 'pass123'), ('khach03', 'pass123'), ('khach04', 'pass123'), ('khach05', 'pass123');
GO

----------------------------------------------------------
-- 2. BẢNG NHÂN VIÊN
----------------------------------------------------------
INSERT INTO NhanVien (MaTaiKhoan, HoTen, ChucVu, SoDienThoai) VALUES 
(1, N'Nguyễn Văn Quản Trị', 0, '0901000001'), (2, N'Lê Văn Bảo 1', 1, '0901000002'),
(3, N'Phạm Văn Bảo 2', 1, '0901000003'), (4, N'Trần Văn Kỹ 1', 2, '0901000004'),
(5, N'Đặng Thị Kế 1', 3, '0901000005'), (NULL, N'Nguyễn Văn Thời Vụ 1', 1, '0901000006'),
(NULL, N'Nguyễn Văn Thời Vụ 2', 1, '0901000007'), (NULL, N'Bùi Văn Tạp Vụ', 1, '0901000008'),
(NULL, N'Lý Văn Kỹ 2', 2, '0901000009'), (NULL, N'Hoàng Thị Kế 2', 3, '0901000010');
GO

----------------------------------------------------------
-- 3. BẢNG KHÁCH HÀNG 
----------------------------------------------------------
INSERT INTO KhachHang (MaTaiKhoan, HoTen, SoDienThoai, BienSoXeMacDinh) VALUES 
(6, N'Khách Thân Thiết 1', '0911000001', '51A-111.11'), (7, N'Khách Thân Thiết 2', '0911000002', '51B-222.22'),
(8, N'Khách Thân Thiết 3', '0911000003', '51C-333.33'), (9, N'Khách Thân Thiết 4', '0911000004', '51D-444.44'),
(10, N'Khách Thân Thiết 5', '0911000005', '51E-555.55'), (NULL, N'Khách Vãng Lai A', '0911000006', NULL),
(NULL, N'Khách Vãng Lai B', '0911000007', NULL), (NULL, N'Khách Vãng Lai C', '0911000008', NULL),
(NULL, N'Khách Vãng Lai D', '0911000009', NULL), (NULL, N'Khách Vãng Lai E', '0911000010', NULL);
GO

----------------------------------------------------------
-- 4. BẢNG CA LÀM VIỆC 
----------------------------------------------------------
INSERT INTO CaLamViec (MaNhanVien, ThoiGianNhanCa, TienDauCa, TrangThaiCa) VALUES 
(2, GETDATE(), 500000, 0), (3, GETDATE(), 500000, 0), (5, GETDATE(), 0, 1),
(2, DATEADD(DAY,-1,GETDATE()), 500000, 1), (3, DATEADD(DAY,-1,GETDATE()), 500000, 1),
(6, GETDATE(), 200000, 0), (7, GETDATE(), 200000, 0), (8, GETDATE(), 100000, 0),
(1, GETDATE(), 0, 0), (4, GETDATE(), 100000, 0);
GO

----------------------------------------------------------
-- 5. BẢNG LOẠI XE & CẤU HÌNH GIÁ 
----------------------------------------------------------
INSERT INTO LoaiXe (TenLoaiXe, MoTa) VALUES 
(N'Xe Máy', N'Xe 2 bánh'), (N'Ô tô 4 chỗ', N'Xe con nhỏ'), (N'Ô tô 7 chỗ', N'Xe gia đình'),
(N'Xe Tải Nhỏ', N'Dưới 2.5 tấn'), (N'Xe Đạp Điện', N'Xe 2 bánh điện'), (N'Xe Máy Điện', N'VinFast/Pega'),
(N'Ô tô Điện', N'Trạm sạc riêng'), (N'Xe Khách', N'Trên 16 chỗ'), (N'Xe Ưu Tiên', N'Cứu thương/Công vụ'), (N'Xe VIP', N'Đối tác');
GO

INSERT INTO CauHinhGia (TenCauHinh, MaLoaiXe, LoaiGia) VALUES 
(N'Giá Xe Máy Ngày', 1, 0), (N'Giá Ô tô 4c Ngày', 2, 0), (N'Giá Ô tô 7c Ngày', 3, 0),
(N'Giá Xe Máy Đêm', 1, 1), (N'Giá Ô tô Đêm', 2, 1), (N'Giá Ngày Lễ XM', 1, 2),
(N'Giá Ngày Lễ Ô tô', 2, 2), (N'Giá Xe Tải', 4, 0), (N'Giá Xe Đạp Điện', 5, 0), (N'Giá Xe VIP', 10, 0);
GO

----------------------------------------------------------
-- 6. BẢNG CHI TIẾT GIÁ 
----------------------------------------------------------
INSERT INTO ChiTietGia (MaCauHinh, ThuTuBlock, SoPhutCuaBlock, GiaTien, IsLuyTien) VALUES 
(1, 1, 480, 5000, 0), (2, 1, 120, 20000, 0), (2, 2, 60, 10000, 1),
(3, 1, 120, 30000, 0), (3, 2, 60, 15000, 1), (4, 1, 600, 10000, 0),
(5, 1, 600, 50000, 0), (6, 1, 1440, 15000, 0), (7, 1, 1440, 100000, 0), (8, 1, 60, 20000, 1);
GO

----------------------------------------------------------
-- 7. BẢNG KHU VỰC & VỊ TRÍ ĐỖ 
----------------------------------------------------------
INSERT INTO KhuVuc (TenKhuVuc) VALUES 
(N'Hầm B1-A'), (N'Hầm B1-B'), (N'Hầm B2-A'), (N'Hầm B2-B'), (N'Sân Thượng'),
(N'Khu Ngoài Trời'), (N'Khu Xe VIP'), (N'Khu Xe Điện'), (N'Khu Xe Tải'), (N'Khu Rửa Xe');
GO

INSERT INTO ViTriDo (MaKhuVuc, TenViTri, TrangThai) VALUES 
(1, 'A-01', 0), (1, 'A-02', 1), (1, 'A-03', 2), (2, 'B-01', 0), (2, 'B-02', 3),
(3, 'C-01', 0), (4, 'D-01', 1), (7, 'VIP-01', 0), (8, 'E-01', 0), (9, 'T-01', 3);
GO

----------------------------------------------------------
-- 8. BẢNG ĐẶT CHỖ (10 Dòng)
----------------------------------------------------------
INSERT INTO DatCho (MaKhachHang, MaViTri, ThoiGianDenDuKien, TrangThaiDatCho) VALUES 
(1, 1, GETDATE(), 0), (2, 4, GETDATE(), 0), (3, 6, GETDATE(), 1),
(4, 8, GETDATE(), 2), (5, 9, GETDATE(), 3), (1, 2, GETDATE(), 0),
(2, 3, GETDATE(), 1), (3, 5, GETDATE(), 2), (4, 7, GETDATE(), 0), (5, 1, GETDATE(), 0);
GO

----------------------------------------------------------
-- 9. BẢNG THẺ XE & VÉ THÁNG 
----------------------------------------------------------
INSERT INTO TheXe (MaThe, MaLoaiXe, LoaiThe) VALUES 
('XM001', 1, 0), ('XM002', 1, 0), ('OT001', 2, 0), ('OT002', 2, 0), ('MT001', 1, 1),
('MT002', 1, 1), ('OTM01', 2, 1), ('OTM02', 2, 1), ('VIP01', 10, 1), ('ED001', 7, 0);
GO

INSERT INTO VeThang (MaKhachHang, MaThe, NgayHetHan, SoTienDong) VALUES 
(1, 'MT001', '2026-12-31', 150000), (2, 'MT002', '2026-12-31', 150000),
(3, 'OTM01', '2026-12-31', 1200000), (4, 'OTM02', '2026-12-31', 1200000),
(5, 'VIP01', '2027-01-01', 5000000), (1, 'XM001', '2026-02-01', 150000),
(2, 'XM002', '2026-02-01', 150000), (3, 'OT001', '2026-02-01', 1000000),
(4, 'OT002', '2026-02-01', 1000000), (5, 'ED001', '2026-02-01', 800000);
GO

----------------------------------------------------------
-- 10. BẢNG LỊCH SỬ GIA HẠN 
----------------------------------------------------------
INSERT INTO LichSuGiaHanVe (MaVeThang, ThoiHanMoi, SoTien, MaNhanVienThucHien) VALUES 
(1, '2026-03-01', 150000, 5), (2, '2026-03-01', 150000, 5), (3, '2026-03-01', 1200000, 5),
(4, '2026-03-01', 1200000, 5), (5, '2027-02-01', 5000000, 5), (1, '2026-04-01', 150000, 5),
(2, '2026-04-01', 150000, 5), (3, '2026-04-01', 1200000, 5), (4, '2026-04-01', 1200000, 5), (5, '2027-03-01', 5000000, 5);
GO

----------------------------------------------------------
-- 11. BẢNG LƯỢT GỬI & SỰ CỐ 
----------------------------------------------------------
INSERT INTO LuotGui (MaThe, MaCaVao, ThoiGianVao, BienSoVao, MaViTri, TrangThai) VALUES 
('XM001', 1, GETDATE(), '51A-1234', 1, 0), ('XM002', 1, GETDATE(), '51B-5678', 2, 0),
('OT001', 2, GETDATE(), '51C-9999', 4, 0), ('OT002', 2, GETDATE(), '51D-8888', 7, 0),
('MT001', 1, DATEADD(HOUR,-5,GETDATE()), '51E-1111', 3, 0), ('MT002', 1, DATEADD(HOUR,-2,GETDATE()), '51F-2222', 5, 0),
('OTM01', 2, DATEADD(HOUR,-10,GETDATE()), '51G-3333', 1, 1), ('OTM02', 2, DATEADD(HOUR,-1,GETDATE()), '51H-4444', 2, 1),
('VIP01', 3, GETDATE(), 'VIP-001', 8, 0), ('ED001', 3, GETDATE(), 'ED-001', 9, 0);
GO

INSERT INTO SuCo (MaNhanVien, LoaiSuCo, MaThe, MoTaChiTiet, TrangThaiXuLy) VALUES 
(2, N'Mất thẻ', 'XM001', N'Khách báo mất thẻ lúc 10h', 1), (3, N'Va chạm', NULL, N'Xe 51A quẹt vào tường', 0),
(2, N'Lỗi cảm biến', NULL, N'Vị trí A-02 không nhận xe', 2), (4, N'Hư thanh chắn', NULL, N'Thanh chắn cổng vào bị kẹt', 1),
(5, N'Sai biển số', 'OT001', N'Camera nhận diện nhầm số 8 thành 3', 2), (2, N'Khách quên vị trí', NULL, N'Hỗ trợ khách tìm xe khu B1', 2),
(3, N'Nghi vấn trộm cắp', 'XM002', N'Đối tượng khả nghi lảng vảng', 1), (4, N'Cháy bóng đèn', NULL, N'Khu vực D-01 tối', 0),
(5, N'Thu phí sai', 'OTM01', N'Hệ thống tính nhầm giờ', 2), (2, N'Mất điện', NULL, N'Chạy máy phát điện dự phòng', 1);
GO

PRINT N'--- ĐÃ NẠP HOÀN TẤT DỮ LIỆU ---';