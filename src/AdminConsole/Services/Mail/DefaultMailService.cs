using System.Net;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.Services.Mail;
using Passwordless.Common.Validation;

namespace Passwordless.AdminConsole.Services.Mail;

public class DefaultMailService : IMailService
{
    private readonly IMailProvider _provider;

    public DefaultMailService(
        IMailProvider provider)
    {
        _provider = provider;
    }

    public async Task SendInviteAsync(Invite inv, string link)
    {
        var organizationDisplayName = WebUtility.HtmlEncode(inv.TargetOrgName);
        var invitedByDisplayName = WebUtility.HtmlEncode(inv.FromName);

        var message = new MailMessage
        {
            To = [inv.ToEmail],
            Subject = "You've been invited to join an organization in passwordless.dev",
            TextBody =
                $"""
                You've been invited to join {organizationDisplayName} on passwordless.dev.
                The invite was sent by '{invitedByDisplayName}' ({inv.FromEmail}).
                Please click the link to accept the invitation: {link}
                """,
            HtmlBody =
                $"""
                 You've been invited to join {organizationDisplayName} on passwordless.dev.
                 The invite was sent by '{invitedByDisplayName}' ({inv.FromEmail}).
                 Please click the link to accept the invitation: {link}
                 """,
            Tag = "admin-invite"
        };

        await _provider.SendAsync(message);
    }

    public async Task SendEmailIsAlreadyInUseAsync(string email)
    {
        var message = new MailMessage
        {
            To = [email],
            Subject = "Your e-mail is already connected to an organization",
            TextBody =
                """
                You recently tried to sign up or join an organization.
                We regret to inform you that your e-mail was already connected to an organization.
                Please use a unique e-mail address to sign up or join an organization.
                """,
            HtmlBody =
                """
                You recently tried to sign up or join an organization.
                We regret to inform you that your e-mail was already connected to an organization.
                Please use a unique e-mail address to sign up or join an organization.
                """,
            Tag = "duplicate-email"
        };

        await _provider.SendAsync(message);
    }

    public Task SendMagicLinksDisabledAsync(string organizationName, string email)
    {
        if (!EmailValidator.Validate(email))
        {
            throw new ArgumentException("The e-mail address provided is invalid.", nameof(email));
        }

        var organizationDisplayName = WebUtility.HtmlEncode(organizationName);

        var message = new MailMessage
        {
            To = [email],
            Subject = $"Magic links have been disabled for '{organizationDisplayName}'",
            TextBody =
                $"""
                Magic links have been disabled for '{organizationDisplayName}'.
                Please contact your organization administrator for more information if you have lost your credentials.
                """,
            HtmlBody =
                $"""
                  Magic links have been disabled for '{organizationDisplayName}'.
                  Please contact your organization administrator for more information if you have lost your credentials.
                  """,
            Tag = "magic-links-disabled"
        };

        return _provider.SendAsync(message);
    }

    public async Task SendOrganizationDeletedAsync(string organizationName, IEnumerable<string> emails, string deletedBy, DateTime deletedAt)
    {
        var organizationDisplayName = WebUtility.HtmlEncode(organizationName);

        var message = new MailMessage
        {
            To = emails,
            Subject = $"Your organization '{organizationDisplayName}' has been deleted.",
            TextBody =
                $"""
                Your organization '{organizationDisplayName}' has been deleted on {deletedAt:D} at {deletedAt:T} UTC by '{deletedBy}'.
                """,
            HtmlBody =
                $"""
                 Your organization '{organizationDisplayName}' has been deleted on {deletedAt:D} at {deletedAt:T} UTC by '{deletedBy}'.
                 """,
            Tag = "organization-deleted"
        };

        await _provider.SendAsync(message);
    }

    public async Task SendApplicationDeletedAsync(Application application, DateTime deletedAt, string deletedBy, ICollection<string> emails)
    {
        var applicationDisplayName = WebUtility.HtmlEncode(application.Name);

        var message = new MailMessage
        {
            To = emails,
            Bcc = ["account-deletion@passwordless.dev"],
            Subject = $"Your app '{applicationDisplayName}' has been deleted.",
            TextBody =
                $"""
                Your app '{applicationDisplayName}' has been deleted on {deletedAt:D} at {deletedAt:T} UTC by '{deletedBy}'.
                """,
            HtmlBody =
                // lang=html
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{applicationDisplayName}' has been deleted.</title>
                  </head>
                  <body>
                    <p>Your app '{applicationDisplayName}' has been deleted on {deletedAt:D} at {deletedAt:T} UTC by '{deletedBy}'.</p>
                  </body>
                </html>
                """,
            Tag = "app-deleted"
        };

        await _provider.SendAsync(message);
    }

    public async Task SendApplicationToBeDeletedAsync(Application application, string deletedBy, string cancellationLink, ICollection<string> emails)
    {
        var applicationDisplayName = WebUtility.HtmlEncode(application.Name);

        var message = new MailMessage
        {
            To = emails,
            Bcc = ["account-deletion@passwordless.dev"],
            Subject = $"Your app '{applicationDisplayName}' is scheduled for deletion in 30 days.",
            TextBody =
                $"""
                Your app '{applicationDisplayName}' is scheduled for deletion on {application.DeleteAt:D} at {application.DeleteAt:T} UTC by '{deletedBy}'.
                If this was unintentional, please visit your administration console: {cancellationLink}.
                """,
            HtmlBody =
                // lang=html
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{applicationDisplayName}' is scheduled for deletion in 30 days.</title>
                  </head>
                  <body>
                    <p>Your app '{applicationDisplayName}' is scheduled for deletion on {application.DeleteAt:D} at {application.DeleteAt:T} UTC by '{deletedBy}'.</p>
                    <p>If this was unintentional, please <a href="{cancellationLink}">visit your administration console</a></p>
                  </body>
                </html>
                """,
            Tag = "app-to-be-deleted"
        };

        await _provider.SendAsync(message);
    }
}