using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.Common.EventLog.Models;

namespace Passwordless.AdminConsole.EventLog.Models;

public class OrganizationEvent : Event
{
    public int OrganizationId { get; init; }

    public OrganizationEventDto ToDto() =>
        new(PerformedBy,
            EventType,
            Message,
            Severity,
            Subject,
            OrganizationId,
            PerformedAt);
}