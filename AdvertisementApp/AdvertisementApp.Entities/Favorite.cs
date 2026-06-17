namespace AdvertisementApp.Entities
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AdvertisementId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public bool PriceAlertEnabled { get; set; }
        public decimal? AlertPrice { get; set; }
        public decimal? LastKnownPrice { get; set; }

        public Advertisement Advertisement { get; set; } = null!;
    }
}
