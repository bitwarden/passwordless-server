using Passwordless.Common.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Models;

public class ApplicationAuditEvent : AuditEvent
{
    public string TenantId { get; init; }
    public string ApiKeyId { get; init; }
}