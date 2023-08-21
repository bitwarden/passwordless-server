using Passwordless.Service.AuditLog.Enums;

namespace Passwordless.Service.AuditLog.Models;

public class AuditEvent
{
    public Guid Id { get; init; }
    public DateTime PerformedAt { get; init; }
    public AuditEventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string PerformedBy { get; init; } // who performed the action | Performed By could be Api key as well to identify the app (***3213) last 4 of key ?
    public string Subject { get; init; } // entity being acted upon
    public string? TenantId { get; init; }
    public int? OrganizationId { get; init; }
    public string ApiKeyAbbreviated { get; init; } // Api key as well to identify the app (3213) last 4 of key ?

}