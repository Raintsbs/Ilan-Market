namespace AdvertisementApp.Business.Interface
{
    public interface IFavoriteService
    {
        Task<bool> IsFavoriteAsync(int userId, int advertisementId);
    }
}
