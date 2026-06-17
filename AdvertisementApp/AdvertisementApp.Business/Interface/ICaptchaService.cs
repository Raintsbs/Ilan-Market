namespace AdvertisementApp.Business.Interface
{
    public interface ICaptchaService
    {
        bool IsEnabled { get; }
        Task<bool> VerifyAsync(string? token, string? remoteIp);
    }
}
