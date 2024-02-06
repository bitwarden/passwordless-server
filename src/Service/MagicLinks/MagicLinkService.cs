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
    private async Task EnforceLimitsAsync(MagicLinkDTO dto)
    {
        var now = timeProvider.GetUtcNow();
        var account = await tenantStorage.GetAccountInformation();
        var accountAge = (now - account.CreatedAt).Duration();
        var isFreeAccount = account.SubscriptionTier == "Free";

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

        const int maxFreeMonthlyLimit = 50;
        const int maxProMonthlyLimit = 1000;
        var monthlyLimit = accountAge.TotalDays switch
        {
            // App created <24 hours ago
            < 1 when isFreeAccount => (int)(0.2 * maxFreeMonthlyLimit),
            < 1 => (int)(0.2 * maxProMonthlyLimit),
            // App created <3 days ago
            < 3 when isFreeAccount => (int)(0.5 * maxFreeMonthlyLimit),
            < 3 => (int)(0.5 * maxProMonthlyLimit),
            // App created <30 days ago
            < 30 when isFreeAccount => (int)(0.75 * maxFreeMonthlyLimit),
            < 30 => (int)(0.75 * maxProMonthlyLimit),
            // App created >30 days ago
            _ when isFreeAccount => maxFreeMonthlyLimit,
            _ => maxProMonthlyLimit
        };

        if (await tenantStorage.GetDispatchedEmailCountAsync(TimeSpan.FromDays(30)) >= monthlyLimit)
        {
            throw new ApiException(
                "You have reached your monthly quota for magic link emails. " +
                "Please try again later.",
                429
            );
        }

        const int maxFreeMinutelyLimit = 50;
        const int maxProMinutelyLimit = 100;
        var minutelyLimit = accountAge.TotalDays switch
        {
            // App created <24 hours ago
            < 1 when isFreeAccount => (int)(0.2 * maxFreeMinutelyLimit),
            < 1 => (int)(0.2 * maxProMinutelyLimit),
            // App created <3 days ago
            < 3 when isFreeAccount => (int)(0.5 * maxFreeMinutelyLimit),
            < 3 => (int)(0.5 * maxProMinutelyLimit),
            // App created <30 days ago
            < 30 when isFreeAccount => (int)(0.75 * maxFreeMinutelyLimit),
            < 30 => (int)(0.75 * maxProMinutelyLimit),
            // App created >30 days ago
            _ when isFreeAccount => maxFreeMinutelyLimit,
            _ => maxProMinutelyLimit
        };

        if (await tenantStorage.GetDispatchedEmailCountAsync(TimeSpan.FromMinutes(1)) >= minutelyLimit)
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

        await tenantStorage.AddDispatchedEmailAsync(dto.UserId, dto.EmailAddress.Address);
    }
}