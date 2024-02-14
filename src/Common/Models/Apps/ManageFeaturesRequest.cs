using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public sealed class ManageFeaturesRequest
{
    public bool EventLoggingIsEnabled { get; init; }

    [Range(0, 90)]
    public int EventLoggingRetentionPeriod { get; init; }

    /// <summary>
    /// Maximum allowed magic link emails sent for this application.
    /// Depending on the age of the application, the actual limit may be lower.
    /// </summary>
    public int MagicLinkEmailMonthlyQuota { get; init; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; init; }

    public bool AllowAttestation { get; init; }
}