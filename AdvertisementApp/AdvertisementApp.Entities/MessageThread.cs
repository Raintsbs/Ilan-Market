namespace AdvertisementApp.Entities
{
    public class MessageThread
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedTime { get; set; }

        public Advertisement Advertisement { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
