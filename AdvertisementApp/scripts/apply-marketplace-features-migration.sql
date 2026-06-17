-- Marketplace features: bump, phone reveal log
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'LastBumpedAt')
    ALTER TABLE Advertisements ADD LastBumpedAt DATETIME2 NULL;
GO

IF OBJECT_ID(N'PhoneRevealLogs', N'U') IS NULL
BEGIN
    CREATE TABLE PhoneRevealLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AdvertisementId INT NOT NULL,
        ViewerUserId INT NOT NULL,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_PhoneRevealLogs_Viewer_Date ON PhoneRevealLogs(ViewerUserId, CreatedTime);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'StripeSessionId')
    ALTER TABLE UserPurchases ADD StripeSessionId NVARCHAR(256) NULL;
GO
