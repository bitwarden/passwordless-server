using Passwordless.Common.EventLog.Enums;

namespace Passwordless.Common.EventLog.Models;

public class Event : IEvent
{
    public Guid Id { get; init; }
    public DateTime PerformedAt { get; init; }
    public EventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string PerformedBy { get; init; }
    public string Subject { get; init; }
}