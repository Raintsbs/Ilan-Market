namespace AdvertisementApp.Dtos.CategoryDtos
{
    public class CategoryCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? FieldSchemaJson { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
