using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    /// <summary>
    /// Platform genişletme şeması — önceden DatabaseSeeder ham SQL ile uygulanıyordu.
    /// Production: Database:ApplyLegacySqlPatches=false ile yalnızca migration kullanın.
    /// </summary>
    public partial class ConsolidatePlatformSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'LastBumpedAt')
                    ALTER TABLE Advertisements ADD LastBumpedAt DATETIME2 NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Categories') AND name = 'Slug')
                    ALTER TABLE Categories ADD Slug NVARCHAR(128) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'IyzicoToken')
                    ALTER TABLE MarketplaceOrders ADD IyzicoToken NVARCHAR(256) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'IyzicoToken')
                    ALTER TABLE UserPurchases ADD IyzicoToken NVARCHAR(256) NULL;

                UPDATE Advertisements
                SET IsActive = 0, UpdatedTime = GETUTCDATE()
                WHERE Status = 0 AND IsActive = 1;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
