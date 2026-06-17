namespace AdvertisementApp.Entities
{
    public class VerificationRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        /// <summary>identity | trade_registry | other</summary>
        public string DocumentType { get; set; } = "identity";
        public string FilePath { get; set; } = null!;
        /// <summary>pending | approved | rejected</summary>
        public string Status { get; set; } = "pending";
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
    }
}
