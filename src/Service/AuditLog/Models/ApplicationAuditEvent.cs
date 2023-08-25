namespace Passwordless.Service.AuditLog.Models;

public class ApplicationAuditEvent : AuditEvent
{
    public string TenantId { get; init; }
}