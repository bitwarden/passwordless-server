using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Models;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog;

public static class EventFunctions
{
    public static EventDto RegistrationTokenCreatedEvent(string performedBy, IEventLogContext eventLogContext) => new()
    {
        Message = $"Created registration token for {performedBy}",
        Severity = Severity.Informational,
        EventType = EventType.ApiAuthUserRegistered,
        PerformedAt = eventLogContext.PerformedAt,
        PerformedBy = performedBy,
        Subject = eventLogContext.TenantId,
        TenantId = eventLogContext.TenantId,
        ApiKeyId = eventLogContext.AbbreviatedKey
    };

    public static EventDto RegistrationBeganEvent(string performedBy, IEventLogContext eventLogContext) => new()
    {
        Message = $"Beginning passkey registration for {performedBy}",
        PerformedBy = performedBy,
        PerformedAt = eventLogContext.PerformedAt,
        EventType = EventType.ApiAuthPasskeyRegistrationBegan,
        Severity = Severity.Informational,
        Subject = eventLogContext.TenantId,
        TenantId = eventLogContext.TenantId,
        ApiKeyId = eventLogContext.AbbreviatedKey
    };

    public static EventDto RegistrationCompletedEvent(string performedBy, IEventLogContext eventLogContext) => new()
    {
        Message = $"Completed passkey registration for user {performedBy}",
        PerformedBy = performedBy,
        PerformedAt = eventLogContext.PerformedAt,
        EventType = EventType.ApiAuthPasskeyRegistrationCompleted,
        Severity = Severity.Informational,
        Subject = eventLogContext.TenantId,
        TenantId = eventLogContext.TenantId,
        ApiKeyId = eventLogContext.AbbreviatedKey
    };

    public static EventDto UserAliasSetEvent(string performedBy, string tenantId, DateTime performedAt, ApplicationSecretKey applicationSecretKey) => new()
    {
        Message = $"Added set aliases for user ({performedBy}).",
        PerformedAt = performedAt,
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = EventType.ApiUserSetAliases,
        Severity = Severity.Informational,
        Subject = performedBy,
        ApiKeyId = applicationSecretKey.AbbreviatedValue
    };

    public static EventDto ApplicationCreatedEvent(string performedBy, string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = $"{tenantId} created.",
        PerformedBy = performedBy,
        TenantId = tenantId,
        EventType = EventType.ApiManagementAppCreated,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = ""
    };

    public static EventDto AppFrozenEvent(string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application frozen.",
        PerformedBy = "System",
        TenantId = tenantId,
        EventType = EventType.ApiManagementAppFrozen,
        Severity = Severity.Alert,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static EventDto AppUnfrozenEvent(string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application unfrozen.",
        PerformedBy = "System",
        TenantId = tenantId,
        EventType = EventType.ApiManagementAppUnfrozen,
        Severity = Severity.Alert,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static Func<IEventLogContext, EventDto> AppMarkedToDeleteEvent(string deletedBy) =>
        context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = "Application was marked for deletion.",
            PerformedBy = deletedBy,
            TenantId = context.TenantId,
            EventType = EventType.ApiManagementAppMarkedForDeletion,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            ApiKeyId = string.Empty
        };

    public static EventDto AppDeleteCancelledEvent(string tenantId, DateTime performedAt) => new()
    {
        PerformedAt = performedAt,
        Message = "Application deletion was cancelled.",
        PerformedBy = "System",
        TenantId = tenantId,
        EventType = EventType.ApiManagementAppDeletionCancelled,
        Severity = Severity.Informational,
        Subject = tenantId,
        ApiKeyId = string.Empty
    };

    public static void LogDeleteCredentialEvent(this IEventLogger logger, string userId) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"Deleted credential for user {userId}",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = EventType.ApiUserDeleteCredential,
            Severity = Severity.Informational,
            Subject = userId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogUserSignInCompletedEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"User {performedBy} completed sign in.",
            PerformedBy = performedBy,
            TenantId = context.TenantId,
            EventType = EventType.ApiUserSignInCompleted,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogUserSignInTokenVerifiedEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"User {performedBy} verified sign in token.",
            PerformedBy = performedBy,
            TenantId = context.TenantId,
            EventType = EventType.ApiUserSignInVerified,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static Func<IEventLogContext, EventDto> DeletedUserEvent(string userId) =>
        context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"Deleted user {userId}",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = EventType.ApiUserDeleted,
            Severity = Severity.Informational,
            Subject = userId,
            ApiKeyId = context.AbbreviatedKey
        };
}