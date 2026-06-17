namespace AdvertisementApp.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Slug { get; set; }
        public string? FieldSchemaJson { get; set; }

        public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    }
}
