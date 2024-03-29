namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool EventLoggingIsEnabled,
        int EventLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt,
        long? MaxUsers,
        bool AllowAttestation,
        bool IsGenerateSignInTokenEndpointEnabled,
        bool IsMagicLinksEnabled)
    : IFeaturesContext
{
    public bool IsInFeaturesContext => true;
}