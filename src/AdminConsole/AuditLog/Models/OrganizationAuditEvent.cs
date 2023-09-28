using Passwordless.Common.AuditLog.Models;

namespace Passwordless.AdminConsole.AuditLog.Models;

public class OrganizationAuditEvent : AuditEvent
{
    public int OrganizationId { get; init; }
}