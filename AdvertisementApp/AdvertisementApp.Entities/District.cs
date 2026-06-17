namespace AdvertisementApp.Entities
{
    public class District
    {
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public string Name { get; set; } = null!;
        public Province Province { get; set; } = null!;
        public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }
}
