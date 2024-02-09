using Microsoft.Extensions.Caching.Memory;
using Passwordless.Common.Services.Mail;
using Passwordless.Service.Helpers;
using Passwordless.Service.MagicLinks.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.MagicLinks;

public class MagicLinkService(
    TimeProvider timeProvider,
    IMemoryCache cache,
    ITenantStorage tenantStorage,
    IFido2Service fido2Service,
    IMailProvider mailProvider)
{
    private readonly string _emailsSentCacheKey = $"magic-link-emails-sent-30days-{tenantStorage.Tenant}";

    private async Task EnforceQuotaAsync(MagicLinkDTO dto)
    {
        var now = timeProvider.GetUtcNow();
        var account = await tenantStorage.GetAccountInformation();
        var accountAge = now - account.CreatedAt;

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

        // Reduce the quota for newly created applications
        var coefficient = accountAge.TotalDays switch
        {
            // App created <24 hours ago
            < 1 => 0.2,
            // App created <3 days ago
            < 3 => 0.5,
            // App created <30 days ago
            < 30 => 0.75,
            // App created >30 days ago
            _ => 1
        };

        var quota = (int)Math.Max(
            // Make sure the quota is at least 1
            1,
            coefficient * (account.Features?.MagicLinkEmailMonthlyQuota ?? 500)
        );

        var emailsDispatchedIn30Days = await cache.GetOrCreateAsync(
            _emailsSentCacheKey,
            async cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromDays(1));
                return await tenantStorage.GetDispatchedEmailCountAsync(TimeSpan.FromDays(30));
            }
        );

        if (emailsDispatchedIn30Days >= quota)
        {
            throw new ApiException(
                "You have reached your monthly quota for magic link emails. " +
                "Please try again later.",
                429
            );
        }
    }

    public async Task SendMagicLinkAsync(MagicLinkDTO dto)
    {
        await EnforceQuotaAsync(dto);

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

        await tenantStorage.AddDispatchedEmailAsync(dto.UserId, dto.EmailAddress.Address, dto.LinkTemplate);

        // Update the cached tally of emails sent in the last 30 days
        if (cache.TryGetValue(_emailsSentCacheKey, out int cachedValue))
            cache.Set(_emailsSentCacheKey, cachedValue + 1);
    }
}