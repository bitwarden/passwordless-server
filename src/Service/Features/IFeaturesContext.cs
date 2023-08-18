namespace Passwordless.Service.Features;

public interface IFeaturesContext
{
    public bool AuditLoggingIsEnabled { get; init; }
    public int AuditLoggingRetentionPeriod { get; init; }
    public DateTime? DeveloperLoggingEndsAt { get; init; }
}