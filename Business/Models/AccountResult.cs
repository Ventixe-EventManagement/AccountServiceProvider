namespace Business.Models;

public class AccountResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; } = 200;

    public static AccountResult CreateSuccess(int statusCode = 200) => new()
    {
        Success = true,
        StatusCode = statusCode
    };

    public static AccountResult CreateFailure(string error, int statusCode = 400) => new()
    {
        Success = false,
        Error = error,
        StatusCode = statusCode
    };
}

public class AccountResult<T> : AccountResult
{
    public T? Result { get; set; }

    public static AccountResult<T> CreateSuccess(T result, int statusCode = 200) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Result = result
    };

    public new static AccountResult<T> CreateFailure(string error, int statusCode = 400) => new()
    {
        Success = false,
        Error = error,
        StatusCode = statusCode
    };
}