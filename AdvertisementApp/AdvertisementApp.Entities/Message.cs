namespace AdvertisementApp.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int SenderUserId { get; set; }
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public MessageThread Thread { get; set; } = null!;
    }
}
