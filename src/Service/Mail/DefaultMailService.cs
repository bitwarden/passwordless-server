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
    
    public async Task SendApplicationDeletedAsync(AccountMetaInformation accountInformation, string deletedBy)
    {
        MailMessage message = new()
        {
            To = accountInformation.AdminEmails,
            From = _fromEmail,
            Subject = $"Your app '{accountInformation.AcountName}' has been deleted.",
            TextBody =
                $"Your app '{accountInformation.AcountName}' has been deleted at {accountInformation.DeleteAt:F} by '{deletedBy}'.",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{accountInformation.AcountName}' has been deleted.</title>
                  </head>
                  <body>
                    <p>Your app '{accountInformation.AcountName}' has been deleted at {accountInformation.DeleteAt:F} by '{deletedBy}'.</p>
                  </body>
                </html>
                """,
            Tag = "app-deleted"
        };

        await _provider.SendAsync(message);
    }

    public async Task SendApplicationToBeDeletedAsync(AccountMetaInformation accountInformation, string deletedBy)
    {
        MailMessage message = new()
        {
            To = accountInformation.AdminEmails,
            From = _fromEmail,
            Subject = $"Your app '{accountInformation.AcountName}' is scheduled for deletion.",
            TextBody =
                $"Your app '{accountInformation.AcountName}' is scheduled for deletion at {accountInformation.DeleteAt:F} by '{deletedBy}'.",
            HtmlBody =
                $"""
                <!doctype html>
                <html lang="en">
                  <head>
                    <meta charset="utf-8">
                    <title>Your app '{accountInformation.AcountName}' is scheduled for deletion.</title>
                  </head>
                  <body>
                    <p>Your app '{accountInformation.AcountName}' is scheduled for deletion at {accountInformation.DeleteAt:F} by '{deletedBy}'.</p>
                  </body>
                </html>
                """,
            Tag = "app-to-be-deleted"
        };

        await _provider.SendAsync(message);
    }
}