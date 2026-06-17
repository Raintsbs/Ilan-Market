namespace AdvertisementApp.Common.Result
{
    public interface IDataResult<out T> : IResult
    {
        T? Data { get; }
    }
}
