using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    public partial class AddMarketplaceOrderDispute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'DisputeReason')
                    ALTER TABLE MarketplaceOrders ADD DisputeReason NVARCHAR(2000) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'DisputedAt')
                    ALTER TABLE MarketplaceOrders ADD DisputedAt DATETIME2 NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'DisputedByUserId')
                    ALTER TABLE MarketplaceOrders ADD DisputedByUserId INT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'DisputeResolutionNote')
                    ALTER TABLE MarketplaceOrders ADD DisputeResolutionNote NVARCHAR(2000) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MarketplaceOrders') AND name = 'DisputeResolvedAt')
                    ALTER TABLE MarketplaceOrders ADD DisputeResolvedAt DATETIME2 NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
