namespace AdvertisementApp.Business.Interface
{
    public interface IFullTextSearchService
    {
        bool IsAvailable { get; }
        Task<List<int>> SearchAdvertisementIdsAsync(string term, int maxResults = 500);
    }
}
