namespace AdvertisementApp.Business.Interface
{
    public interface ISmsService
    {
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
    }
}
