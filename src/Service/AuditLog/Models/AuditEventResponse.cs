using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.Service.AuditLog.Models;

public class AuditEventResponse
{
    public AuditEventResponse(DateTime performedAt, string message, string performedBy, string applicationId, AuditEventType eventType, Severity severity, string subject, string apiKeyId)
    {
        PerformedAt = performedAt;
        Message = message;
        PerformedBy = performedBy;
        TenantId = applicationId;
        EventType = eventType.ToString();
        Severity = severity.ToString();
        Subject = subject;
        ApiKeyId = apiKeyId;
    }

    public DateTime PerformedAt { get; }
    public string Message { get; }
    public string PerformedBy { get; }
    public string TenantId { get; }
    public string EventType { get; }
    public string Severity { get; }
    public string Subject { get; }
    public string ApiKeyId { get; }
}