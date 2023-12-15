namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool EventLoggingIsEnabled,
        int EventLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt,
        long? MaxUsers,
        bool IsGenerateSignInTokenEndpointEnabled)
    : IFeaturesContext
{
    public bool IsInFeaturesContext => true;
}