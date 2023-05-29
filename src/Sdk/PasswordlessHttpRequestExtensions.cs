namespace Passwordless.Net;

internal static class PasswordlessHttpRequestExtensions
{
    internal static HttpRequestOptionsKey<bool> SkipErrorHandlingOption = new(nameof(SkipErrorHandling));

    internal static HttpRequestMessage SkipErrorHandling(this HttpRequestMessage request, bool skip = true)
    {
        request.Options.Set(SkipErrorHandlingOption, skip);
        return request;
    }
}