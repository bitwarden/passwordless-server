using AdminConsole.Identity;
using AdminConsole.Models;
using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog;

public static class AuditLogEventFunctions
{
    public static AuditLogEventRequest CreateOrganizationCreatedEvent(Organization organization, ConsoleAdmin performedBy) =>
        new(performedBy.Id,
            7000,
            $"Organization {organization.Name} created by {performedBy.Name}.",
            2,
            organization.Id.ToString(),
            null,
            organization.Id);
}