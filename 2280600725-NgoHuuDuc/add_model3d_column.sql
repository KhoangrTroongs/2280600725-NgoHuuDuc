-- Check if the column already exists
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Products' 
    AND COLUMN_NAME = 'Model3DUrl'
)
BEGIN
    -- Add the Model3DUrl column to the Products table
    ALTER TABLE Products
    ADD Model3DUrl NVARCHAR(MAX) NULL;
    
    PRINT 'Model3DUrl column added successfully.';
END
ELSE
BEGIN
    PRINT 'Model3DUrl column already exists.';
END
