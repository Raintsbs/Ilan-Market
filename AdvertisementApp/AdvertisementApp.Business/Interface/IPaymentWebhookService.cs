namespace AdvertisementApp.Business.Interface
{
    public interface IPaymentWebhookService
    {
        Task<bool> ProcessStripeWebhookAsync(string json, string? signature);
        Task<bool> ProcessIyzicoCallbackAsync(string token);
        Task RetryFailedEventsAsync(int maxBatch = 10);
    }
}
