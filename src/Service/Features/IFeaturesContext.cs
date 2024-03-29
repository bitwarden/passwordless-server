namespace Passwordless.Service.Features;

public interface IFeaturesContext
{
    public bool EventLoggingIsEnabled { get; init; }
    public int EventLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
    public bool IsInFeaturesContext { get; }
    public bool IsGenerateSignInTokenEndpointEnabled { get; init; }
    public bool IsMagicLinksEnabled { get; init; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    long? MaxUsers { get; init; }

    /// <summary>
    /// Determines or now whether attestation is allowed for this application.
    /// </summary>
    public bool AllowAttestation { get; init; }
}