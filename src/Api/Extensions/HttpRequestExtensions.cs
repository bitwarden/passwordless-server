using Passwordless.Common.Utils;
using UAParser;

namespace Passwordless.Api.Extensions;

public static class HttpRequestExtensions
{
    public static string? GetApiSecret(this HttpRequest req) => req.Headers.GetApiSecret();

    public static string? GetPublicApiKey(this HttpRequest req) => req.Headers.GetPublicApiKey();

    public static string? GetTenantNameFromKey(this HttpRequest request)
    {
        var key = request.GetPublicApiKey() ?? request.GetApiSecret();

        return key != null ? ApiKeyUtils.GetAppId(key) : null;
    }

    public static string? GetTenantName(this HttpRequest httpRequest) =>
        GetTenantNameFromKey(httpRequest) ?? httpRequest.RouteValues["appId"]?.ToString();
}

public static class Helpers
{
    public static (string deviceInfo, string country) GetDeviceInfo(this HttpRequest req)
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