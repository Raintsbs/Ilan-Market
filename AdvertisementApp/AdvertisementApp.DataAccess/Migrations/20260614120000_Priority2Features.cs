using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    public partial class Priority2Features : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'RefundedAt')
                    ALTER TABLE MarketplaceOrders ADD RefundedAt DATETIME2 NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'RefundNote')
                    ALTER TABLE MarketplaceOrders ADD RefundNote NVARCHAR(2000) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'SellerPaidOutAt')
                    ALTER TABLE MarketplaceOrders ADD SellerPaidOutAt DATETIME2 NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'SellerPayoutNote')
                    ALTER TABLE MarketplaceOrders ADD SellerPayoutNote NVARCHAR(2000) NULL;

                IF OBJECT_ID(N'RefreshTokens', N'U') IS NULL
                BEGIN
                    CREATE TABLE RefreshTokens (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        TokenHash NVARCHAR(128) NOT NULL,
                        ExpiresAt DATETIME2 NOT NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        RevokedAt DATETIME2 NULL,
                        ReplacedByTokenHash NVARCHAR(128) NULL
                    );
                    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
                    CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash ON RefreshTokens(TokenHash);
                END

                IF OBJECT_ID(N'PaymentWebhookEvents', N'U') IS NULL
                BEGIN
                    CREATE TABLE PaymentWebhookEvents (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Provider NVARCHAR(32) NOT NULL,
                        EventId NVARCHAR(256) NOT NULL,
                        EventType NVARCHAR(128) NOT NULL,
                        PayloadJson NVARCHAR(MAX) NULL,
                        Status NVARCHAR(32) NOT NULL,
                        RetryCount INT NOT NULL DEFAULT 0,
                        LastError NVARCHAR(2000) NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        ProcessedAt DATETIME2 NULL
                    );
                    CREATE UNIQUE INDEX IX_PaymentWebhookEvents_Provider_EventId ON PaymentWebhookEvents(Provider, EventId);
                    CREATE INDEX IX_PaymentWebhookEvents_Status ON PaymentWebhookEvents(Status);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
