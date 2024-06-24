namespace Passwordless.Common.Overrides;

/// <summary>
/// Configures behavior overrides for an application.
/// </summary>
public class ApplicationOverrides
{
    /// <summary>
    /// Whether actions on behalf of this application bypass rate limiting.
    /// </summary>
    public bool IsRateLimitBypassEnabled { get; init; }

    /// <summary>
    /// Whether actions on behalf of this application bypass magic link quotas and restrictions.
    /// </summary>
    public bool IsMagicLinkQuotaBypassEnabled { get; init; }
}