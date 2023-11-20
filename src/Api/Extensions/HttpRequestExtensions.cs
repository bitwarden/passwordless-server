using Passwordless.Common.Utils;
using UAParser;

namespace Passwordless.Api.Extensions;

public static class HttpRequestExtensions
{
    public static string GetApiSecret(this HttpRequest req)
    {
        req.Headers.TryGetValue("ApiSecret", out var value);
        return value.SingleOrDefault();
    }

    public static string GetPublicApiKey(this HttpRequest req)
    {
        req.Headers.TryGetValue("ApiKey", out var value);
        return value.SingleOrDefault();
    }

    public static string? GetTenantName(this HttpRequest request)
    {
        if (request.RouteValues.ContainsKey("appId"))
        {
            return request.RouteValues["appId"].ToString();
        }
        if (request.Headers.ContainsKey("ApiKey"))
        {
            return ApiKeyUtils.GetAppId(request.Headers["ApiKey"].ToString());
        }
        if (request.Headers.ContainsKey("ApiSecret"))
        {
            return ApiKeyUtils.GetAppId(request.Headers["ApiSecret"].ToString());
        }

        return null;
    }
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