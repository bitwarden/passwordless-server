using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Mappings;

public static class AuditEventExtensions
{
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
        Severity = auditEventDto.Severity,
        PerformedBy = auditEventDto.PerformedBy,
        Subject = auditEventDto.Subject,
        TenantId = auditEventDto.TenantId,
        ApiKeyId = auditEventDto.ApiKeyId
    };
}