namespace Passwordless.Service.Features;

public sealed record FeaturesContext(
        bool AuditLoggingIsEnabled,
        int AuditLoggingRetentionPeriod,
        DateTime? DeveloperLoggingEndsAt)
    : IFeaturesContext;