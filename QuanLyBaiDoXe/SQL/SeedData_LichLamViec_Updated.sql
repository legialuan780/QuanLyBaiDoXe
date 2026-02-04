-- Updated LichLamViec seed data with new timesheet logic
-- Ca sáng: 00:00 - 08:00 (0-8h)
-- Ca chiều: 08:00 - 16:00 (8-16h)
-- Ca tối: 16:00 - 00:00 (16-24h)
-- LoaiCa: 0 = Thường | 1 = Tăng ca | 2 = Đêm
-- TrangThai: 0 = Nghỉ | 1 = Làm

INSERT INTO LichLamViec (MaNhanVien, MaCa, NgayLamViec, GioBatDau, GioKetThuc, LoaiCa, TrangThai, GhiChu)
VALUES
(1, 1, '2026-01-30', '08:00:00', '16:00:00', 0, 1, N'Ca chiều'),
(2, 2, '2026-01-30', '00:00:00', '08:00:00', 0, 1, N'Ca sáng'),
(3, 3, '2026-01-31', '08:00:00', '16:00:00', 0, 1, N'Ca chiều'),
(4, 4, '2026-01-31', '16:00:00', '00:00:00', 2, 1, N'Ca tối'),
(5, 5, '2026-02-01', '00:00:00', '08:00:00', 0, 1, N'Ca sáng - Trực lễ'),
(6, 6, '2026-02-01', '08:00:00', '16:00:00', 0, 1, N'Ca chiều - Trực lễ'),
(7, 7, '2026-02-02', '08:00:00', '16:00:00', 0, 1, N'Ca chiều - Thứ 2'),
(8, 8, '2026-02-02', '16:00:00', '00:00:00', 2, 1, N'Ca tối - Thứ 2'),
(3, 10, '2026-02-03', '16:00:00', '00:00:00', 2, 1, N'Ca tối - Tăng ca'),
(9, 9, '2026-02-03', '16:00:00', '00:00:00', 2, 1, N'Ca tối - Thứ 3');
GO
