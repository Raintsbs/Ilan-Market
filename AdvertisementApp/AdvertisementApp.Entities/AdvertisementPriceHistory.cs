namespace AdvertisementApp.Entities
{
    public class AdvertisementPriceHistory
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public decimal Price { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public Advertisement Advertisement { get; set; } = null!;
    }
}
