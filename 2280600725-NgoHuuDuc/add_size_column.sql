-- Thêm cột Size vào bảng CartItems nếu chưa tồn tại
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'CartItems' AND COLUMN_NAME = 'Size'
)
BEGIN
    ALTER TABLE CartItems
    ADD Size NVARCHAR(50) NULL;
    
    PRINT 'Đã thêm cột Size vào bảng CartItems';
END
ELSE
BEGIN
    PRINT 'Cột Size đã tồn tại trong bảng CartItems';
END

-- Thêm cột Size vào bảng OrderDetails nếu chưa tồn tại
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'OrderDetails' AND COLUMN_NAME = 'Size'
)
BEGIN
    ALTER TABLE OrderDetails
    ADD Size NVARCHAR(50) NULL;
    
    PRINT 'Đã thêm cột Size vào bảng OrderDetails';
END
ELSE
BEGIN
    PRINT 'Cột Size đã tồn tại trong bảng OrderDetails';
END
