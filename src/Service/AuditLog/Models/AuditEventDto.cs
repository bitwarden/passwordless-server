using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.Common.AuditLog.Models;

public class AuditEventDto
{
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;
    public string Message { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public AuditEventType EventType { get; init; }
    public Severity Severity { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string ApiKeyId { get; init; } = string.Empty;
}