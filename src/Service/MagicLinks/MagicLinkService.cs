using Microsoft.Extensions.Internal;
using Passwordless.Common.Services.Mail;
using Passwordless.Service.Helpers;
using Passwordless.Service.MagicLinks.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.MagicLinks;

public class MagicLinkService(
    ISystemClock clock,
    ITenantStorage tenantStorage,
    IFido2Service fido2Service,
    IMailProvider mailProvider)
{
    private async Task EnsureQuotaAsync(MagicLinkDTO dto)
    {
        // Free: 50 emails/month, 50 emails/minute
        // Pro: 1000 emails/month, 100 emails/minute
        const int maxFreeMonthlyQuota = 50;
        const int maxFreePerMinuteRateLimit = 50;
        const int maxProMonthlyQuota = 1000;
        const int maxProPerMinuteRateLimit = 100;

        var now = clock.UtcNow;
        var account = await tenantStorage.GetAccountInformation();

        // App created <24 hours ago
        // Can only email the admin email address.
        // 20% of quota, 20% of rate limit
        if ((account.CreatedAt - now).Duration() < TimeSpan.FromHours(24))
        {
            if (!account.AdminEmails.Contains(dto.EmailAddress.Address, StringComparer.OrdinalIgnoreCase))
            {
                throw new ApiException(
                    "Because your application has been created less than 24 hours ago, " +
                    "you can only request magic links to the admin email address.",
                    403
                );
            }
        }

        // App created <3 days ago
        // 50% of quota, 50% of rate limit
        if ((account.CreatedAt - now).Duration() < TimeSpan.FromDays(3))
        {
        }

        // App created <30 days ago
        // 75% of quota, 75% of rate limit
        if ((account.CreatedAt - now).Duration() < TimeSpan.FromDays(30))
        {
        }

        // App created >30 days ago
        // 100% of quota, 100% of rate limit

    }

    public async Task SendMagicLinkAsync(MagicLinkDTO dto)
    {
        await EnsureQuotaAsync(dto);

        var token = await fido2Service.CreateSigninTokenAsync(new SigninTokenRequest(dto.UserId));
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
    }
}