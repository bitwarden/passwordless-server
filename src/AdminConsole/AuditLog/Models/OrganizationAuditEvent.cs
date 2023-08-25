using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.AdminConsole.AuditLog.Models;

public class OrganizationAuditEvent
{
    public Guid Id { get; init; }
    public DateTime PerformedAt { get; init; }
    public AuditEventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string PerformedBy { get; init; }
    public string Subject { get; init; }
    public int OrganizationId { get; init; }
    public string ManagementKeyId { get; init; }
}