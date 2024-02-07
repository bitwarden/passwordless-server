using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public sealed class ManageFeaturesRequest
{
    public bool EventLoggingIsEnabled { get; init; }

    [Range(0, 90)]
    public int EventLoggingRetentionPeriod { get; init; }

    /// <summary>
    /// Maximum monthly limit for magic link emails sent by this application.
    /// The actual limit may be lower, depending on the age of the application.
    /// </summary>
    public int MagicLinkEmailMaxMonthlyLimit { get; init; }

    /// <summary>
    /// By-minute rate limit for magic link emails sent by this application.
    /// The actual limit may be lower, depending on the age of the application.
    /// </summary>
    public int MagicLinkEmailMaxMinutelyLimit { get; init; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; init; }

    public bool AllowAttestation { get; init; }
}