using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Models;
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

    public static void LogUserAliasSetEvent(
        this IEventLogger logger,
        string performedBy,
        string tenantId,
        DateTime performedAt,
        ApplicationSecretKey applicationSecretKey) =>
        logger.LogEvent(new EventDto
        {
            Message = $"Added set aliases for user ({performedBy}).",
            PerformedAt = performedAt,
            PerformedBy = performedBy,
            TenantId = tenantId,
            EventType = EventType.ApiUserSetAliases,
            Severity = Severity.Informational,
            Subject = performedBy,
            ApiKeyId = applicationSecretKey.AbbreviatedValue
        });

    public static void LogApplicationCreatedEvent(
        this IEventLogger logger,
        string performedBy,
        string tenantId,
        DateTime performedAt) =>
        logger.LogEvent(new EventDto
        {
            PerformedAt = performedAt,
            Message = $"{tenantId} created.",
            PerformedBy = performedBy,
            TenantId = tenantId,
            EventType = EventType.ApiManagementAppCreated,
            Severity = Severity.Informational,
            Subject = tenantId,
            ApiKeyId = ""
        });

    public static void LogAppFrozenEvent(this IEventLogger logger, string tenantId, DateTime performedAt) =>
        logger.LogEvent(new EventDto
        {
            PerformedAt = performedAt,
            Message = "Application frozen.",
            PerformedBy = "System",
            TenantId = tenantId,
            EventType = EventType.ApiManagementAppFrozen,
            Severity = Severity.Alert,
            Subject = tenantId,
            ApiKeyId = string.Empty
        });

    public static void LogAppUnfrozenEvent(this IEventLogger logger, string tenantId, DateTime performedAt) =>
        logger.LogEvent(new EventDto
        {
            PerformedAt = performedAt,
            Message = "Application unfrozen.",
            PerformedBy = "System",
            TenantId = tenantId,
            EventType = EventType.ApiManagementAppUnfrozen,
            Severity = Severity.Alert,
            Subject = tenantId,
            ApiKeyId = string.Empty
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
            ApiKeyId = string.Empty
        });

    public static void LogAppDeleteCancelledEvent(this IEventLogger logger, string tenantId, DateTime performedAt) =>
        logger.LogEvent(new EventDto
        {
            PerformedAt = performedAt,
            Message = "Application deletion was cancelled.",
            PerformedBy = "System",
            TenantId = tenantId,
            EventType = EventType.ApiManagementAppDeletionCancelled,
            Severity = Severity.Informational,
            Subject = tenantId,
            ApiKeyId = string.Empty
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