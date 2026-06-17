namespace AdvertisementApp.Business.Interface
{
    public interface IEmailTemplateService
    {
        Task SendWelcomeAsync(string to, string firstName);
        Task SendAdApprovedAsync(string to, string adTitle, string linkPath);
        Task SendPaymentConfirmedAsync(string to, string description, decimal amount, string? linkPath);
        Task SendOrderNotificationAsync(string to, string title, string body, string? linkPath);
        Task SendListingQuestionEmailAsync(string to, string adTitle, string question, string linkPath);
        Task SendQuestionAnsweredEmailAsync(string to, string adTitle, string answer, string linkPath);
        Task SendFollowedSellerListingEmailAsync(string to, string sellerName, string adTitle, string linkPath);
        Task SendSellerPayoutEmailAsync(string to, decimal amount, string? note, string linkPath);
    }
}
