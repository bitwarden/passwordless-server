using Fido2NetLib.Objects;

namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool EventLoggingIsEnabled,
        int EventLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt,
        long? MaxUsers,
        AttestationConveyancePreference Attestation,
        bool IsGenerateSignInTokenEndpointEnabled)
    : IFeaturesContext
{
    public bool IsInFeaturesContext => true;
}