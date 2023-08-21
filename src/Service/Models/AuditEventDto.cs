using Passwordless.Service.AuditLog.Enums;

namespace Passwordless.Service.Models;

public class AuditEventDto
{
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;
    public string Message { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public string? TenantId { get; init; }
    public int? OrganizationId { get; init; }
    public AuditEventType EventType { get; set; }
    public Severity Severity { get; set; }
    public string Subject { get; set; }
    public string ApiKeyAbbreviated { get; set; }
}