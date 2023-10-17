using System.Security.Claims;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.EventLog.Enums;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public interface IEventLogger
{
    void LogEvent(OrganizationEventDto @event);
    Task FlushAsync();
}

public static class EventLoggerExtensions
{
    public static void LogCreateOrganizationCreatedEvent(
        this IEventLogger logger,
        Organization organization,
        ConsoleAdmin performedBy) =>
        logger.LogEvent(
            new(performedBy.Id,
                EventType.AdminOrganizationCreated,
                $"Organization {organization.Name} created by {performedBy.Name}.",
                Severity.Informational,
                organization.Id.ToString(),
                organization.Id,
                DateTime.UtcNow
            )
        );

    public static void LogCreateLoginViaMagicLinkEvent(this IEventLogger logger, ConsoleAdmin user) =>
        logger.LogEvent(
            new("System",
                EventType.AdminMagicLinkLogin,
                $"Login email sent for {user.Id} to {user.Email}.",
                Severity.Informational,
                user.Id,
                user.OrganizationId,
                DateTime.UtcNow
            )
        );

    public static void LogDeleteAdminEvent(
        this IEventLogger logger,
        ConsoleAdmin performedBy,
        ConsoleAdmin deletedAdmin,
        DateTime performedAt) =>
        logger.LogEvent(
            new(performedBy.Id,
                EventType.AdminDeleteAdmin,
                $"Deleted admin {deletedAdmin.Name}",
                Severity.Informational,
                deletedAdmin.Id,
                performedBy.OrganizationId,
                performedAt
            )
        );

    public static void LogInviteAdminEvent(
        this IEventLogger logger,
        ConsoleAdmin performedBy,
        string invitedEmail,
        DateTime performedAt) =>
        logger.LogEvent(
            new(performedBy.Id,
                EventType.AdminSendAdminInvite,
                $"Sent admin invite to {invitedEmail}",
                Severity.Informational,
                performedBy.OrganizationId.ToString(),
                performedBy.OrganizationId,
                performedAt
            )
        );

    public static void LogCancelAdminInviteEvent(
        this IEventLogger logger,
        ConsoleAdmin performedBy,
        string invitedEmail,
        DateTime performedAt) =>
        logger.LogEvent(
            new(performedBy.Id,
                EventType.AdminCancelAdminInvite,
                $"Cancel admin invite for {invitedEmail}",
                Severity.Informational,
                performedBy.OrganizationId.ToString(),
                performedBy.OrganizationId,
                performedAt
            )
        );

    public static void LogAdminInvalidInviteUsedEvent(
        this IEventLogger logger,
        Invite invite,
        DateTime performedAt) =>
        logger.LogEvent(
            new(invite.ToEmail,
                EventType.AdminInvalidInviteUsed,
                "Expired invite used.",
                Severity.Warning,
                invite.ToEmail,
                invite.TargetOrgId,
                performedAt
            )
        );

    public static void LogAdminAcceptedInviteEvent(
        this IEventLogger logger,
        Invite invite,
        ConsoleAdmin consoleAdmin,
        DateTime performedAt) =>
        logger.LogEvent(
            new(consoleAdmin.Id,
                EventType.AdminAcceptedInvite,
                $"{consoleAdmin.Name} accepted invite sent to {invite.ToEmail}.",
                Severity.Informational,
                consoleAdmin.OrganizationId.ToString(),
                consoleAdmin.OrganizationId,
                performedAt
            )
        );

    public static void LogAdminLoginTokenVerifiedEvent(
        this IEventLogger logger,
        ClaimsPrincipal claimsPrincipal,
        DateTime performedAt) =>
        logger.LogEvent(
            new(claimsPrincipal.GetId(),
                EventType.AdminSignInTokenVerified,
                $"{claimsPrincipal.GetName()} successfully signed-in via passkey.",
                Severity.Informational,
                claimsPrincipal.GetOrgId()?.ToString() ?? string.Empty,
                claimsPrincipal.GetOrgId().GetValueOrDefault(),
                performedAt
            )
        );
}