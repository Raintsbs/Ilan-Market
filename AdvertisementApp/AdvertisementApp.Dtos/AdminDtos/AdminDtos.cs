using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;

namespace AdvertisementApp.Dtos.AdminDtos
{
    public class AdminDashboardDto
    {
        public int TotalAds { get; set; }
        public int PendingAds { get; set; }
        public int TotalUsers { get; set; }
        public int OpenReports { get; set; }
        public int FeaturedAds { get; set; }
        public int ExpiredAds { get; set; }
        public List<CategoryAdCountDto> CategoryDistribution { get; set; } = new();
    }

    public class CategoryAdCountDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int Count { get; set; }
    }

    public class AdminBulkActionDto
    {
        public List<int> Ids { get; set; } = new();
        public string Action { get; set; } = null!;
        public string? Reason { get; set; }
        public int? ExtendDays { get; set; }
    }

    public class AdminAdActionDto
    {
        public string? Reason { get; set; }
        public string? AdminNote { get; set; }
        public int? ExtendDays { get; set; }
        public bool? IsFeatured { get; set; }
        public int? FeaturedDays { get; set; }
    }

    public class AuditLogDto
    {
        public int Id { get; set; }
        public string ActorEmail { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Details { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class ReportAdminDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string AdTitle { get; set; } = null!;
        public int ReporterUserId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Details { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
    }

    public class ResolveReportDto
    {
        public string Action { get; set; } = null!;
        public string? Note { get; set; }
        public bool RejectAd { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsVerified { get; set; }
        public bool IsBanned { get; set; }
        public bool IsFrozen { get; set; }
        public int WarningCount { get; set; }
        public bool PhoneVerified { get; set; }
        public string? PhoneNumber { get; set; }
        public int AdCount { get; set; }
        public DateTime? CreatedTime { get; set; }
    }

    public class AdminUserDetailDto : AdminUserDto
    {
        public string? BanReason { get; set; }
        public DateTime? FrozenUntil { get; set; }
        public List<AdvertisementListDto> RecentAds { get; set; } = new();
        public List<ActivityLogDto> Activity { get; set; } = new();
    }

    public class ActivityLogDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
    }

    public class ModerateUserDto
    {
        public bool? IsBanned { get; set; }
        public bool? IsFrozen { get; set; }
        public string? BanReason { get; set; }
        public int? FrozenDays { get; set; }
        public bool? PhoneVerified { get; set; }
        public bool? IsVerified { get; set; }
        public bool AddWarning { get; set; }
        public string? Role { get; set; }
    }

    public class AdminCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? Slug { get; set; }
        public int SortOrder { get; set; }
        public string? FieldSchemaJson { get; set; }
        public bool IsActive { get; set; }
        public int AdCount { get; set; }
        public int ChildCount { get; set; }
    }

    public class StaticPageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class CityLocationDto
    {
        public int Id { get; set; }
        public string City { get; set; } = null!;
        public string? District { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class BlogPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Summary { get; set; }
        public string Content { get; set; } = null!;
        public bool IsPublished { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
