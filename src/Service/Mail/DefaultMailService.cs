using Microsoft.Extensions.Configuration;
using Passwordless.Common.Services.Mail;
using Passwordless.Service.Models;

namespace Passwordless.Service.Mail;

public sealed class DefaultMailService : IMailService
{
    private readonly IMailProvider _provider;
    private readonly string _fromEmail;

    public DefaultMailService(IMailProvider provider, IConfiguration configuration)
    {
        _provider = provider;
        IConfigurationSection mailOptions = configuration.GetSection("Mail");
        _fromEmail = mailOptions.GetValue<string>("From") ?? null;
    }

    public async Task SendApplicationDeletedAsync(AccountMetaInformation accountInformation, DateTime deletedAt, string deletedBy)
    {
        MailMessage message = new()
        {
            To = accountInformation.AdminEmails,
            From = _fromEmail,
            Bcc = new List<string> { "account-deletion@passwordless.dev" },
            Subject = $"Your app '{accountInformation.AcountName}' has been deleted.",
            TextBody =
                $"Your app '{accountInformation.AcountName}' has been deleted at {deletedAt:F} UTC by '{deletedBy}'.",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{accountInformation.AcountName}' has been deleted.</title>
                  </head>
                  <body>
                    <p>Your app '{accountInformation.AcountName}' has been deleted at {deletedAt:F} UTC by '{deletedBy}'.</p>
                  </body>
                </html>
                """,
            Tag = "app-deleted"
        };

        await _provider.SendAsync(message);
    }

    public async Task SendApplicationToBeDeletedAsync(AccountMetaInformation accountInformation, DateTime deleteAt, string deletedBy, string cancellationLink)
    {
        MailMessage message = new()
        {
            To = accountInformation.AdminEmails,
            Bcc = new List<string> { "account-deletion@passwordless.dev" },
            From = _fromEmail,
            Subject = $"Your app '{accountInformation.AcountName}' is scheduled for deletion in 30 days.",
            TextBody =
                $"Your app '{accountInformation.AcountName}' is scheduled for deletion at {deleteAt:F} UTC by '{deletedBy}'.",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{accountInformation.AcountName}' is scheduled for deletion in 30 days.</title>
                  </head>
                  <body>
                    <p>Your app '{accountInformation.AcountName}' is scheduled for deletion at {deleteAt:F} UTC by '{deletedBy}'.</p>
                    <p>If this was unintentional, please visit the your administration console or click <a href="{cancellationLink}">this link</a></p>
                  </body>
                </html>
                """,
            Tag = "app-to-be-deleted"
        };

        await _provider.SendAsync(message);
    }
}