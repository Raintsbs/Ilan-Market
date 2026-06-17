namespace AdvertisementApp.Entities
{
    public class SellerFollow
    {
        public int Id { get; set; }
        public int FollowerUserId { get; set; }
        public int SellerUserId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
