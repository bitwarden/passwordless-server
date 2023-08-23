using Passwordless.Service.AuditLog.Enums;

namespace Passwordless.Service.AuditLog.Models;

public class AuditEvent
{
    public Guid Id { get; init; }
    public DateTime PerformedAt { get; init; }
    public AuditEventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string PerformedBy { get; init; }
    public string Subject { get; init; }
    public string? TenantId { get; init; }
    public int? OrganizationId { get; init; }
    public string ApiKeyAbbreviated { get; init; }
}