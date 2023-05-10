namespace Passwordless.Service.Helpers;

public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }

    public ApiException(string message, int statusCode, Dictionary<string, object> extras = null) : base(message)
    {
        StatusCode = statusCode;
        Extras = extras;
    }

    public ApiException(string errorCode, string message, int statusCode, Dictionary<string, object> extras = null) :
        base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Extras = extras;
    }

    public string ErrorCode { get; }
    public int StatusCode { get; set; } = 500;
    public Dictionary<string, object> Extras { get; }
}