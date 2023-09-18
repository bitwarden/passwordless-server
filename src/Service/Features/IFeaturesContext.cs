namespace Passwordless.Service.Features;

public interface IFeaturesContext
{
    bool AuditLoggingIsEnabled { get; init; }
    int AuditLoggingRetentionPeriod { get; init; }
    DateTime? DeveloperLoggingEndsAt { get; init; }
    int? MaxUsers { get; init; }
    bool IsInFeaturesContext { get; }
}