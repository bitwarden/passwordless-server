using Passwordless.Common.AuditLog.Enums;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Common.Models;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog;

public static class AuditEventFunctions
{
    public static AuditEventDto RegistrationTokenCreatedEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        Message = $"Created registration token for {performedBy}",
        Severity = Severity.Informational,
        EventType = AuditEventType.ApiAuthUserRegistered,
        PerformedAt = auditLogContext.PerformedAt,
        PerformedBy = performedBy,
        Subject = auditLogContext.TenantId,
        TenantId = auditLogContext.TenantId,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static AuditEventDto RegistrationBeganEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        Message = $"Beginning passkey registration for {performedBy}",
        PerformedBy = performedBy,
        PerformedAt = auditLogContext.PerformedAt,
        EventType = AuditEventType.ApiAuthPasskeyRegistrationBegan,
        Severity = Severity.Informational,
        Subject = auditLogContext.TenantId,
        TenantId = auditLogContext.TenantId,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static AuditEventDto RegistrationCompletedEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        Message = $"Completed passkey registration for user {performedBy}",
        PerformedBy = performedBy,
        PerformedAt = auditLogContext.PerformedAt,
        EventType = AuditEventType.ApiAuthPasskeyRegistrationCompleted,
        Severity = Severity.Informational,
        Subject = auditLogContext.TenantId,
        TenantId = auditLogContext.TenantId,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static AuditEventDto UserAliasSetEvent(string performedBy, string tenantId, DateTime performedAt, ApplicationSecretKey applicationSecretKey) => new()
    {
        Message = $"Added set aliases for user ({performedBy}).",
        PerformedAt = performedAt,
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserSetAliases,
        Severity = Severity.Informational,
        Subject = performedBy,
        ApiKeyId = applicationSecretKey.AbbreviatedValue
    };

    public static AuditEventDto ApplicationCreatedEvent(string performedBy, string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = $"{tenantId} created.",
        PerformedBy = performedBy,
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

    public static Func<IAuditLogContext, AuditEventDto> AppMarkedToDeleteEvent(string deletedBy) =>
        context => new AuditEventDto
        {
            PerformedAt = context.PerformedAt,
            Message = "Application was marked for deletion.",
            PerformedBy = deletedBy,
            TenantId = context.TenantId,
            EventType = AuditEventType.ApiManagementAppMarkedForDeletion,
            Severity = Severity.Informational,
            Subject = context.TenantId,
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

    public static AuditEventDto DeleteCredentialEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        PerformedAt = auditLogContext.PerformedAt,
        Message = $"Deleted credential for user {performedBy}",
        PerformedBy = performedBy,
        TenantId = auditLogContext.TenantId,
        EventType = AuditEventType.ApiUserDeleteCredential,
        Severity = Severity.Informational,
        Subject = performedBy,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static AuditEventDto UserSignInBeganEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        PerformedAt = auditLogContext.PerformedAt,
        Message = $"User {performedBy} began sign in.",
        PerformedBy = performedBy,
        EventType = AuditEventType.ApiUserSignInBegan,
        Severity = Severity.Informational,
        Subject = auditLogContext.TenantId,
        TenantId = auditLogContext.TenantId,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static AuditEventDto UserSignInCompletedEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        PerformedAt = auditLogContext.PerformedAt,
        Message = $"User {performedBy} completed sign in.",
        PerformedBy = performedBy,
        TenantId = auditLogContext.TenantId,
        EventType = AuditEventType.ApiUserSignInCompleted,
        Severity = Severity.Informational,
        Subject = auditLogContext.TenantId,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static AuditEventDto UserSignInTokenVerifiedEvent(string performedBy, IAuditLogContext auditLogContext) => new()
    {
        PerformedAt = auditLogContext.PerformedAt,
        Message = $"User {performedBy} verified sign in token.",
        PerformedBy = performedBy,
        TenantId = auditLogContext.TenantId,
        EventType = AuditEventType.ApiUserSignInVerified,
        Severity = Severity.Informational,
        Subject = auditLogContext.TenantId,
        ApiKeyId = auditLogContext.AbbreviatedKey
    };

    public static Func<IAuditLogContext, AuditEventDto> DeletedUserEvent(string userId) =>
        context => new AuditEventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"Deleted user {userId}",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = AuditEventType.ApiUserDeleted,
            Severity = Severity.Informational,
            Subject = userId,
            ApiKeyId = context.AbbreviatedKey
        };
}