namespace AdvertisementApp.Dtos.CategoryDtos
{
    public class CategoryListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Slug { get; set; }
        public string? FieldSchemaJson { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
