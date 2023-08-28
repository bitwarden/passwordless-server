using System.Collections.Specialized;
using System.Web;
using AdminConsole.Identity;
using AdminConsole.Services.Mail;
using Passwordless.Common.Services.Mail;

namespace Passwordless.AdminConsole.Services.Mail;

public class DefaultMailService : IMailService
{
    private readonly string? _fromEmail;
    private readonly IMailProvider _provider;

    public DefaultMailService(IConfiguration configuration, IMailProvider provider)
    {
        _provider = provider;
        IConfigurationSection mailOptions = configuration.GetSection("Mail");
        _fromEmail = mailOptions.GetValue<string>("From") ?? null;
    }

    public async Task SendPasswordlessSignInAsync(string returnUrl, string token, string email)
    {
        UriBuilder uriBuilder = new(returnUrl);
        NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["token"] = token;
        query["email"] = email;
        uriBuilder.Query = query.ToString();
        MailMessage message = new()
        {
            To = new List<string> { email },
            From = _fromEmail,
            Subject = "Verify your passwordless.dev account",
            TextBody =
                $"Please click the link below to verify your account: {uriBuilder}",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Verify your Passwordless.dev account</title>
                  </head>
                  <body>
                    <p>Please click the link below to verify your account: <a href="{uriBuilder}">Verify</a></p>
                    <br />
                    <br />
                    <p>In case the link above doesn't work, please copy and paste the link below into your browser's address bar:</p>
                    <p>{uriBuilder}</p>
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
            Subject = "You're email is already connected to an organization",
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
}