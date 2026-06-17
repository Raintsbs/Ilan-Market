namespace AdvertisementApp.Entities
{
    public class Province
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? PlateCode { get; set; }
        public int SortOrder { get; set; }
        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
