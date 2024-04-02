namespace Passwordless.Api.Overrides;

/// <summary>
/// Configures behavior overrides for an application.
/// </summary>
public record ApplicationOverrides(string ApplicationId)
{
    /// <summary>
    /// Whether actions on behalf of this application bypass rate limiting.
    /// </summary>
    public bool IsRateLimitBypassEnabled { get; init; }
}