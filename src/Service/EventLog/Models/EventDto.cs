using Passwordless.Common.EventLog.Enums;

namespace Passwordless.Service.EventLog.Models;

public class EventDto
{
    public DateTime PerformedAt { get; init; } = DateTime.UtcNow;
    public string Message { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public EventType EventType { get; init; }
    public Severity Severity { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string ApiKeyId { get; init; } = string.Empty;
}