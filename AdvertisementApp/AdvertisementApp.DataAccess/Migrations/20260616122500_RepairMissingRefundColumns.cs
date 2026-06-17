using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260616122500_RepairMissingRefundColumns")]
    public partial class RepairMissingRefundColumns : Migration
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
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
