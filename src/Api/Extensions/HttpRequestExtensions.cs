using UAParser;

namespace Passwordless.Api.Extensions;

public static class HttpRequestExtensions
{
    public static string? GetApiSecret(this HttpRequest req) => req.Headers.GetApiSecret();
    
    public static string? GetApiSecret(this IHeaderDictionary headerDictionary) => headerDictionary.GetHeaderValue("ApiSecret");

    public static string? GetPublicApiKey(this HttpRequest req) => req.Headers.GetPublicApiKey();

    public static string? GetPublicApiKey(this IHeaderDictionary headerDictionary) => headerDictionary.GetHeaderValue("ApiKey");

    private static string? GetHeaderValue(this IHeaderDictionary headerDictionary, string key)
    {
        headerDictionary.TryGetValue(key, out var value);
        return value.SingleOrDefault();
    }

    public static string? GetTenantNameFromKey(this HttpRequest req)
    {
        var key = req.GetPublicApiKey() ?? req.GetApiSecret();

        if (key == null)
        {
            key = req.Query["key"];
        }

        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        var span = key.AsSpan();
        var i = span.IndexOf(':');

        if (i == -1)
        {
            return null;
        }

        return span.Slice(0, i).ToString();
    }

    public static string? GetTenantName(this HttpRequest httpRequest) =>
        GetTenantNameFromKey(httpRequest) ?? httpRequest.RouteValues["appId"]?.ToString();
    
}


public static class Helpers
{

    public static (string deviceInfo, string country) GetDeviceInfo(HttpRequest req)
    {
        var uap = Parser.GetDefault();
        var d = uap.Parse(req.Headers["User-Agent"]);

        var deviceInfo = $"{d.UA.Family}, {d.OS.Family} {d.OS.Major}";
        var country = string.Empty;
        if (req.Headers.TryGetValue("CF-IPCountry", out var countryh))
        {
            country = countryh.FirstOrDefault();
        }

        return (deviceInfo, country);
    }
}