using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.Services.Mail;

namespace Passwordless.AdminConsole.Services.Mail;

public class DefaultMailService : IMailService
{
    private readonly string? _fromEmail;
    private readonly IMailProvider _provider;

    public DefaultMailService(
        IConfiguration configuration,
        IMailProvider provider)
    {
        _provider = provider;
        IConfigurationSection mailOptions = configuration.GetSection("Mail");
        _fromEmail = mailOptions.GetValue<string>("From") ?? null;
    }

    public async Task SendPasswordlessSignInAsync(string magicLink, string email)
    {
        MailMessage message = new()
        {
            To = new List<string> { email },
            From = _fromEmail,
            Subject = "Verify your passwordless.dev account",
            TextBody =
                $"Please click the link below to verify your account: {magicLink}",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Verify your Passwordless.dev account</title>
                  </head>
                  <body>
                    <p>Please click the link below to verify your account: <a href="{magicLink}">Verify</a></p>
                    <br />
                    <br />
                    <p>In case the link above doesn't work, please copy and paste the link below into your browser's address bar:</p>
                    <p>{magicLink}</p>
                  </body>
                </html>
                """,
            Tag = "verify-account"
        };

        await _provider.SendAsync(message);
    }

    public Task SendInviteAsync(Invite inv, string link)
    {
        var message = new MailMessage
        {
            To = new List<string> { inv.ToEmail },
            From = _fromEmail,
            Subject = "You've been invited to join an organization in passwordless.dev",
            TextBody =
                $"""You've been invited to join {inv.TargetOrgName} on passwordless.dev. {Environment.NewLine}The invite was sent by '{inv.FromName}' ({inv.FromEmail}).{Environment.NewLine}Please click the link to accept the invitation: {link}""",
            Tag = "admin-invite"
        };

        return _provider.SendAsync(message);
    }

    public Task SendEmailIsAlreadyInUseAsync(string email)
    {
        var message = new MailMessage
        {
            To = new List<string> { email },
            From = _fromEmail,
            Subject = "Your email is already connected to an organization",
            TextBody =
                $"""You recently tried to sign up or join an organization. We regret to inform you that your email was already connected to an organization. Please use a unique email address to sign up or join an organization. """,
            Tag = "duplicate-email"
        };

        return _provider.SendAsync(message);
    }

    public Task SendOrganizationDeletedAsync(string organizationName, IEnumerable<string> emails, string deletedBy, DateTime deletedAt)
    {
        var message = new MailMessage
        {
            To = emails,
            From = _fromEmail,
            Subject = $"Your organization '{organizationName}' was deleted.",
            TextBody =
                $"""Your organization '{organizationName}' was deleted by {deletedBy} at {deletedAt:F} UTC.""",
            Tag = "organization-deleted"
        };

        return _provider.SendAsync(message);
    }



    public async Task SendApplicationDeletedAsync(Application application, DateTime deletedAt, string deletedBy, ICollection<string> emails)
    {
        MailMessage message = new()
        {
            To = emails,
            From = _fromEmail,
            Bcc = new List<string> { "account-deletion@passwordless.dev" },
            Subject = $"Your app '{application.Name}' has been deleted.",
            TextBody =
                $"Your app '{application.Name}' has been deleted at {deletedAt:F} UTC by '{deletedBy}'.",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{application.Name}' has been deleted.</title>
                  </head>
                  <body>
                    <p>Your app '{application.Name}' has been deleted at {deletedAt:F} UTC by '{deletedBy}'.</p>
                  </body>
                </html>
                """,
            Tag = "app-deleted"
        };

        await _provider.SendAsync(message);
    }

    public async Task SendApplicationToBeDeletedAsync(Application application, string deletedBy, string cancellationLink, ICollection<string> emails)
    {
        MailMessage message = new()
        {
            To = emails,
            Bcc = new List<string> { "account-deletion@passwordless.dev" },
            From = _fromEmail,
            Subject = $"Your app '{application.Name}' is scheduled for deletion in 30 days.",
            TextBody =
                $"Your app '{application.Name}' is scheduled for deletion at {application.DeleteAt:F} UTC by '{deletedBy}'. If this was unintentional, please visit the your administration console: {cancellationLink}.",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{application.Name}' is scheduled for deletion in 30 days.</title>
                  </head>
                  <body>
                    <p>Your app '{application.Name}' is scheduled for deletion at {application.DeleteAt:F} UTC by '{deletedBy}'.</p>
                    <p>If this was unintentional, please visit the your administration console <a href="{cancellationLink}">this link</a></p>
                  </body>
                </html>
                """,
            Tag = "app-to-be-deleted"
        };

        await _provider.SendAsync(message);
    }
}