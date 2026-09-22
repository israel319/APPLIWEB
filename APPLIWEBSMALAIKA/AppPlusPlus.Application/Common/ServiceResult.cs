namespace AppPlusPlus.Application.Common;

public class ServiceResult
{
    public bool Success { get; }
    public string Message { get; }

    protected ServiceResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static ServiceResult Ok(string message = "") => new(true, message);
    public static ServiceResult Fail(string message) => new(false, message);
    public static ServiceResult<T> Ok<T>(T data, string message = "") => new(data, true, message);
    public static ServiceResult<T> Fail<T>(string message) => new(default, false, message);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    internal ServiceResult(T? data, bool success, string message) : base(success, message)
    {
        Data = data;
    }
}
