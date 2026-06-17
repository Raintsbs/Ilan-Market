namespace AdvertisementApp.Entities
{
    public class PhoneVerificationCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
