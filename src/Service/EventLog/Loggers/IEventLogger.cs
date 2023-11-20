using Passwordless.Common.EventLog.Enums;
using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog.Loggers;

public interface IEventLogger
{
    void LogEvent(EventDto @event);
    void LogEvent(Func<IEventLogContext, EventDto> eventFunc);
    Task FlushAsync();
}

public static class EventLoggerExtensions
{
    public static void LogRegistrationTokenCreatedEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            Message = $"Created registration token for {performedBy}",
            Severity = Severity.Informational,
            EventType = EventType.ApiAuthUserRegistered,
            PerformedAt = context.PerformedAt,
            PerformedBy = performedBy,
            Subject = context.TenantId,
            TenantId = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogSigninTokenCreatedEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            Message = $"Created signin token for {performedBy}",
            Severity = Severity.Informational,
            EventType = EventType.ApiUserSigninTokenCreated,
            PerformedAt = context.PerformedAt,
            PerformedBy = performedBy,
            Subject = context.TenantId,
            TenantId = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogRegistrationBeganEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            Message = $"Beginning passkey registration for {performedBy}",
            PerformedBy = performedBy,
            PerformedAt = context.PerformedAt,
            EventType = EventType.ApiAuthPasskeyRegistrationBegan,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            TenantId = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogRegistrationCompletedEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            Message = $"Completed passkey registration for user {performedBy}",
            PerformedBy = performedBy,
            PerformedAt = context.PerformedAt,
            EventType = EventType.ApiAuthPasskeyRegistrationCompleted,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            TenantId = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogUserAliasSetEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            Message = $"Added set aliases for user ({performedBy}).",
            PerformedAt = context.PerformedAt,
            PerformedBy = performedBy,
            TenantId = context.TenantId,
            EventType = EventType.ApiUserSetAliases,
            Severity = Severity.Informational,
            Subject = performedBy,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogApplicationCreatedEvent(this IEventLogger logger, string performedBy) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"{context.TenantId} created.",
            PerformedBy = performedBy,
            TenantId = context.TenantId,
            EventType = EventType.ApiManagementAppCreated,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogAppFrozenEvent(this IEventLogger logger) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = "Application frozen.",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = EventType.ApiManagementAppFrozen,
            Severity = Severity.Alert,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogAppUnfrozenEvent(this IEventLogger logger) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = "Application unfrozen.",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = EventType.ApiManagementAppUnfrozen,
            Severity = Severity.Alert,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogAppMarkedToDeleteEvent(this IEventLogger logger, string deletedBy) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = "Application was marked for deletion.",
            PerformedBy = deletedBy,
            TenantId = context.TenantId,
            EventType = EventType.ApiManagementAppMarkedForDeletion,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

    public static void LogAppDeleteCancelledEvent(this IEventLogger logger) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = "Application deletion was cancelled.",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = EventType.ApiManagementAppDeletionCancelled,
            Severity = Severity.Informational,
            Subject = context.TenantId,
            ApiKeyId = context.AbbreviatedKey
        });

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

    public static void LogDeletedUserEvent(this IEventLogger logger, string userId) =>
        logger.LogEvent(context => new EventDto
        {
            PerformedAt = context.PerformedAt,
            Message = $"Deleted user {userId}",
            PerformedBy = "System",
            TenantId = context.TenantId,
            EventType = EventType.ApiUserDeleted,
            Severity = Severity.Informational,
            Subject = userId,
            ApiKeyId = context.AbbreviatedKey
        });
}