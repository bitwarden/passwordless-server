using Passwordless.Common.Services.Mail;
using Passwordless.Service.Helpers;
using Passwordless.Service.MagicLinks.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.MagicLinks;

public class MagicLinkService(
    TimeProvider timeProvider,
    ITenantStorage tenantStorage,
    IFido2Service fido2Service,
    IMailProvider mailProvider)
{
    // Cache last 30 days of dispatched emails in memory, so we don't have to query the database every time
    // TODO: this needs to be shared between instances of this service per tenant
    private List<DispatchedEmail>? _dispatchedEmails;

    private async Task<IReadOnlyList<DispatchedEmail>> GetDispatchedEmailsAsync(TimeSpan? duration = null)
    {
        _dispatchedEmails ??=
        [
            ..await tenantStorage.GetDispatchedEmailsAsync(duration ?? TimeSpan.FromDays(30))
        ];

        return _dispatchedEmails
            // Filter again because the in-memory cache may contain more than 30 days of emails
            .Where(e => e.CreatedAt > timeProvider.GetUtcNow() - (duration ?? TimeSpan.FromDays(30)))
            .ToArray();
    }

    private async Task<int> GetDispatchedEmailCountAsync(TimeSpan? duration = null) =>
        (await GetDispatchedEmailsAsync(duration)).Count;

    private async Task EnforceLimitsAsync(MagicLinkDTO dto)
    {
        var now = timeProvider.GetUtcNow();
        var account = await tenantStorage.GetAccountInformation();
        var accountAge = (now - account.CreatedAt).Duration();

        // Applications created less than 24 hours ago can only send magic links to the admin email address
        if (accountAge < TimeSpan.FromHours(24) &&
            !account.AdminEmails.Contains(dto.EmailAddress.Address, StringComparer.OrdinalIgnoreCase))
        {
            throw new ApiException(
                "Because your application has been created less than 24 hours ago, " +
                "you can only request magic links to the admin email address.",
                403
            );
        }

        var maxMonthlyLimit = account.Features?.MaxMagicLinkEmailMonthlyLimit ?? 500;
        var monthlyLimit = accountAge.TotalDays switch
        {
            // App created <24 hours ago
            < 1 => 0.2 * maxMonthlyLimit,
            // App created <3 days ago
            < 3 => 0.5 * maxMonthlyLimit,
            // App created <30 days ago
            < 30 => 0.75 * maxMonthlyLimit,
            // App created >30 days ago
            _ => maxMonthlyLimit
        };

        if (await GetDispatchedEmailCountAsync(TimeSpan.FromDays(30)) >= (int)Math.Max(1, monthlyLimit))
        {
            throw new ApiException(
                "You have reached your monthly quota for magic link emails. " +
                "Please try again later.",
                429
            );
        }

        var maxMinutelyLimit = account.Features?.MaxMagicLinkEmailMinutelyLimit ?? 5;
        var minutelyLimit = accountAge.TotalDays switch
        {
            // App created <24 hours ago
            < 1 => 0.2 * maxMinutelyLimit,
            // App created <3 days ago
            < 3 => 0.5 * maxMinutelyLimit,
            // App created <30 days ago
            < 30 => 0.75 * maxMinutelyLimit,
            // App created >30 days ago
            _ => maxMinutelyLimit
        };

        if (await GetDispatchedEmailCountAsync(TimeSpan.FromMinutes(1)) >= (int)Math.Max(1, minutelyLimit))
        {
            throw new ApiException(
                "You have reached your rate limit for magic link emails. " +
                "Please try again later.",
                429
            );
        }
    }

    public async Task SendMagicLinkAsync(MagicLinkDTO dto)
    {
        await EnforceLimitsAsync(dto);

        var token = await fido2Service.CreateSigninToken(new SigninTokenRequest(dto.UserId));
        var link = new Uri(dto.LinkTemplate.Replace("<token>", token));

        await mailProvider.SendAsync(new MailMessage
        {
            To = [dto.EmailAddress.ToString()],
            Subject = "Verify Email Address",
            TextBody = $"Please click the link or copy into your browser of choice to log in: {link}. If you did not request this email, it is safe to ignore.",
            HtmlBody =
                //lang=html
                $"""
                 <!DOCTYPE html>
                 <html lang="en">
                    <head>
                        <title>Sign-In With Link Request</title>
                    </head>
                    <body style="background-color: white;font-family: DM Sans,system-ui,-apple-system,BlinkMacSystemFont,Segoe UI,Helvetica Neue,sans-serif;font-size: 1rem;">
                        <h3 style="text-align: center;">Hello</h3>
                        <p style="text-align: center;">Please click the button below to log in.</p>
                        <a href="{link}" style="width: 2.5rem; margin-left: auto; margin-right: auto; padding: .75rem 1.5rem; background-color: rgb(18 82 163); border-radius: 999px; color: white; text-align: center; text-decoration: none; display: block; cursor: pointer;">Login</a>
                        <p>Having trouble? Copy this link to your browser: {link}</p>
                        <p style="text-align: center;">If you did not request this email, it is safe to ignore.</p>
                    </body>
                 </html>
                 """,
            MessageType = "magic-links"
        });

        var email = await tenantStorage.AddDispatchedEmailAsync(dto.UserId, dto.EmailAddress.Address);
        _dispatchedEmails?.Add(email);
    }
}