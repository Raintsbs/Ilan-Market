namespace AdvertisementApp.Entities
{
    public class PaymentWebhookEvent
    {
        public int Id { get; set; }
        public string Provider { get; set; } = null!;
        public string EventId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string? PayloadJson { get; set; }
        /// <summary>pending | processed | failed | dead</summary>
        public string Status { get; set; } = "pending";
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
