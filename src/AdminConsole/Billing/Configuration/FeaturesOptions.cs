namespace Passwordless.AdminConsole.Billing.Configuration;

public class FeaturesOptions
{
    public bool EventLoggingIsEnabled { get; set; }

    public int EventLoggingRetentionPeriod { get; set; }

    /// <summary>
    /// Maximum monthly limit for magic link emails sent by this application.
    /// The actual limit may be lower, depending on the age of the application.
    /// </summary>
    public int MagicLinkEmailMaxMonthlyLimit { get; set; }

    /// <summary>
    /// By-minute rate limit for magic link emails sent by this application.
    /// The actual limit may be lower, depending on the age of the application.
    /// </summary>
    public int MagicLinkEmailMaxMinutelyLimit { get; set; }

    public int MaxAdmins { get; set; }

    public int MaxApplications { get; set; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; set; }

    public bool AllowAttestation { get; set; }
}