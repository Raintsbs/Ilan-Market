namespace AdvertisementApp.Entities
{
    public class PhoneRevealLog
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int ViewerUserId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
