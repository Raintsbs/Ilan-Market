IF COL_LENGTH('Advertisements', 'ImagePathsJson') IS NULL
BEGIN
    ALTER TABLE [Advertisements] ADD [ImagePathsJson] nvarchar(4000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260603150000_AddAdvertisementImagePathsJson')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260603150000_AddAdvertisementImagePathsJson', N'8.0.26');
END
GO
