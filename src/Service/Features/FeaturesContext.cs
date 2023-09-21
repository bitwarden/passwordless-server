namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool AuditLoggingIsEnabled,
        int AuditLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt,
        int? MaxUsers)
    : IFeaturesContext
{
    public bool IsInFeaturesContext => true;
}