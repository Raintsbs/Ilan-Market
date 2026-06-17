namespace AdvertisementApp.Entities
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public int MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
