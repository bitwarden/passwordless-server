using Passwordless.Service.AuditLog.Enums;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog;

public static class AuditEventExtensions
{
    public static AuditEventDto ToEvent(this RegisterToken tokenRequest, string tenantName, string apiAbbreviation) => new()
    {
        Message = $"Created registration token for {tokenRequest.UserId}",
        Severity = Severity.Informational,
        EventType = AuditEventType.ApiAuthUserRegistered,
        PerformedAt = DateTime.UtcNow,
        PerformedBy = tokenRequest.UserId,
        Subject = tenantName,
        TenantId = tenantName,
        ApiKeyAbbreviated = apiAbbreviation
    };

    public static AuditEvent ToEvent(this AuditEventDto auditEventDto) => new()
    {
        Id = Guid.NewGuid(),
        PerformedAt = auditEventDto.PerformedAt,
        EventType = auditEventDto.EventType,
        Message = auditEventDto.Message,
        Severity = Severity.Alert,
        PerformedBy = auditEventDto.PerformedBy,
        Subject = auditEventDto.Subject,
        TenantId = auditEventDto.TenantId,
        OrganizationId = auditEventDto.OrganizationId,
        ApiKeyAbbreviated = auditEventDto.ApiKeyAbbreviated
    };
}