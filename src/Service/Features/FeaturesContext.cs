namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool EventLoggingIsEnabled,
        int EventLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt)
    : IFeaturesContext
{
    public bool IsInFeaturesContext => true;
}