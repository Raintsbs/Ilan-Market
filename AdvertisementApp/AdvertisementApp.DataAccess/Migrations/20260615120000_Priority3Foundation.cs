using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260615120000_Priority3Foundation")]
    public partial class Priority3Foundation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ListingPrice')
                    ALTER TABLE Advertisements ADD ListingPrice DECIMAL(18,2) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ListingYear')
                    ALTER TABLE Advertisements ADD ListingYear INT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ListingMileageKm')
                    ALTER TABLE Advertisements ADD ListingMileageKm INT NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Advertisements_ListingPrice' AND object_id = OBJECT_ID('Advertisements'))
                    CREATE INDEX IX_Advertisements_ListingPrice ON Advertisements(ListingPrice) WHERE ListingPrice IS NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Advertisements_ListingYear' AND object_id = OBJECT_ID('Advertisements'))
                    CREATE INDEX IX_Advertisements_ListingYear ON Advertisements(ListingYear) WHERE ListingYear IS NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Advertisements_ListingMileageKm' AND object_id = OBJECT_ID('Advertisements'))
                    CREATE INDEX IX_Advertisements_ListingMileageKm ON Advertisements(ListingMileageKm) WHERE ListingMileageKm IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'ReferralCode')
                    ALTER TABLE AspNetUsers ADD ReferralCode NVARCHAR(16) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'ReferredByUserId')
                    ALTER TABLE AspNetUsers ADD ReferredByUserId INT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'StoreSlug')
                    ALTER TABLE AspNetUsers ADD StoreSlug NVARCHAR(64) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'CompanyName')
                    ALTER TABLE AspNetUsers ADD CompanyName NVARCHAR(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'StoreDescription')
                    ALTER TABLE AspNetUsers ADD StoreDescription NVARCHAR(2000) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'StoreBannerPath')
                    ALTER TABLE AspNetUsers ADD StoreBannerPath NVARCHAR(512) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'IsCorporateStore')
                    ALTER TABLE AspNetUsers ADD IsCorporateStore BIT NOT NULL CONSTRAINT DF_AspNetUsers_IsCorporateStore DEFAULT 0;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_ReferralCode' AND object_id = OBJECT_ID('AspNetUsers'))
                    CREATE UNIQUE INDEX IX_AspNetUsers_ReferralCode ON AspNetUsers(ReferralCode) WHERE ReferralCode IS NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_StoreSlug' AND object_id = OBJECT_ID('AspNetUsers'))
                    CREATE UNIQUE INDEX IX_AspNetUsers_StoreSlug ON AspNetUsers(StoreSlug) WHERE StoreSlug IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'VerificationRequests', N'U') IS NULL
                BEGIN
                    CREATE TABLE VerificationRequests (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        DocumentType NVARCHAR(32) NOT NULL,
                        FilePath NVARCHAR(512) NOT NULL,
                        Status NVARCHAR(32) NOT NULL,
                        AdminNote NVARCHAR(2000) NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        ReviewedAt DATETIME2 NULL,
                        ReviewedByUserId INT NULL
                    );
                    CREATE INDEX IX_VerificationRequests_Status ON VerificationRequests(Status);
                    CREATE INDEX IX_VerificationRequests_UserId ON VerificationRequests(UserId);
                END
                """);

            migrationBuilder.Sql("""
                -- Backfill indexed listing fields from JSON
                UPDATE Advertisements SET ListingPrice = TRY_CAST(JSON_VALUE(ListingDetailsJson, '$.price') AS DECIMAL(18,2))
                    WHERE ListingDetailsJson IS NOT NULL AND ListingPrice IS NULL;
                UPDATE Advertisements SET ListingYear = TRY_CAST(JSON_VALUE(ListingDetailsJson, '$.year') AS INT)
                    WHERE ListingDetailsJson IS NOT NULL AND ListingYear IS NULL;
                """);

            migrationBuilder.Sql("""
                -- Full-text catalog + index (SQL Server Express may skip)
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'AdvertisementFtCatalog')
                BEGIN TRY
                    CREATE FULLTEXT CATALOG AdvertisementFtCatalog AS DEFAULT;
                END TRY BEGIN CATCH END CATCH

                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Advertisements'))
                BEGIN TRY
                    CREATE FULLTEXT INDEX ON Advertisements(Title, Description, Content)
                    KEY INDEX PK_Advertisements ON AdvertisementFtCatalog
                    WITH CHANGE_TRACKING AUTO;
                END TRY BEGIN CATCH END CATCH
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
