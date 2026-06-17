using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.DataAccess.Context
{
    public class AdvertisementAppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<MessageThread> MessageThreads { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<ListingReport> ListingReports { get; set; }
        public DbSet<SavedSearch> SavedSearches { get; set; }
        public DbSet<AppNotification> Notifications { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<AdPackage> AdPackages { get; set; }
        public DbSet<UserPurchase> UserPurchases { get; set; }
        public DbSet<AdvertisementAuditLog> AdvertisementAuditLogs { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<StaticPage> StaticPages { get; set; }
        public DbSet<CityLocation> CityLocations { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<PhoneRevealLog> PhoneRevealLogs { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<MarketplaceOrder> MarketplaceOrders { get; set; }
        public DbSet<OrderShipment> OrderShipments { get; set; }
        public DbSet<SellerReview> SellerReviews { get; set; }
        public DbSet<AdvertisementReview> AdvertisementReviews { get; set; }
        public DbSet<BuyerReview> BuyerReviews { get; set; }
        public DbSet<PhoneVerificationCode> PhoneVerificationCodes { get; set; }
        public DbSet<AdvertisementPriceHistory> AdvertisementPriceHistories { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<AuctionBid> AuctionBids { get; set; }
        public DbSet<WebPushSubscription> WebPushSubscriptions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PaymentWebhookEvent> PaymentWebhookEvents { get; set; }
        public DbSet<VerificationRequest> VerificationRequests { get; set; }
        public DbSet<ListingQuestion> ListingQuestions { get; set; }
        public DbSet<SellerFollow> SellerFollows { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        public AdvertisementAppDbContext(DbContextOptions<AdvertisementAppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }

            builder.ApplyConfigurationsFromAssembly(typeof(AdvertisementAppDbContext).Assembly);

            // Favori: aynı kullanıcı aynı ilanı bir kez favoriye ekleyebilir
            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.AdvertisementId })
                .IsUnique();

            builder.Entity<Favorite>()
                .HasOne(f => f.Advertisement)
                .WithMany()
                .HasForeignKey(f => f.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MessageThread>()
                .HasIndex(t => new { t.AdvertisementId, t.BuyerUserId })
                .IsUnique();

            builder.Entity<Message>()
                .HasOne(m => m.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Offer>()
                .HasIndex(o => new { o.AdvertisementId, o.BuyerUserId, o.Status });

            builder.Entity<ListingReport>()
                .HasIndex(r => new { r.AdvertisementId, r.ReporterUserId });

            builder.Entity<SavedSearch>()
                .HasIndex(s => s.UserId);

            builder.Entity<WebPushSubscription>()
                .HasIndex(w => new { w.UserId, w.Endpoint })
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(c => c.ParentId);

            builder.Entity<District>()
                .HasOne(d => d.Province)
                .WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Neighborhood>()
                .HasOne(n => n.District)
                .WithMany(d => d.Neighborhoods)
                .HasForeignKey(n => n.DistrictId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MarketplaceOrder>()
                .HasOne(o => o.Advertisement)
                .WithMany()
                .HasForeignKey(o => o.AdvertisementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderShipment>()
                .HasOne(s => s.Order)
                .WithOne(o => o.Shipment)
                .HasForeignKey<OrderShipment>(s => s.MarketplaceOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SellerReview>()
                .HasIndex(r => r.SellerUserId);

            builder.Entity<SellerReview>()
                .HasIndex(r => r.MarketplaceOrderId)
                .IsUnique()
                .HasFilter("[MarketplaceOrderId] IS NOT NULL");

            builder.Entity<AdvertisementReview>()
                .HasIndex(r => r.AdvertisementId);

            builder.Entity<AdvertisementReview>()
                .HasIndex(r => r.MarketplaceOrderId)
                .IsUnique()
                .HasFilter("[MarketplaceOrderId] IS NOT NULL");

            builder.Entity<BuyerReview>()
                .HasIndex(r => r.BuyerUserId);

            builder.Entity<BuyerReview>()
                .HasIndex(r => r.MarketplaceOrderId)
                .IsUnique()
                .HasFilter("[MarketplaceOrderId] IS NOT NULL");

            builder.Entity<PhoneVerificationCode>()
                .HasIndex(c => new { c.UserId, c.Used });

            builder.Entity<AdvertisementPriceHistory>()
                .HasIndex(h => new { h.AdvertisementId, h.RecordedAt });

            builder.Entity<Auction>()
                .HasOne(a => a.Advertisement)
                .WithOne(ad => ad.Auction)
                .HasForeignKey<Auction>(a => a.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AuctionBid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AuctionBid>()
                .HasIndex(b => new { b.AuctionId, b.CreatedTime });

            builder.Entity<ListingQuestion>()
                .HasIndex(q => q.AdvertisementId);

            builder.Entity<SellerFollow>()
                .HasIndex(f => new { f.FollowerUserId, f.SellerUserId })
                .IsUnique();

            builder.Entity<SellerFollow>()
                .HasIndex(f => f.SellerUserId);

            builder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();
        }
    }
}
