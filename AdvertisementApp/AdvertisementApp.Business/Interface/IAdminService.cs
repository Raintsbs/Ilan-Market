using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.AdminDtos;
using AdvertisementApp.Dtos.Admin;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Dtos.CategoryDtos;

namespace AdvertisementApp.Business.Interface
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<PagedResult<AdvertisementListDto>> GetAdsAsync(AdvertisementFilterDto filter);
        Task<bool> ApproveAdAsync(int id, int actorId, string actorEmail);
        Task<bool> RejectAdAsync(int id, int actorId, string actorEmail, string? reason);
        Task<bool> DeleteAdAsync(int id, int actorId, string actorEmail);
        Task<bool> ExtendAdAsync(int id, int days, int actorId, string actorEmail);
        Task<bool> ArchiveAdAsync(int id, int actorId, string actorEmail);
        Task<bool> SetFeaturedAsync(int id, bool featured, int? days, int actorId, string actorEmail);
        Task<int> BulkAdsAsync(AdminBulkActionDto dto, int actorId, string actorEmail);
        Task<List<AuditLogDto>> GetAdHistoryAsync(int adId);
        Task<List<ReportAdminDto>> GetReportsAsync(string? status = "open");
        Task<bool> ResolveReportAsync(int reportId, ResolveReportDto dto, int actorId, string actorEmail);
        Task<List<AdminUserDto>> GetUsersAsync(string? search, string? role);
        Task<AdminUserDetailDto?> GetUserDetailAsync(int userId);
        Task<bool> ModerateUserAsync(int userId, ModerateUserDto dto, int actorId, string actorEmail);
        Task LogActivityAsync(int? userId, string type, string message, string? ip = null);
        Task<List<AdminCategoryDto>> GetAdminCategoriesAsync();
        Task<bool> SaveCategoryAsync(AdminCategoryDto dto, int? id, int actorId, string actorEmail);
        Task<bool> DeleteCategoryAsync(int id, int actorId, string actorEmail);
        Task<List<StaticPageDto>> GetStaticPagesAsync();
        Task<bool> SaveStaticPageAsync(StaticPageDto dto);
        Task<List<CityLocationDto>> GetCitiesAsync();
        Task<bool> SaveCityAsync(CityLocationDto dto, int? id);
        Task<bool> DeleteCityAsync(int id);
        Task<List<BlogPostDto>> GetBlogPostsAsync();
        Task<bool> SaveBlogPostAsync(BlogPostDto dto, int? id);
        Task<bool> DeleteBlogPostAsync(int id);
        Task<List<AdminAdPackageDto>> GetAdPackagesAsync();
        Task<bool> SaveAdPackageAsync(AdminAdPackageDto dto, int? id);
        Task<bool> DeleteAdPackageAsync(int id);
        Task<BulkImportResultDto> ImportAdvertisementsCsvAsync(string csv, int actorUserId);
    }
}
