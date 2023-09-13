using Passwordless.Common.AuditLog.Enums;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog.Mappings;

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
        ApiKeyId = apiAbbreviation
    };

    public static AuditEventResponse ToEvent(this ApplicationAuditEvent dbEvent) => new
    (
        dbEvent.PerformedAt,
        dbEvent.Message,
        dbEvent.PerformedBy,
        dbEvent.TenantId,
        dbEvent.EventType,
        dbEvent.Severity,
        dbEvent.Subject,
        dbEvent.ApiKeyId
    );

    public static ApplicationAuditEvent ToEvent(this AuditEventDto auditEventDto) => new()
    {
        Id = Guid.NewGuid(),
        PerformedAt = auditEventDto.PerformedAt,
        EventType = auditEventDto.EventType,
        Message = auditEventDto.Message,
        Severity = Severity.Alert,
        PerformedBy = auditEventDto.PerformedBy,
        Subject = auditEventDto.Subject,
        TenantId = auditEventDto.TenantId,
        ApiKeyId = auditEventDto.ApiKeyId
    };
}