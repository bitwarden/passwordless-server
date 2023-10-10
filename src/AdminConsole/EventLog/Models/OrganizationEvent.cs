using Passwordless.Common.EventLog.Models;

namespace Passwordless.AdminConsole.EventLog.Models;

public class OrganizationEvent : Event
{
    public int OrganizationId { get; init; }
}