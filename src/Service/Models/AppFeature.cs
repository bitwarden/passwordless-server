namespace Passwordless.Service.Models;

public class AppFeature : PerTenant
{
    public bool EventLoggingIsEnabled { get; set; }

    /// <summary>
    /// The event logging retention period in days.
    /// </summary>
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

    /// <summary>
    /// Developer logging is only enabled when an end date has been set, and has to be manually re-enabled every time.
    /// </summary>
    public DateTime? DeveloperLoggingEndsAt { get; set; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; set; }

    /// <summary>
    /// Determines or now whether attestation is allowed for this application.
    /// </summary>
    public bool AllowAttestation { get; set; }

    /// <summary>
    /// Determines if the Sign In Token Endpoint is enabled or disabled
    /// </summary>
    public bool IsGenerateSignInTokenEndpointEnabled { get; set; } = true;

    /// <summary>
    /// Determines if the Magic Link feature is enabled or disabled
    /// </summary>
    public bool IsMagicLinksEnabled { get; set; } = true;

    public AccountMetaInformation? Application { get; set; }

    public IReadOnlyCollection<Authenticator>? Authenticators { get; set; }
}