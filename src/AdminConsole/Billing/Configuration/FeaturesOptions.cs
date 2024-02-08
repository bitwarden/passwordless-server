namespace Passwordless.AdminConsole.Billing.Configuration;

public class FeaturesOptions
{
    public bool EventLoggingIsEnabled { get; set; }

    public int EventLoggingRetentionPeriod { get; set; }

    /// <summary>
    /// Maximum allowed magic link emails sent for this application.
    /// Depending on the age of the application, the actual limit may be lower.
    /// </summary>
    public int MagicLinkEmailMonthlyQuota { get; set; }

    public int MaxAdmins { get; set; }

    public int MaxApplications { get; set; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; set; }

    public bool AllowAttestation { get; set; }
}