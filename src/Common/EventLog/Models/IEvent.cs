using Passwordless.Common.EventLog.Enums;

namespace Passwordless.Common.EventLog.Models;

public interface IEvent
{
    Guid Id { get; init; }
    DateTime PerformedAt { get; init; }
    EventType EventType { get; init; }
    string Message { get; init; }
    Severity Severity { get; init; }
    string PerformedBy { get; init; }
    string Subject { get; init; }
}