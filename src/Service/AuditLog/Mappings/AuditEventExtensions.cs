using Passwordless.Common.AuditLog.Enums;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog.Mappings;

public static class AuditEventExtensions
{
    public static AuditEventDto ToEvent(this RegisterToken tokenRequest, string tenantId, DateTime performedAt, PrivateKey secretKey) => new()
    {
        Message = $"Created registration token for {tokenRequest.UserId}",
        Severity = Severity.Informational,
        EventType = AuditEventType.ApiAuthUserRegistered,
        PerformedAt = performedAt,
        PerformedBy = tokenRequest.UserId,
        Subject = tenantId,
        TenantId = tenantId,
        ApiKeyId = secretKey.AbbreviatedValue
    };

    public static AuditEventDto ToEvent(this FidoRegistrationBeginDTO dto, string tenantId, DateTime performedAt, ApplicationPublicKey applicationPublicKey) => new()
    {
        Message = $"Beginning passkey registration for token: {string.Join("***", dto.Token.GetLast(4))}",
        PerformedBy = "",
        PerformedAt = performedAt,
        EventType = AuditEventType.ApiAuthPasskeyRegistrationBegan,
        Severity = Severity.Informational,
        Subject = tenantId,
        TenantId = tenantId,
        ApiKeyId = applicationPublicKey.AbbreviatedValue
    };

    public static AuditEventDto RegistrationCompletedEvent(string token, string tenantId, DateTime performedAt, ApplicationPublicKey applicationPublicKey) => new()
    {
        Message = $"Completed passkey registration  for token: {string.Join("***", token.GetLast(4))}.",
        PerformedBy = "",
        PerformedAt = performedAt,
        EventType = AuditEventType.ApiAuthPasskeyRegistrationCompleted,
        Severity = Severity.Informational,
        Subject = tenantId,
        TenantId = tenantId,
        ApiKeyId = applicationPublicKey.AbbreviatedValue
    };

    public static AuditEventDto ToEvent(this AliasPayload payload, string tenantId, DateTime performedAt, PrivateKey privateKey) => new()
    {
        Message = $"Added set aliases for user ({payload.UserId}).",
        PerformedAt = performedAt,
        PerformedBy = payload.UserId,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserSetAliases,
        Severity = Severity.Informational,
        Subject = payload.UserId,
        ApiKeyId = privateKey.AbbreviatedValue
    };

    public static AuditEventDto ToEvent(this AppCreateDTO dto, string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = $"{tenantId} created.",
        PerformedBy = dto.AdminEmail,
        TenantId = tenantId,
        EventType = AuditEventType.ApiManagementAppCreated,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = ""
    };

    public static AuditEventDto AppFrozenEvent(string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application frozen.",
        PerformedBy = "System",
        TenantId = tenantId,
        EventType = AuditEventType.ApiManagementAppFrozen,
        Severity = Severity.Alert,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static AuditEventDto AppUnfrozenEvent(string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application unfrozen.",
        PerformedBy = "System",
        TenantId = tenantId,
        EventType = AuditEventType.ApiManagementAppUnfrozen,
        Severity = Severity.Alert,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static AuditEventDto AppMarkedToDeleteEvent(string performedBy, string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application was marked for deletion.",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiManagementAppMarkedForDeletion,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static AuditEventDto AppDeleteCancelledEvent(string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application deletion was cancelled.",
        PerformedBy = "System",
        TenantId = tenantId,
        EventType = AuditEventType.ApiManagementAppDeletionCancelled,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static AuditEventDto DeleteCredentialEvent(string performedBy, DateTime performedAt, string tenantId, PrivateKey apiSecret) => new()
    {
        PerformedAt = performedAt,
        Message = $"Deleted credential for user {performedBy}",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserDeleteCredential,
        Severity = Severity.Informational,
        Subject = performedBy,
        ApiKeyId = apiSecret.AbbreviatedValue
    };

    public static AuditEventDto UserSignInBeganEvent(string performedBy, DateTime performedAt, string tenantId, ApplicationPublicKey applicationPublicKey) => new()
    {
        PerformedAt = performedAt,
        Message = $"User {performedBy} began sign in.",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserSignInBegan,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = applicationPublicKey.AbbreviatedValue
    };

    public static AuditEventDto UserSignInCompletedEvent(string performedBy, DateTime performedAt, string tenantId, ApplicationPublicKey applicationPublicKey) => new()
    {
        PerformedAt = performedAt,
        Message = $"User {performedBy} completed sign in.",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserSignInCompleted,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = applicationPublicKey.AbbreviatedValue
    };

    public static AuditEventDto UserSignInTokenVerifiedEvent(string performedBy, DateTime performedAt, string tenantId, PrivateKey privateKey) => new()
    {
        PerformedAt = performedAt,
        Message = $"User {performedBy} verified sign in token.",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserSignInCompleted,
        Severity = Severity.Informational,
        Subject = tenantId,
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