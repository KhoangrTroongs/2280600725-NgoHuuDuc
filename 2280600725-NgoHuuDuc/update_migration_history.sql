-- Check if the migration is already in the history
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20250509041227_AddModel3DUrlToProduct'
)
BEGIN
    -- Insert the migration record into the history table
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250509041227_AddModel3DUrlToProduct', '9.0.4');
    
    PRINT 'Migration history updated successfully.';
END
ELSE
BEGIN
    PRINT 'Migration already exists in history.';
END
