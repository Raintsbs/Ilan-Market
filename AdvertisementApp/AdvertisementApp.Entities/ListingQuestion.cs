namespace AdvertisementApp.Entities
{
    public class ListingQuestion
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int UserId { get; set; }
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public int? AnsweredByUserId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime? AnsweredTime { get; set; }
        public bool IsHidden { get; set; }
    }
}
