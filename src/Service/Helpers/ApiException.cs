namespace Passwordless.Service.Helpers;

public class ApiException(
    string? errorCode,
    string message,
    int statusCode = 500,
    Dictionary<string, object>? extras = null)
    : Exception(message)
{
    public ApiException(string message, int statusCode = 500, Dictionary<string, object>? extras = null)
        : this(null, message, statusCode, extras)
    {
    }

    public string? ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
    public Dictionary<string, object>? Extras { get; } = extras;
}