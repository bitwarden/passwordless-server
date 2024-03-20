namespace Passwordless.Api.Helpers;

public static class FeatureFlagging
{
    public static bool IsRateLimitBypassEnabled(this IConfiguration configuration)
        => configuration.GetValue<bool>("Flags:IsRateLimitBypassEnabled");

    public static bool IsRateLimitBypassRequested(this HttpRequest request)
        => request.Headers.TryGetValue("X-RateLimit-Bypass", out var value) &&
           string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
}