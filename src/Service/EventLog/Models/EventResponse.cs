using Passwordless.Common.EventLog.Enums;

namespace Passwordless.Service.EventLog.Models;

public class EventResponse
{
    public EventResponse(DateTime performedAt, string message, string performedBy, string applicationId, EventType eventType, Severity severity, string subject, string apiKeyId)
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