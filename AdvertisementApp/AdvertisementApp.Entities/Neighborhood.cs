namespace AdvertisementApp.Entities
{
    public class Neighborhood
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }
        public string Name { get; set; } = null!;
        public District District { get; set; } = null!;
    }
}
