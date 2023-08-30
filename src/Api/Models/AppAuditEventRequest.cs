using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.Api.Models;

public class AppAuditEventRequest
{
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;
    public string Message { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public string? TenantId { get; init; }
    public AuditEventType EventType { get; set; }
    public Severity Severity { get; set; }
    public string Subject { get; set; }
    public string ApiKeyId { get; set; }
}