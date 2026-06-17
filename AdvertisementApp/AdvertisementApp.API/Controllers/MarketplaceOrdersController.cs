using AdvertisementApp.Business.Configuration;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/marketplace/orders")]
    [Authorize]
    [EnableRateLimiting("write")]
    public class MarketplaceOrdersController : ControllerBase
    {
        private readonly IMarketplaceOrderService _orders;
        private readonly IConfiguration _config;

        public MarketplaceOrdersController(IMarketplaceOrderService orders, IConfiguration config)
        {
            _orders = orders;
            _config = config;
        }

        [HttpGet("carriers")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> Carriers() =>
            Ok(ApiResponse<List<CargoCarrierDto>>.Ok(await _orders.GetCarriersAsync()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMarketplaceOrderDto dto)
        {
            var order = await _orders.CreateOrderAsync(GetUserId(), dto);
            if (order == null) return BadRequest(ApiResponse.Fail("Sipariş oluşturulamadı."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "Sipariş oluşturuldu."));
        }

        [HttpPost("{id:int}/pay")]
        public async Task<IActionResult> Pay(int id, [FromBody] PayMarketplaceOrderDto? dto)
        {
            var result = await _orders.PayOrderAsync(GetUserId(), id, dto);
            if (result == null)
            {
                var hint = PaymentOptionsHelper.IsRealPaymentConfigured(_config)
                    ? "Ödeme sayfası açılamadı. iyzico/Stripe yapılandırmasını ve API loglarını kontrol edin."
                    : "Ödeme sağlayıcısı yapılandırılmamış. Development.local.json içinde Iyzico anahtarlarını ekleyin.";
                return BadRequest(ApiResponse.Fail(hint));
            }
            var msg = result.Message ?? (result.IsDemo ? "Demo ödeme tamamlandı." : "Ödeme sayfasına yönlendiriliyorsunuz.");
            return Ok(ApiResponse<PayMarketplaceOrderResultDto>.Ok(result, msg));
        }

        [HttpPost("{id:int}/ship")]
        public async Task<IActionResult> Ship(int id, [FromBody] ShipOrderDto dto)
        {
            var order = await _orders.ShipOrderAsync(GetUserId(), id, dto);
            if (order == null) return BadRequest(ApiResponse.Fail("Kargo bilgisi kaydedilemedi."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "Kargo yola çıktı."));
        }

        [HttpPost("{id:int}/confirm-delivery")]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var order = await _orders.ConfirmDeliveryAsync(GetUserId(), id);
            if (order == null) return BadRequest(ApiResponse.Fail("Teslimat onaylanamadı."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "Teslimat onaylandı."));
        }

        [HttpPost("{id:int}/dispute")]
        public async Task<IActionResult> OpenDispute(int id, [FromBody] OpenDisputeDto dto)
        {
            var order = await _orders.OpenDisputeAsync(GetUserId(), id, dto);
            if (order == null) return BadRequest(ApiResponse.Fail("İtiraz açılamadı."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "İtirazınız alındı. Ekibimiz inceleyecek."));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _orders.CancelOrderAsync(GetUserId(), id, asAdmin: false);
            if (order == null) return BadRequest(ApiResponse.Fail("Sipariş iptal edilemedi."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order, "Sipariş iptal edildi."));
        }

        [HttpGet("{id:int}")]
        [DisableRateLimiting]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orders.GetOrderAsync(GetUserId(), id);
            if (order == null) return NotFound(ApiResponse.Fail("Sipariş bulunamadı."));
            return Ok(ApiResponse<MarketplaceOrderDto>.Ok(order));
        }

        [HttpGet("mine")]
        [DisableRateLimiting]
        public async Task<IActionResult> Mine([FromQuery] bool asSeller = false) =>
            Ok(ApiResponse<List<MarketplaceOrderDto>>.Ok(await _orders.GetMyOrdersAsync(GetUserId(), asSeller)));

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
