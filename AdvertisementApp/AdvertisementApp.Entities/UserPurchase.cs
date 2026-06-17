namespace AdvertisementApp.Entities
{
    public class UserPurchase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? AdvertisementId { get; set; }
        public int AdPackageId { get; set; }
        public string Status { get; set; } = "pending";
        public string? StripeSessionId { get; set; }
        public string? IyzicoToken { get; set; }
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public AdPackage AdPackage { get; set; } = null!;
    }
}
