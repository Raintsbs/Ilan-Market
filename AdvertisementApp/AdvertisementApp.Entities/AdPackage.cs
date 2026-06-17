namespace AdvertisementApp.Entities
{
    public class AdPackage
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int FeaturedDays { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
