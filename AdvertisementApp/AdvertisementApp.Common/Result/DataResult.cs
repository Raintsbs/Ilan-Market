namespace AdvertisementApp.Common.Result
{
    public class DataResult<T> : IDataResult<T>
    {
        public bool Success { get; }
        public string Message { get; }
        public T? Data { get; }

        public DataResult(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static DataResult<T> Ok(T data, string message = "") => new(true, message, data);
        public static DataResult<T> Fail(string message) => new(false, message);
    }
}
