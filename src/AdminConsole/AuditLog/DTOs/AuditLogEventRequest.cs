namespace Passwordless.AdminConsole.AuditLog.DTOs;

public record AuditLogEventRequest(
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