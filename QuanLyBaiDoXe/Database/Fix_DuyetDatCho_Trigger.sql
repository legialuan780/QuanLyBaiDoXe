-- ========================================
-- FIX: Xóa trigger gây lỗi "Vị trí không còn ở trạng thái đã đặt!"
-- ========================================

-- Bước 1: Tìm tất cả trigger trên bảng DatCho
PRINT '=== DANH SÁCH TRIGGER TRÊN BẢNG DatCho ===';
SELECT 
    t.name AS TriggerName,
    OBJECT_NAME(t.parent_id) AS TableName,
    t.is_disabled AS IsDisabled,
    t.create_date AS CreatedDate
FROM sys.triggers t
WHERE t.parent_id = OBJECT_ID('DatCho');
GO

-- Bước 2: Xóa trigger nếu tồn tại
-- (Thay 'TenTrigger' bằng tên trigger thực tế từ kết quả trên)

-- Ví dụ các tên trigger có thể có:
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_DuyetDatCho')
BEGIN
    DROP TRIGGER trg_DuyetDatCho;
    PRINT N'✓ Đã xóa trigger: trg_DuyetDatCho';
END

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_UpdateDatCho')
BEGIN
    DROP TRIGGER trg_UpdateDatCho;
    PRINT N'✓ Đã xóa trigger: trg_UpdateDatCho';
END

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_CheckViTriDatCho')
BEGIN
    DROP TRIGGER trg_CheckViTriDatCho;
    PRINT N'✓ Đã xóa trigger: trg_CheckViTriDatCho';
END

-- Nếu bạn biết tên trigger chính xác, thêm vào đây:
-- DROP TRIGGER IF EXISTS [TenTriggerCuaBan];

PRINT '=== HOÀN TẤT ===';
PRINT N'Vui lòng kiểm tra lại danh sách trigger ở trên.';
PRINT N'Nếu còn trigger nào, hãy chạy: DROP TRIGGER [TenTrigger];';
GO
