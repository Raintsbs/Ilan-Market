namespace AdvertisementApp.Entities
{
    public class AdvertisementAuditLog
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int ActorUserId { get; set; }
        public string ActorEmail { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Details { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public Advertisement Advertisement { get; set; } = null!;
    }
}
