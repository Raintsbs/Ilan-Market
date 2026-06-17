IF COL_LENGTH('AspNetUsers', 'ProfileImagePath') IS NULL
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ProfileImagePath] nvarchar(500) NULL;
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260603160000_AddUserProfileImagePath')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260603160000_AddUserProfileImagePath', N'8.0.26');
END
