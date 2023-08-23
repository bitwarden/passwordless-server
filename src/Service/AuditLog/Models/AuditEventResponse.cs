namespace Passwordless.Service.AuditLog.Models;

public class AuditEventResponse
{
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;
    public string Message { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public string? TenantId { get; init; }
    public int? OrganizationId { get; init; }
    public string EventType { get; set; }
    public string Severity { get; set; }
    public string Subject { get; set; }
    public string ApiKeyAbbreviated { get; set; }
}