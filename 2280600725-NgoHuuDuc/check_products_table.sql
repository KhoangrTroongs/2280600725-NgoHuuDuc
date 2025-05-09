-- Check the structure of the Products table
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products'
ORDER BY ORDINAL_POSITION;

-- Check the migration history
SELECT * FROM [__EFMigrationsHistory];
