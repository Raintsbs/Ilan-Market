namespace AdvertisementApp.Dtos.Marketplace
{
    public class SavedSearchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string FilterJson { get; set; } = "{}";
        public bool NotifyOnNew { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class CreateSavedSearchDto
    {
        public string Name { get; set; } = null!;
        public string FilterJson { get; set; } = "{}";
        public bool NotifyOnNew { get; set; }
    }
}
