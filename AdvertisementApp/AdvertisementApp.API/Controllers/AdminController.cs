using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Admin;
using AdvertisementApp.Dtos.AdminDtos;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.AdminOrModerator)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;
        private readonly IMarketplaceOrderService _orders;
        private readonly IReviewService _reviews;

        public AdminController(IAdminService admin, IMarketplaceOrderService orders, IReviewService reviews)
        {
            _admin = admin;
            _orders = orders;
            _reviews = reviews;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard() =>
            Ok(ApiResponse<AdminDashboardDto>.Ok(await _admin.GetDashboardAsync()));

        [HttpGet("advertisements")]
        public async Task<IActionResult> Ads([FromQuery] AdvertisementFilterDto filter)
        {
            var data = await _admin.GetAdsAsync(filter);
            return Ok(ApiResponse<PagedResult<AdvertisementListDto>>.Ok(data));
        }

        [HttpPost("advertisements/{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var ok = await _admin.ApproveAdAsync(id, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Onaylandı.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpPost("advertisements/{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] AdminAdActionDto? dto)
        {
            var ok = await _admin.RejectAdAsync(id, GetUserId(), GetEmail(), dto?.Reason);
            return ok ? Ok(ApiResponse.Ok("Reddedildi.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpDelete("advertisements/{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ok = await _admin.DeleteAdAsync(id, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpPost("advertisements/{id:int}/extend")]
        public async Task<IActionResult> Extend(int id, [FromBody] AdminAdActionDto dto)
        {
            var days = dto.ExtendDays ?? 30;
            var ok = await _admin.ExtendAdAsync(id, days, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Süre uzatıldı.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpPost("advertisements/{id:int}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            var ok = await _admin.ArchiveAdAsync(id, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Arşivlendi.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpPost("advertisements/{id:int}/featured")]
        public async Task<IActionResult> Featured(int id, [FromBody] AdminAdActionDto dto)
        {
            var ok = await _admin.SetFeaturedAsync(id, dto.IsFeatured ?? true, dto.FeaturedDays ?? 7, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Güncellendi.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpPost("advertisements/bulk")]
        public async Task<IActionResult> Bulk([FromBody] AdminBulkActionDto dto)
        {
            var count = await _admin.BulkAdsAsync(dto, GetUserId(), GetEmail());
            return Ok(ApiResponse<int>.Ok(count, $"{count} ilan işlendi."));
        }

        [HttpGet("advertisements/{id:int}/history")]
        public async Task<IActionResult> History(int id)
        {
            var items = await _admin.GetAdHistoryAsync(id);
            return Ok(ApiResponse<List<AuditLogDto>>.Ok(items));
        }

        [HttpGet("reports")]
        public async Task<IActionResult> Reports([FromQuery] string? status = "open")
        {
            var items = await _admin.GetReportsAsync(status);
            return Ok(ApiResponse<List<ReportAdminDto>>.Ok(items));
        }

        [HttpPost("reports/{id:int}/resolve")]
        public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportDto dto)
        {
            var ok = await _admin.ResolveReportAsync(id, dto, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Şikayet kapatıldı.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users([FromQuery] string? search, [FromQuery] string? role)
        {
            var items = await _admin.GetUsersAsync(search, role);
            return Ok(ApiResponse<List<AdminUserDto>>.Ok(items));
        }

        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> UserDetail(int id)
        {
            var u = await _admin.GetUserDetailAsync(id);
            if (u == null) return NotFound(ApiResponse.Fail("Kullanıcı bulunamadı."));
            return Ok(ApiResponse<AdminUserDetailDto>.Ok(u));
        }

        [HttpPut("users/{id:int}/moderate")]
        [Authorize(Roles = AppRoles.AdminOrModerator)]
        public async Task<IActionResult> ModerateUser(int id, [FromBody] ModerateUserDto dto)
        {
            var ok = await _admin.ModerateUserAsync(id, dto, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Kullanıcı güncellendi.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> Categories()
        {
            var items = await _admin.GetAdminCategoriesAsync();
            return Ok(ApiResponse<List<AdminCategoryDto>>.Ok(items));
        }

        [HttpPost("categories")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> CreateCategory([FromBody] AdminCategoryDto dto)
        {
            var ok = await _admin.SaveCategoryAsync(dto, null, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Kaydedildi.")) : BadRequest(ApiResponse.Fail("Kaydedilemedi."));
        }

        [HttpPut("categories/{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminCategoryDto dto)
        {
            var ok = await _admin.SaveCategoryAsync(dto, id, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Güncellendi.")) : BadRequest(ApiResponse.Fail("Güncellenemedi."));
        }

        [HttpDelete("categories/{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var ok = await _admin.DeleteCategoryAsync(id, GetUserId(), GetEmail());
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpGet("pages")]
        public async Task<IActionResult> Pages()
        {
            var items = await _admin.GetStaticPagesAsync();
            return Ok(ApiResponse<List<StaticPageDto>>.Ok(items));
        }

        [HttpPut("pages")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> SavePage([FromBody] StaticPageDto dto)
        {
            var ok = await _admin.SaveStaticPageAsync(dto);
            return ok ? Ok(ApiResponse.Ok("Kaydedildi.")) : BadRequest(ApiResponse.Fail("Kaydedilemedi."));
        }

        [HttpGet("cities")]
        public async Task<IActionResult> Cities()
        {
            var items = await _admin.GetCitiesAsync();
            return Ok(ApiResponse<List<CityLocationDto>>.Ok(items));
        }

        [HttpPost("cities")]
        public async Task<IActionResult> CreateCity([FromBody] CityLocationDto dto)
        {
            var ok = await _admin.SaveCityAsync(dto, null);
            return ok ? Ok(ApiResponse.Ok("Eklendi.")) : BadRequest(ApiResponse.Fail("Eklenemedi."));
        }

        [HttpPut("cities/{id:int}")]
        public async Task<IActionResult> UpdateCity(int id, [FromBody] CityLocationDto dto)
        {
            var ok = await _admin.SaveCityAsync(dto, id);
            return ok ? Ok(ApiResponse.Ok("Güncellendi.")) : BadRequest(ApiResponse.Fail("Güncellenemedi."));
        }

        [HttpDelete("cities/{id:int}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var ok = await _admin.DeleteCityAsync(id);
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpGet("blog")]
        public async Task<IActionResult> Blog()
        {
            var items = await _admin.GetBlogPostsAsync();
            return Ok(ApiResponse<List<BlogPostDto>>.Ok(items));
        }

        [HttpPost("blog")]
        public async Task<IActionResult> CreateBlog([FromBody] BlogPostDto dto)
        {
            var ok = await _admin.SaveBlogPostAsync(dto, null);
            return ok ? Ok(ApiResponse.Ok("Kaydedildi.")) : BadRequest(ApiResponse.Fail("Kaydedilemedi."));
        }

        [HttpPut("blog/{id:int}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogPostDto dto)
        {
            var ok = await _admin.SaveBlogPostAsync(dto, id);
            return ok ? Ok(ApiResponse.Ok("Güncellendi.")) : BadRequest(ApiResponse.Fail("Güncellenemedi."));
        }

        [HttpDelete("blog/{id:int}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var ok = await _admin.DeleteBlogPostAsync(id);
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpGet("marketplace-orders/disputed")]
        public async Task<IActionResult> DisputedOrders() =>
            Ok(ApiResponse<List<MarketplaceOrderDto>>.Ok(await _orders.GetDisputedOrdersAsync()));

        [HttpGet("marketplace-orders")]
        public async Task<IActionResult> MarketplaceOrders([FromQuery] AdminMarketplaceOrderFilterDto filter) =>
            Ok(ApiResponse<PagedResult<MarketplaceOrderDto>>.Ok(await _orders.GetAdminOrdersAsync(filter)));

        [HttpPost("marketplace-orders/{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orders.CancelOrderAsync(GetUserId(), id, asAdmin: true);
            if (order == null) return BadRequest(ApiResponse.Fail("Sipariş iptal edilemedi."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "Sipariş iptal edildi."));
        }

        [HttpPost("marketplace-orders/{id:int}/refund")]
        public async Task<IActionResult> MarkRefund(int id, [FromBody] AdminOrderNoteDto dto)
        {
            var order = await _orders.MarkRefundedAsync(id, dto.Note);
            if (order == null) return BadRequest(ApiResponse.Fail("İade kaydedilemedi."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "İade işaretlendi."));
        }

        [HttpPost("marketplace-orders/{id:int}/seller-payout")]
        public async Task<IActionResult> MarkSellerPayout(int id, [FromBody] AdminOrderNoteDto dto)
        {
            var order = await _orders.MarkSellerPayoutAsync(id, dto.Note);
            if (order == null) return BadRequest(ApiResponse.Fail("Satıcı ödemesi kaydedilemedi."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "Satıcı ödemesi işaretlendi."));
        }

        [HttpPost("marketplace-orders/{id:int}/resolve-dispute")]
        public async Task<IActionResult> ResolveDispute(int id, [FromBody] ResolveDisputeDto dto)
        {
            var order = await _orders.ResolveDisputeAsync(id, dto, GetUserId());
            if (order == null) return BadRequest(ApiResponse.Fail("İtiraz çözülemedi."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "İtiraz sonuçlandırıldı."));
        }

        [HttpGet("ad-packages")]
        public async Task<IActionResult> AdPackages() =>
            Ok(ApiResponse<List<AdminAdPackageDto>>.Ok(await _admin.GetAdPackagesAsync()));

        [HttpPost("ad-packages")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> CreateAdPackage([FromBody] AdminAdPackageDto dto)
        {
            var ok = await _admin.SaveAdPackageAsync(dto, null);
            return ok ? Ok(ApiResponse.Ok("Paket oluşturuldu.")) : BadRequest(ApiResponse.Fail("Paket kaydedilemedi."));
        }

        [HttpPut("ad-packages/{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> UpdateAdPackage(int id, [FromBody] AdminAdPackageDto dto)
        {
            var ok = await _admin.SaveAdPackageAsync(dto, id);
            return ok ? Ok(ApiResponse.Ok("Paket güncellendi.")) : BadRequest(ApiResponse.Fail("Paket güncellenemedi."));
        }

        [HttpDelete("ad-packages/{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> DeleteAdPackage(int id)
        {
            var ok = await _admin.DeleteAdPackageAsync(id);
            return ok ? Ok(ApiResponse.Ok("Paket pasifleştirildi.")) : BadRequest(ApiResponse.Fail("Paket silinemedi."));
        }

        [HttpPost("advertisements/import")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> ImportAds([FromBody] string csv)
        {
            var result = await _admin.ImportAdvertisementsCsvAsync(csv, GetUserId());
            return Ok(ApiResponse<BulkImportResultDto>.Ok(result, $"{result.Created} ilan içe aktarıldı."));
        }

        [HttpGet("verifications/pending")]
        public async Task<IActionResult> PendingVerifications([FromServices] IPlatformService platform) =>
            Ok(ApiResponse<List<VerificationRequestDto>>.Ok(await platform.GetPendingVerificationsAsync()));

        [HttpPost("verifications/{id:int}/review")]
        public async Task<IActionResult> ReviewVerification(
            int id,
            [FromBody] ReviewVerificationDto dto,
            [FromServices] IPlatformService platform)
        {
            var ok = await platform.ReviewVerificationAsync(id, dto, GetUserId());
            return ok ? Ok(ApiResponse.Ok("İşlem tamamlandı.")) : BadRequest(ApiResponse.Fail("İşlem yapılamadı."));
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews([FromQuery] string? type = null, [FromQuery] int take = 50) =>
            Ok(ApiResponse<List<AdminReviewItemDto>>.Ok(await _reviews.GetAdminReviewsAsync(type, take)));

        [HttpPost("reviews/{reviewType}/{id:int}/hide")]
        public async Task<IActionResult> HideReview(string reviewType, int id, [FromQuery] bool hidden = true)
        {
            var ok = await _reviews.SetReviewHiddenAsync(reviewType, id, hidden);
            return ok ? Ok(ApiResponse.Ok(hidden ? "Gizlendi." : "Gösterildi.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpDelete("reviews/{reviewType}/{id:int}")]
        public async Task<IActionResult> DeleteReview(string reviewType, int id)
        {
            var ok = await _reviews.DeleteReviewAdminAsync(reviewType, id);
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpGet("coupons")]
        public async Task<IActionResult> Coupons([FromServices] IGrowthService growth) =>
            Ok(ApiResponse<List<CouponDto>>.Ok(await growth.GetCouponsAdminAsync()));

        [HttpPost("coupons")]
        public async Task<IActionResult> SaveCoupon([FromBody] CouponDto dto, [FromServices] IGrowthService growth)
        {
            var ok = await growth.SaveCouponAsync(dto, null);
            return ok ? Ok(ApiResponse.Ok("Kupon kaydedildi.")) : BadRequest(ApiResponse.Fail("Kaydedilemedi."));
        }

        [HttpPut("coupons/{id:int}")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] CouponDto dto, [FromServices] IGrowthService growth)
        {
            var ok = await growth.SaveCouponAsync(dto, id);
            return ok ? Ok(ApiResponse.Ok("Güncellendi.")) : BadRequest(ApiResponse.Fail("Güncellenemedi."));
        }

        [HttpDelete("coupons/{id:int}")]
        public async Task<IActionResult> DeleteCoupon(int id, [FromServices] IGrowthService growth)
        {
            var ok = await growth.DeleteCouponAsync(id);
            return ok ? Ok(ApiResponse.Ok("Pasifleştirildi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetEmail() => User.FindFirstValue(ClaimTypes.Email) ?? "admin";
    }
}
