using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPlatformService _platform;
        private readonly IPaymentService _payments;
        private readonly IPaymentWebhookService _webhooks;

        public PaymentsController(
            IPlatformService platform,
            IPaymentService payments,
            IPaymentWebhookService webhooks)
        {
            _platform = platform;
            _payments = payments;
            _webhooks = webhooks;
        }

        [HttpGet("packages")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> Packages([FromQuery] string? variant = null)
        {
            var experiment = await _platform.GetPackageExperimentAsync(variant);
            return Ok(ApiResponse<PackageExperimentDto>.Ok(experiment));
        }

        [HttpPost("packages/experiment-log")]
        [AllowAnonymous]
        public async Task<IActionResult> LogExperiment([FromBody] LogPackageExperimentDto dto)
        {
            int? userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
            await _platform.LogPackageExperimentAsync(userId, dto);
            return Ok(ApiResponse.Ok("Kaydedildi."));
        }

        [HttpPost("checkout")]
        [Authorize]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            var result = await _payments.CreateFeaturedCheckoutAsync(GetUserId(), dto);
            if (result == null)
                return BadRequest(ApiResponse.Fail("Ödeme başlatılamadı."));
            return Ok(ApiResponse<CheckoutResultDto>.Ok(result));
        }

        [HttpPost("complete/{purchaseId:int}")]
        [Authorize]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Complete(int purchaseId)
        {
            var ok = await _payments.CompleteCheckoutAsync(GetUserId(), purchaseId);
            if (!ok)
                return BadRequest(ApiResponse.Fail("Ödeme tamamlanamadı."));
            return Ok(ApiResponse.Ok("İlan öne çıkarıldı."));
        }

        [HttpPost("stripe-session")]
        [Authorize]
        public async Task<IActionResult> CompleteStripeSession([FromQuery] string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest(ApiResponse.Fail("Oturum kimliği gerekli."));
            var ok = await _payments.CompleteStripeSessionAsync(sessionId);
            if (!ok)
                return BadRequest(ApiResponse.Fail("Ödeme doğrulanamadı."));
            return Ok(ApiResponse.Ok("İlan öne çıkarıldı."));
        }

        [HttpPost("stripe-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];
            var ok = await _webhooks.ProcessStripeWebhookAsync(json, signature);
            return ok ? Ok() : BadRequest();
        }

        [HttpPost("iyzico-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> IyzicoCallback(
            [FromForm] string token,
            [FromQuery] string type,
            [FromQuery] int? orderId)
        {
            var frontend = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["App:FrontendUrl"] ?? "http://localhost:3000";

            if (string.IsNullOrWhiteSpace(token))
                return Redirect($"{frontend}/?payment=failed");

            var ok = await _webhooks.ProcessIyzicoCallbackAsync(token);
            if (!ok)
                return Redirect($"{frontend}/?payment=failed");

            if (string.Equals(type, "escrow", StringComparison.OrdinalIgnoreCase) && orderId.HasValue)
                return Redirect($"{frontend}/siparisler/{orderId.Value}?paid=1&provider=iyzico");

            return Redirect($"{frontend}/one-cikan/basarili?provider=iyzico");
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
