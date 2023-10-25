namespace Passwordless.AdminConsole.EventLog.DTOs;

public record EventLogEventRequest(
    string PerformedBy,
    int EventType,
    string Message,
    int Severity,
    string Subject,
    string? TenantId,
    int? OrganizationId)
{
    public DateTime PerformedAt { get; } = DateTime.UtcNow;
};