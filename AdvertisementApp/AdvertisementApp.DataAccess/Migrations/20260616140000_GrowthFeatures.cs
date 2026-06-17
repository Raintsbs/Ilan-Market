using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260616140000_GrowthFeatures")]
    public partial class GrowthFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Favorites') AND name = 'LastKnownPrice')
                    ALTER TABLE Favorites ADD LastKnownPrice DECIMAL(18,2) NULL;

                IF OBJECT_ID(N'ListingQuestions', N'U') IS NULL
                BEGIN
                    CREATE TABLE ListingQuestions (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        AdvertisementId INT NOT NULL,
                        UserId INT NOT NULL,
                        Question NVARCHAR(1000) NOT NULL,
                        Answer NVARCHAR(2000) NULL,
                        AnsweredByUserId INT NULL,
                        CreatedTime DATETIME2 NOT NULL,
                        AnsweredTime DATETIME2 NULL,
                        IsHidden BIT NOT NULL CONSTRAINT DF_ListingQuestions_IsHidden DEFAULT 0
                    );
                    CREATE INDEX IX_ListingQuestions_AdvertisementId ON ListingQuestions(AdvertisementId);
                END

                IF OBJECT_ID(N'SellerFollows', N'U') IS NULL
                BEGIN
                    CREATE TABLE SellerFollows (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        FollowerUserId INT NOT NULL,
                        SellerUserId INT NOT NULL,
                        CreatedTime DATETIME2 NOT NULL
                    );
                    CREATE UNIQUE INDEX IX_SellerFollows_Follower_Seller ON SellerFollows(FollowerUserId, SellerUserId);
                    CREATE INDEX IX_SellerFollows_SellerUserId ON SellerFollows(SellerUserId);
                END

                IF OBJECT_ID(N'Coupons', N'U') IS NULL
                BEGIN
                    CREATE TABLE Coupons (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Code NVARCHAR(32) NOT NULL,
                        Description NVARCHAR(256) NULL,
                        DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Coupons_DiscountAmount DEFAULT 0,
                        DiscountPercent INT NULL,
                        MaxUses INT NOT NULL CONSTRAINT DF_Coupons_MaxUses DEFAULT 100,
                        UsedCount INT NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT 0,
                        ExpiresAt DATETIME2 NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Coupons_IsActive DEFAULT 1,
                        CreatedTime DATETIME2 NOT NULL
                    );
                    CREATE UNIQUE INDEX IX_Coupons_Code ON Coupons(Code);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS Coupons;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS SellerFollows;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS ListingQuestions;");
        }
    }
}
