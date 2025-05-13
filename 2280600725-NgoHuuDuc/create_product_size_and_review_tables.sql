-- Check if the ProductSizes table exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductSizes')
BEGIN
    -- Create ProductSizes table
    CREATE TABLE ProductSizes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        Size NVARCHAR(50) NOT NULL,
        Quantity INT NOT NULL DEFAULT 0,
        CONSTRAINT FK_ProductSizes_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
    );
    
    PRINT 'ProductSizes table created successfully.';
END
ELSE
BEGIN
    PRINT 'ProductSizes table already exists.';
END

-- Check if the ProductReviews table exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductReviews')
BEGIN
    -- Create ProductReviews table
    CREATE TABLE ProductReviews (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        Rating INT NOT NULL,
        Comment NVARCHAR(MAX) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ProductReviews_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductReviews_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    PRINT 'ProductReviews table created successfully.';
END
ELSE
BEGIN
    PRINT 'ProductReviews table already exists.';
END

-- Add these tables to the migration history
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20250513000000_AddProductSizeAndReview'
)
BEGIN
    -- Insert the migration record into the history table
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250513000000_AddProductSizeAndReview', '9.0.4');
    
    PRINT 'Migration history updated successfully.';
END
ELSE
BEGIN
    PRINT 'Migration already exists in history.';
END
