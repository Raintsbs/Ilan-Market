using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260616120500_RepairMissingRefreshTokens")]
    public partial class RepairMissingRefreshTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
