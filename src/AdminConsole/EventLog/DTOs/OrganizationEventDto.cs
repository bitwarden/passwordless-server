using Passwordless.AdminConsole.EventLog.Models;
using Passwordless.Common.EventLog.Enums;

namespace Passwordless.AdminConsole.EventLog.DTOs;

public record OrganizationEventDto(
    string PerformedBy,
    EventType EventType,
    string Message,
    Severity Severity,
    string Subject,
    int OrganizationId,
    DateTime PerformedAt)
{
    public OrganizationEvent ToNewEvent() =>
        new()
        {
            Id = Guid.NewGuid(),
            PerformedAt = PerformedAt,
            EventType = EventType,
            Message = Message,
            Severity = Severity,
            PerformedBy = PerformedBy,
            Subject = Subject,
            OrganizationId = OrganizationId
        };

    public EventLogEvent ToResponse() =>
        new(PerformedAt,
            EventType.ToString(),
            Message,
            Severity.ToString(),
            PerformedBy,
            Subject,
            string.Empty);
}