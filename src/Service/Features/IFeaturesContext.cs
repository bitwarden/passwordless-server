using Fido2NetLib.Objects;

namespace Passwordless.Service.Features;

public interface IFeaturesContext
{
    public bool EventLoggingIsEnabled { get; init; }
    public int EventLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
    public bool IsInFeaturesContext { get; }
    public bool IsGenerateSignInTokenEndpointEnabled { get; init; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    long? MaxUsers { get; init; }
    
    /// <inheritdoc cref="AttestationConveyancePreference"/>
    public AttestationConveyancePreference Attestation { get; init; }
}