namespace AdvertisementApp.Common.Result
{
    public class Result : IResult
    {
        public bool Success { get; }
        public string Message { get; }

        public Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static Result Ok(string message = "") => new(true, message);
        public static Result Fail(string message) => new(false, message);
    }
}
