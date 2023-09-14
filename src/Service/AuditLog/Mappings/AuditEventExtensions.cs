using Passwordless.Common.AuditLog.Enums;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog.Mappings;

public static class AuditEventExtensions
{
    public static AuditEventDto ToEvent(this RegisterToken tokenRequest, string tenantName, DateTime performedAt, PrivateKey secretKey) => new()
    {
        Message = $"Created registration token for {tokenRequest.UserId}",
        Severity = Severity.Informational,
        EventType = AuditEventType.ApiAuthUserRegistered,
        PerformedAt = performedAt,
        PerformedBy = tokenRequest.UserId,
        Subject = tenantName,
        TenantId = tenantName,
        ApiKeyId = secretKey.AbbreviatedValue
    };

    public static AuditEventDto ToEvent(this FidoRegistrationBeginDTO dto, string tenantName, DateTime performedAt, PublicKey publicKey) => new()
    {
        Message = $"Beginning passkey registration for token: {string.Join("***", dto.Token.GetLast(4))}",
        PerformedBy = "",
        PerformedAt = performedAt,
        EventType = AuditEventType.ApiAuthPasskeyRegistrationBegan,
        Severity = Severity.Informational,
        Subject = tenantName,
        TenantId = tenantName,
        ApiKeyId = publicKey.AbbreviatedValue
    };

    public static AuditEventDto RegistrationCompletedEvent(string token, string tenantName, DateTime performedAt, PublicKey publicKey) => new()
    {
        Message = $"Completed passkey registration  for token: {string.Join("***", token.GetLast(4))}.",
        PerformedBy = "",
        PerformedAt = performedAt,
        EventType = AuditEventType.ApiAuthPasskeyRegistrationCompleted,
        Severity = Severity.Informational,
        Subject = tenantName,
        TenantId = tenantName,
        ApiKeyId = publicKey.AbbreviatedValue
    };

    public static AuditEventDto ToEvent(this AliasPayload payload, string tenantName, DateTime performedAt, PrivateKey privateKey) => new()
    {
        Message = $"Added set aliases for user ({payload.UserId}).",
        PerformedAt = performedAt,
        PerformedBy = payload.UserId,
        TenantId = tenantName,
        EventType = AuditEventType.ApiUserSetAliases,
        Severity = Severity.Informational,
        Subject = payload.UserId,
        ApiKeyId = privateKey.AbbreviatedValue
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
        Severity = auditEventDto.Severity,
        PerformedBy = auditEventDto.PerformedBy,
        Subject = auditEventDto.Subject,
        TenantId = auditEventDto.TenantId,
        ApiKeyId = auditEventDto.ApiKeyId
    };
}