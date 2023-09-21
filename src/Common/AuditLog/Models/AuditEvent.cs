using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.Common.AuditLog.Models;

public class AuditEvent
{
    public Guid Id { get; init; }
    public DateTime PerformedAt { get; init; }
    public AuditEventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string PerformedBy { get; init; }
    public string Subject { get; init; }
}