using Microsoft.AspNetCore.Identity;

namespace AdvertisementApp.DataAccess.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfileImagePath { get; set; }
        public bool IsVerified { get; set; }
        public string? OAuthProvider { get; set; }
        public string? OAuthSubject { get; set; }
        public bool IsBanned { get; set; }
        public bool IsFrozen { get; set; }
        public int WarningCount { get; set; }
        public bool PhoneVerified { get; set; }
        public string? BanReason { get; set; }
        public DateTime? FrozenUntil { get; set; }
        public string? ReferralCode { get; set; }
        public int? ReferredByUserId { get; set; }
        public string? StoreSlug { get; set; }
        public string? CompanyName { get; set; }
        public string? StoreDescription { get; set; }
        public string? StoreBannerPath { get; set; }
        public bool IsCorporateStore { get; set; }
    }
}
