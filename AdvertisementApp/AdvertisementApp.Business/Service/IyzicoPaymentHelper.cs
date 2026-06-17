using AdvertisementApp.DataAccess.Entities;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class IyzicoPaymentHelper
    {
        private readonly IConfiguration _config;
        private readonly ILogger<IyzicoPaymentHelper> _logger;

        public IyzicoPaymentHelper(IConfiguration config, ILogger<IyzicoPaymentHelper> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_config["Iyzico:ApiKey"]) &&
            !string.IsNullOrWhiteSpace(_config["Iyzico:SecretKey"]);

        private Options BuildOptions() => new()
        {
            ApiKey = _config["Iyzico:ApiKey"]!.Trim(),
            SecretKey = _config["Iyzico:SecretKey"]!.Trim(),
            BaseUrl = string.IsNullOrWhiteSpace(_config["Iyzico:BaseUrl"])
                ? "https://sandbox-api.iyzipay.com"
                : _config["Iyzico:BaseUrl"]!.Trim(),
        };

        public async Task<CheckoutFormInitializeResult?> CreateCheckoutAsync(
            AppUser user,
            string conversationId,
            string basketId,
            decimal amountTry,
            string itemName,
            string itemCategory,
            string callbackUrl)
        {
            if (!IsConfigured) return null;

            var price = amountTry.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var buyerName = string.IsNullOrWhiteSpace(user.FirstName) ? "Musteri" : user.FirstName;
            var buyerSurname = string.IsNullOrWhiteSpace(user.LastName) ? "Kullanici" : user.LastName;
            var email = string.IsNullOrWhiteSpace(user.Email) ? "musteri@ilanmarket.local" : user.Email;
            var gsm = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "+905555555555" : user.PhoneNumber;

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = conversationId,
                Price = price,
                PaidPrice = price,
                Currency = Currency.TRY.ToString(),
                BasketId = basketId,
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = callbackUrl,
                EnabledInstallments = new List<int> { 1, 2, 3, 6, 9 },
            };

            request.Buyer = new Buyer
            {
                Id = user.Id.ToString(),
                Name = buyerName,
                Surname = buyerSurname,
                GsmNumber = gsm,
                Email = email,
                IdentityNumber = "11111111111",
                RegistrationAddress = "Turkiye",
                Ip = "85.34.78.112",
                City = "Istanbul",
                Country = "Turkey",
            };

            var address = new Address
            {
                ContactName = $"{buyerName} {buyerSurname}",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Turkiye",
            };
            request.ShippingAddress = address;
            request.BillingAddress = address;

            request.BasketItems =
            [
                new BasketItem
                {
                    Id = basketId,
                    Name = itemName.Length > 100 ? itemName[..100] : itemName,
                    Category1 = itemCategory,
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = price,
                },
            ];

            var result = await CheckoutFormInitialize.Create(request, BuildOptions());
            if (result.Status != "success" || string.IsNullOrWhiteSpace(result.PaymentPageUrl))
            {
                _logger.LogWarning("iyzico checkout basarisiz: {Status} {Error}", result.Status, result.ErrorMessage);
                return null;
            }

            return new CheckoutFormInitializeResult
            {
                PaymentPageUrl = result.PaymentPageUrl,
                Token = result.Token,
            };
        }

        public async Task<CheckoutForm?> RetrieveCheckoutAsync(string token)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(token)) return null;

            var request = new RetrieveCheckoutFormRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Token = token,
            };
            return await CheckoutForm.Retrieve(request, BuildOptions());
        }
    }

    public class CheckoutFormInitializeResult
    {
        public string PaymentPageUrl { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
