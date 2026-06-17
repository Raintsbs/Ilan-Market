-- Platform features: messaging, offers, reports, analytics, payments, notifications
-- Run against your AdvertisementApp database after backup.
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ViewCount')
BEGIN
    ALTER TABLE Advertisements ADD ViewCount INT NOT NULL CONSTRAINT DF_Advertisements_ViewCount DEFAULT 0;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'IsFeatured')
BEGIN
    ALTER TABLE Advertisements ADD IsFeatured BIT NOT NULL CONSTRAINT DF_Advertisements_IsFeatured DEFAULT 0;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'FeaturedUntil')
BEGIN
    ALTER TABLE Advertisements ADD FeaturedUntil DATETIME2 NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'IsVerified')
BEGIN
    ALTER TABLE AspNetUsers ADD IsVerified BIT NOT NULL CONSTRAINT DF_AspNetUsers_IsVerified DEFAULT 0;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'OAuthProvider')
BEGIN
    ALTER TABLE AspNetUsers ADD OAuthProvider NVARCHAR(32) NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'OAuthSubject')
BEGIN
    ALTER TABLE AspNetUsers ADD OAuthSubject NVARCHAR(256) NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Favorites') AND name = 'PriceAlertEnabled')
BEGIN
    ALTER TABLE Favorites ADD PriceAlertEnabled BIT NOT NULL CONSTRAINT DF_Favorites_PriceAlert DEFAULT 0;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Favorites') AND name = 'AlertPrice')
BEGIN
    ALTER TABLE Favorites ADD AlertPrice DECIMAL(18,2) NULL;
END
GO

IF OBJECT_ID(N'MessageThreads', N'U') IS NULL
BEGIN
    CREATE TABLE MessageThreads (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AdvertisementId INT NOT NULL REFERENCES Advertisements(Id) ON DELETE CASCADE,
        BuyerUserId INT NOT NULL,
        SellerUserId INT NOT NULL,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedTime DATETIME2 NULL
    );
    CREATE UNIQUE INDEX IX_MessageThreads_Ad_Buyer ON MessageThreads(AdvertisementId, BuyerUserId);
END
GO
IF OBJECT_ID(N'Messages', N'U') IS NULL
BEGIN
    CREATE TABLE Messages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ThreadId INT NOT NULL REFERENCES MessageThreads(Id) ON DELETE CASCADE,
        SenderUserId INT NOT NULL,
        Body NVARCHAR(4000) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'Offers', N'U') IS NULL
BEGIN
    CREATE TABLE Offers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AdvertisementId INT NOT NULL REFERENCES Advertisements(Id) ON DELETE CASCADE,
        BuyerUserId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Message NVARCHAR(500) NULL,
        Status INT NOT NULL DEFAULT 0,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'ListingReports', N'U') IS NULL
BEGIN
    CREATE TABLE ListingReports (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AdvertisementId INT NOT NULL REFERENCES Advertisements(Id) ON DELETE CASCADE,
        ReporterUserId INT NOT NULL,
        Reason NVARCHAR(100) NOT NULL,
        Details NVARCHAR(2000) NULL,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_ListingReports_Ad_Reporter ON ListingReports(AdvertisementId, ReporterUserId);
END
GO
IF OBJECT_ID(N'SavedSearches', N'U') IS NULL
BEGIN
    CREATE TABLE SavedSearches (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Name NVARCHAR(120) NOT NULL,
        FilterJson NVARCHAR(MAX) NOT NULL,
        NotifyOnNew BIT NOT NULL DEFAULT 0,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Body NVARCHAR(1000) NOT NULL,
        Link NVARCHAR(500) NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'SearchLogs', N'U') IS NULL
BEGIN
    CREATE TABLE SearchLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        CategoryId INT NULL,
        SearchTerm NVARCHAR(200) NULL,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'AdPackages', N'U') IS NULL
BEGIN
    CREATE TABLE AdPackages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(120) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        FeaturedDays INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
    IF NOT EXISTS (SELECT 1 FROM AdPackages)
    BEGIN
        INSERT INTO AdPackages (Code, Name, Price, FeaturedDays, IsActive) VALUES
        ('featured_7', N'Öne çıkan 7 gün', 99.00, 7, 1),
        ('featured_30', N'Öne çıkan 30 gün', 299.00, 30, 1);
    END
END
GO
IF OBJECT_ID(N'UserPurchases', N'U') IS NULL
BEGIN
    CREATE TABLE UserPurchases (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        AdvertisementId INT NULL REFERENCES Advertisements(Id),
        AdPackageId INT NOT NULL REFERENCES AdPackages(Id),
        Status NVARCHAR(30) NOT NULL DEFAULT 'pending',
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
PRINT 'Platform features migration completed.';
