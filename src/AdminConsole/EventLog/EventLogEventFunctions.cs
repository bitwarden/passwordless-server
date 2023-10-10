using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Models;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.EventLog.Enums;

namespace Passwordless.AdminConsole.EventLog;

public static class EventLogEventFunctions
{
    public static OrganizationEventDto CreateOrganizationCreatedEvent(Organization organization, ConsoleAdmin performedBy) =>
        new(performedBy.Id,
            EventType.AdminOrganizationCreated,
            $"Organization {organization.Name} created by {performedBy.Name}.",
            Severity.Informational,
            organization.Id.ToString(),
            organization.Id,
            DateTime.UtcNow);

    public static OrganizationEventDto CreateLoginViaMagicLinkEvent(ConsoleAdmin user) =>
        new("System",
            EventType.AdminMagicLinkLogin,
            $"Login email sent for {user.Id} to {user.Email}.",
            Severity.Informational,
            user.Id,
            user.OrganizationId,
            DateTime.UtcNow);

    public static OrganizationEventDto DeleteAdminEvent(ConsoleAdmin performedBy, ConsoleAdmin deletedAdmin, DateTime performedAt) =>
        new(performedBy.Id,
            EventType.AdminDeleteAdmin,
            $"Deleted admin {deletedAdmin.Name}",
            Severity.Informational,
            deletedAdmin.Id,
            performedBy.OrganizationId,
            performedAt);

    public static OrganizationEventDto InviteAdminEvent(ConsoleAdmin performedBy, string invitedEmail, DateTime performedAt) =>
        new(performedBy.Id,
            EventType.AdminSendAdminInvite,
            $"Sent admin invite to {invitedEmail}",
            Severity.Informational,
            performedBy.OrganizationId.ToString(),
            performedBy.OrganizationId,
            performedAt);

    public static OrganizationEventDto CancelAdminInviteEvent(ConsoleAdmin performedBy, string invitedEmail, DateTime performedAt) =>
        new(performedBy.Id,
            EventType.AdminCancelAdminInvite,
            $"Cancel admin invite for {invitedEmail}",
            Severity.Informational,
            performedBy.OrganizationId.ToString(),
            performedBy.OrganizationId,
            performedAt);

    public static OrganizationEventDto AdminInvalidInviteUsedEvent(Invite invite, DateTime performedAt) =>
        new(invite.ToEmail,
            EventType.AdminInvalidInviteUsed,
            "Expired invite used.",
            Severity.Warning,
            invite.ToEmail,
            invite.TargetOrgId,
            performedAt);

    public static OrganizationEventDto AdminAcceptedInviteEvent(Invite invite, ConsoleAdmin consoleAdmin, DateTime performedAt) =>
        new(consoleAdmin.Id,
            EventType.AdminAcceptedInvite,
            $"{consoleAdmin.Name} accepted invite sent to {invite.ToEmail}.",
            Severity.Informational,
            consoleAdmin.OrganizationId.ToString(),
            consoleAdmin.OrganizationId,
            performedAt);

    public static OrganizationEvent ToNewEvent(this OrganizationEventDto dto) =>
        new()
        {
            Id = Guid.NewGuid(),
            PerformedAt = dto.PerformedAt,
            EventType = dto.EventType,
            Message = dto.Message,
            Severity = dto.Severity,
            PerformedBy = dto.PerformedBy,
            Subject = dto.Subject,
            OrganizationId = dto.OrganizationId
        };

    public static OrganizationEventDto ToDto(this OrganizationEvent @event) =>
        new(@event.PerformedBy,
            @event.EventType,
            @event.Message,
            @event.Severity,
            @event.Subject,
            @event.OrganizationId,
            @event.PerformedAt);

    public static EventLogEvent ToResponse(this OrganizationEventDto dto) =>
        new(dto.PerformedAt,
            dto.EventType.ToString(),
            dto.Message,
            dto.Severity.ToString(),
            dto.PerformedBy,
            dto.Subject,
            string.Empty);
}