using Passwordless.Common.AuditLog.Enums;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models;

namespace Passwordless.Service.AuditLog;

public static class AuditEventFunctions
{
    public static AuditEventDto RegistrationTokenCreatedEvent(string performedBy, string tenantId, DateTime performedAt, ApplicationSecretKey secretKey) => new()
    {
        Message = $"Created registration token for {performedBy}",
        Severity = Severity.Informational,
        EventType = AuditEventType.ApiAuthUserRegistered,
        PerformedAt = performedAt,
        PerformedBy = performedBy,
        Subject = tenantId,
        TenantId = tenantId,
        ApiKeyId = secretKey.AbbreviatedValue
    };

    public static AuditEventDto RegistrationBeganEvent(string token, string tenantId, DateTime performedAt, ApplicationPublicKey applicationPublicKey) => new()
    {
        Message = $"Beginning passkey registration for token: {string.Join("***", token.GetLast(4))}",
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

    public static AuditEventDto DeleteCredentialEvent(string performedBy, DateTime performedAt, string tenantId, ApplicationSecretKey apiSecret) => new()
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

    public static AuditEventDto UserSignInTokenVerifiedEvent(string performedBy, DateTime performedAt, string tenantId, ApplicationSecretKey applicationSecretKey) => new()
    {
        PerformedAt = performedAt,
        Message = $"User {performedBy} verified sign in token.",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = AuditEventType.ApiUserSignInCompleted,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = applicationSecretKey.AbbreviatedValue
    };
}