using Microsoft.Extensions.Caching.Memory;
using Passwordless.Common.Services.Mail;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.MagicLinks;

public class MagicLinkService(
    TimeProvider timeProvider,
    IMemoryCache cache,
    ITenantStorage tenantStorage,
    IFido2Service fido2Service,
    IMailProvider mailProvider,
    IEventLogger eventLogger)
{
    private readonly string _emailsSentCacheKey = $"magic-link-emails-sent-30days-{tenantStorage.Tenant}";

    private async Task EnforceQuotaAsync(MagicLinkTokenRequest request)
    {
        var now = timeProvider.GetUtcNow();
        var account = await tenantStorage.GetAccountInformation();
        var accountAge = now - account.CreatedAt;

        // Applications created less than 24 hours ago can only send magic links to the admin email address
        if (accountAge < TimeSpan.FromHours(24) &&
            !account.AdminEmails.Contains(request.EmailAddress.Address, StringComparer.OrdinalIgnoreCase))
        {
            throw new ApiException(
                "magic_link_email_admin_address_only",
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
                var expiration = now.AddDays(1).Date;
                cacheEntry.SetAbsoluteExpiration(expiration);

                return await tenantStorage.GetDispatchedEmailCountAsync(TimeSpan.FromDays(30));
            }
        );

        if (emailsDispatchedIn30Days >= quota)
        {
            throw new ApiException(
                "magic_link_email_quota_exceeded",
                "You have reached your monthly quota for magic link emails. " +
                "Please try again later.",
                429
            );
        }
    }

    public async Task SendMagicLinkAsync(MagicLinkTokenRequest request)
    {
        await EnforceQuotaAsync(request);

        var token = await fido2Service.CreateMagicLinkToken(request);
        var link = new Uri(request.LinkTemplate.Replace("<token>", token));

        await mailProvider.SendAsync(new MailMessage
        {
            To = [request.EmailAddress.ToString()],
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

        await tenantStorage.AddDispatchedEmailAsync(request.UserId, request.EmailAddress.Address, request.LinkTemplate);

        // Update the cached tally of emails sent in the last 30 days
        if (cache.TryGetValue<int>(_emailsSentCacheKey, out var cachedValue))
        {
            var expiration = timeProvider.GetUtcNow().AddDays(1).Date;
            cache.Set(_emailsSentCacheKey, cachedValue + 1, expiration);
        }
        
        eventLogger.LogMagicLinkCreatedEvent(request.UserId);
    }
}