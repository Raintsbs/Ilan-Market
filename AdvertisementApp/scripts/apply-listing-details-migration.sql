IF COL_LENGTH('Advertisements', 'ListingDetailsJson') IS NULL
BEGIN
    ALTER TABLE [Advertisements] ADD [ListingDetailsJson] nvarchar(max) NULL;
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260603170000_AddListingDetailsJson')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260603170000_AddListingDetailsJson', N'8.0.26');
END
