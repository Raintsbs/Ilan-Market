namespace AdvertisementApp.Entities
{
    public class CityLocation
    {
        public int Id { get; set; }
        public string City { get; set; } = null!;
        public string? District { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
