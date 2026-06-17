namespace AdvertisementApp.Common.Result
{
    public interface IResult
    {
        bool Success { get; }
        string Message { get; }
    }
}
