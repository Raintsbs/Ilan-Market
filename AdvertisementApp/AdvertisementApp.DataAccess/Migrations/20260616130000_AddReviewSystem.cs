using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260616130000_AddReviewSystem")]
    public partial class AddReviewSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'SellerReviews', N'U') IS NULL
                BEGIN
                    CREATE TABLE SellerReviews (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        SellerUserId INT NOT NULL,
                        BuyerUserId INT NOT NULL,
                        MarketplaceOrderId INT NULL,
                        Rating INT NOT NULL,
                        Comment NVARCHAR(2000) NULL,
                        IsHidden BIT NOT NULL CONSTRAINT DF_SellerReviews_IsHidden DEFAULT 0,
                        CreatedTime DATETIME2 NOT NULL,
                        UpdatedTime DATETIME2 NULL
                    );
                    CREATE INDEX IX_SellerReviews_SellerUserId ON SellerReviews(SellerUserId);
                    CREATE UNIQUE INDEX IX_SellerReviews_MarketplaceOrderId ON SellerReviews(MarketplaceOrderId) WHERE MarketplaceOrderId IS NOT NULL;
                END
                ELSE
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SellerReviews') AND name = 'IsHidden')
                        ALTER TABLE SellerReviews ADD IsHidden BIT NOT NULL CONSTRAINT DF_SellerReviews_IsHidden2 DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'SellerReviews') AND name = 'UpdatedTime')
                        ALTER TABLE SellerReviews ADD UpdatedTime DATETIME2 NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'AdvertisementReviews', N'U') IS NULL
                BEGIN
                    CREATE TABLE AdvertisementReviews (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        AdvertisementId INT NOT NULL,
                        UserId INT NOT NULL,
                        MarketplaceOrderId INT NULL,
                        Rating INT NOT NULL,
                        Comment NVARCHAR(2000) NULL,
                        IsHidden BIT NOT NULL CONSTRAINT DF_AdvertisementReviews_IsHidden DEFAULT 0,
                        CreatedTime DATETIME2 NOT NULL,
                        UpdatedTime DATETIME2 NULL
                    );
                    CREATE INDEX IX_AdvertisementReviews_AdvertisementId ON AdvertisementReviews(AdvertisementId);
                    CREATE UNIQUE INDEX IX_AdvertisementReviews_MarketplaceOrderId ON AdvertisementReviews(MarketplaceOrderId) WHERE MarketplaceOrderId IS NOT NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'BuyerReviews', N'U') IS NULL
                BEGIN
                    CREATE TABLE BuyerReviews (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        BuyerUserId INT NOT NULL,
                        SellerUserId INT NOT NULL,
                        MarketplaceOrderId INT NULL,
                        Rating INT NOT NULL,
                        Comment NVARCHAR(2000) NULL,
                        IsHidden BIT NOT NULL CONSTRAINT DF_BuyerReviews_IsHidden DEFAULT 0,
                        CreatedTime DATETIME2 NOT NULL
                    );
                    CREATE INDEX IX_BuyerReviews_BuyerUserId ON BuyerReviews(BuyerUserId);
                    CREATE UNIQUE INDEX IX_BuyerReviews_MarketplaceOrderId ON BuyerReviews(MarketplaceOrderId) WHERE MarketplaceOrderId IS NOT NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS BuyerReviews;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS AdvertisementReviews;");
        }
    }
}
