-- Kiểm tra xem cột Size đã tồn tại trong bảng CartItems chưa
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'CartItems' AND COLUMN_NAME = 'Size'
)
BEGIN
    -- Thêm cột Size vào bảng CartItems
    ALTER TABLE CartItems
    ADD Size NVARCHAR(50) NULL;
    
    PRINT 'Đã thêm cột Size vào bảng CartItems';
END
ELSE
BEGIN
    PRINT 'Cột Size đã tồn tại trong bảng CartItems';
END
