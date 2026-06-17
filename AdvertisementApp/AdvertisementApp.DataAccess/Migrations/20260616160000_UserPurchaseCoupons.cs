using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260616160000_UserPurchaseCoupons")]
    public partial class UserPurchaseCoupons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'CouponCode')
                    ALTER TABLE UserPurchases ADD CouponCode NVARCHAR(32) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'DiscountAmount')
                    ALTER TABLE UserPurchases ADD DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_UserPurchases_DiscountAmount DEFAULT 0;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'PaidAmount')
                    ALTER TABLE UserPurchases ADD PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_UserPurchases_PaidAmount DEFAULT 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'PaidAmount')
                    ALTER TABLE UserPurchases DROP CONSTRAINT DF_UserPurchases_PaidAmount;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'PaidAmount')
                    ALTER TABLE UserPurchases DROP COLUMN PaidAmount;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'DiscountAmount')
                    ALTER TABLE UserPurchases DROP CONSTRAINT DF_UserPurchases_DiscountAmount;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'DiscountAmount')
                    ALTER TABLE UserPurchases DROP COLUMN DiscountAmount;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UserPurchases') AND name = 'CouponCode')
                    ALTER TABLE UserPurchases DROP COLUMN CouponCode;
                """);
        }
    }
}
