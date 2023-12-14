namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool EventLoggingIsEnabled,
        int EventLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt,
        long? MaxUsers,
        bool SignInTokenEndpointEnabled)
    : IFeaturesContext
{
    public bool IsInFeaturesContext => true;
}