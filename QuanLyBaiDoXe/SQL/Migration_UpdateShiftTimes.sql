-- =============================================
-- Script: Cập nhật thời gian ca làm việc
-- Mô tả: Chuyển đổi từ ca cũ sang ca mới
--   Ca cũ: Sáng (6-14h), Chiều (14-22h), Đêm (22-6h)
--   Ca mới: Sáng (0-8h), Chiều (8-16h), Tối (16-0h)
-- =============================================

BEGIN TRANSACTION;

PRINT N'Bắt đầu cập nhật thời gian ca làm việc...';

-- 1. Cập nhật Ca Sáng: 06:00 -> 00:00
UPDATE CaLamViec
SET ThoiGianNhanCa = DATEADD(HOUR, -6, ThoiGianNhanCa)
WHERE DATEPART(HOUR, ThoiGianNhanCa) = 6
  AND TrangThaiCa = 0; -- Chỉ cập nhật ca chưa chốt

PRINT N'✓ Đã cập nhật Ca Sáng: 06:00 -> 00:00';

-- 2. Cập nhật Ca Chiều: 14:00 -> 08:00  
UPDATE CaLamViec
SET ThoiGianNhanCa = DATEADD(HOUR, -6, ThoiGianNhanCa)
WHERE DATEPART(HOUR, ThoiGianNhanCa) = 14
  AND TrangThaiCa = 0;

PRINT N'✓ Đã cập nhật Ca Chiều: 14:00 -> 08:00';

-- 3. Cập nhật Ca Tối/Đêm: 22:00 -> 16:00
UPDATE CaLamViec
SET ThoiGianNhanCa = DATEADD(HOUR, -6, ThoiGianNhanCa)
WHERE DATEPART(HOUR, ThoiGianNhanCa) = 22
  AND TrangThaiCa = 0;

PRINT N'✓ Đã cập nhật Ca Tối: 22:00 -> 16:00';

-- 4. Cập nhật thời gian giao ca (nếu có)
-- Ca Sáng: 14:00 -> 08:00
UPDATE CaLamViec
SET ThoiGianGiaoCa = DATEADD(HOUR, -6, ThoiGianGiaoCa)
WHERE ThoiGianGiaoCa IS NOT NULL
  AND DATEPART(HOUR, ThoiGianGiaoCa) = 14
  AND TrangThaiCa = 0;

PRINT N'✓ Đã cập nhật giờ kết thúc Ca Sáng';

-- Ca Chiều: 22:00 -> 16:00
UPDATE CaLamViec
SET ThoiGianGiaoCa = DATEADD(HOUR, -6, ThoiGianGiaoCa)
WHERE ThoiGianGiaoCa IS NOT NULL
  AND DATEPART(HOUR, ThoiGianGiaoCa) = 22
  AND TrangThaiCa = 0;

PRINT N'✓ Đã cập nhật giờ kết thúc Ca Chiều';

-- Ca Tối: 06:00 (ngày hôm sau) -> 00:00 (ngày hôm sau)
UPDATE CaLamViec
SET ThoiGianGiaoCa = DATEADD(HOUR, -6, ThoiGianGiaoCa)
WHERE ThoiGianGiaoCa IS NOT NULL
  AND DATEPART(HOUR, ThoiGianGiaoCa) = 6
  AND TrangThaiCa = 0;

PRINT N'✓ Đã cập nhật giờ kết thúc Ca Tối';

-- 5. Kiểm tra kết quả
SELECT 
    COUNT(*) AS TongCaCapNhat,
    COUNT(CASE WHEN DATEPART(HOUR, ThoiGianNhanCa) = 0 THEN 1 END) AS CaSang_0h,
    COUNT(CASE WHEN DATEPART(HOUR, ThoiGianNhanCa) = 8 THEN 1 END) AS CaChieu_8h,
    COUNT(CASE WHEN DATEPART(HOUR, ThoiGianNhanCa) = 16 THEN 1 END) AS CaToi_16h
FROM CaLamViec
WHERE TrangThaiCa = 0;

PRINT N'';
PRINT N'=============================================';
PRINT N'Hoàn thành cập nhật thời gian ca làm việc!';
PRINT N'=============================================';

-- Nếu mọi thứ OK, commit transaction
COMMIT TRANSACTION;
PRINT N'✓ Transaction đã được commit thành công!';

-- Nếu có lỗi, rollback
-- ROLLBACK TRANSACTION;
-- PRINT N'✗ Transaction đã được rollback!';
