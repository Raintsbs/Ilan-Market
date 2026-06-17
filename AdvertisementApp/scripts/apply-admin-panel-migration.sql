SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ExpiresAt')
    ALTER TABLE Advertisements ADD ExpiresAt DATETIME2 NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ArchivedAt')
    ALTER TABLE Advertisements ADD ArchivedAt DATETIME2 NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'RejectReason')
    ALTER TABLE Advertisements ADD RejectReason NVARCHAR(500) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'AdminNote')
    ALTER TABLE Advertisements ADD AdminNote NVARCHAR(1000) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Categories') AND name = 'ParentId')
    ALTER TABLE Categories ADD ParentId INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Categories') AND name = 'SortOrder')
    ALTER TABLE Categories ADD SortOrder INT NOT NULL CONSTRAINT DF_Categories_SortOrder DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Categories') AND name = 'FieldSchemaJson')
    ALTER TABLE Categories ADD FieldSchemaJson NVARCHAR(MAX) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'IsBanned')
    ALTER TABLE AspNetUsers ADD IsBanned BIT NOT NULL CONSTRAINT DF_AspNetUsers_IsBanned DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'IsFrozen')
    ALTER TABLE AspNetUsers ADD IsFrozen BIT NOT NULL CONSTRAINT DF_AspNetUsers_IsFrozen DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'WarningCount')
    ALTER TABLE AspNetUsers ADD WarningCount INT NOT NULL CONSTRAINT DF_AspNetUsers_Warnings DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'PhoneVerified')
    ALTER TABLE AspNetUsers ADD PhoneVerified BIT NOT NULL CONSTRAINT DF_AspNetUsers_PhoneVerified DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'BanReason')
    ALTER TABLE AspNetUsers ADD BanReason NVARCHAR(500) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'FrozenUntil')
    ALTER TABLE AspNetUsers ADD FrozenUntil DATETIME2 NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ListingReports') AND name = 'Status')
    ALTER TABLE ListingReports ADD Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ListingReports_Status DEFAULT 'open';
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ListingReports') AND name = 'AdminAction')
    ALTER TABLE ListingReports ADD AdminAction NVARCHAR(500) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ListingReports') AND name = 'ResolvedAt')
    ALTER TABLE ListingReports ADD ResolvedAt DATETIME2 NULL;
GO

IF OBJECT_ID(N'AdvertisementAuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE AdvertisementAuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AdvertisementId INT NOT NULL REFERENCES Advertisements(Id) ON DELETE CASCADE,
        ActorUserId INT NOT NULL,
        ActorEmail NVARCHAR(256) NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        Details NVARCHAR(2000) NULL,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'ActivityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE ActivityLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        Type NVARCHAR(50) NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        IpAddress NVARCHAR(64) NULL,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO
IF OBJECT_ID(N'StaticPages', N'U') IS NULL
BEGIN
    CREATE TABLE StaticPages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Slug NVARCHAR(80) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        UpdatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE UNIQUE INDEX IX_StaticPages_Slug ON StaticPages(Slug);
    IF NOT EXISTS (SELECT 1 FROM StaticPages WHERE Slug = 'hakkimizda')
        INSERT INTO StaticPages (Slug, Title, Content) VALUES ('hakkimizda', N'Hakkımızda', N'<p>İlanMarket hakkında</p>');
    IF NOT EXISTS (SELECT 1 FROM StaticPages WHERE Slug = 'gizlilik')
        INSERT INTO StaticPages (Slug, Title, Content) VALUES ('gizlilik', N'Gizlilik', N'<p>Gizlilik politikası</p>');
    IF NOT EXISTS (SELECT 1 FROM StaticPages WHERE Slug = 'sss')
        INSERT INTO StaticPages (Slug, Title, Content) VALUES ('sss', N'SSS', N'<p>Sıkça sorulan sorular</p>');
END
GO
IF OBJECT_ID(N'CityLocations', N'U') IS NULL
BEGIN
    CREATE TABLE CityLocations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        City NVARCHAR(80) NOT NULL,
        District NVARCHAR(80) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0
    );
END
GO
IF OBJECT_ID(N'BlogPosts', N'U') IS NULL
BEGIN
    CREATE TABLE BlogPosts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(120) NOT NULL,
        Summary NVARCHAR(500) NULL,
        Content NVARCHAR(MAX) NOT NULL,
        IsPublished BIT NOT NULL DEFAULT 0,
        CreatedTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        PublishedTime DATETIME2 NULL
    );
END
GO
PRINT 'Admin panel migration completed.';
