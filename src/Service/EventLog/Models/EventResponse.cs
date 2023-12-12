using Passwordless.Common.EventLog.Enums;

namespace Passwordless.Service.EventLog.Models;

public class EventResponse
{
    public EventResponse() { }

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

    public DateTime PerformedAt { get; init; }
    public string Message { get; init; }
    public string PerformedBy { get; init; }
    public string TenantId { get; init; }
    public string EventType { get; init; }
    public string Severity { get; init; }
    public string Subject { get; init; }
    public string ApiKeyId { get; init; }
}