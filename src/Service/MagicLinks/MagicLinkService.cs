using Passwordless.Common.MagicLinks.Models;
using Passwordless.Common.Services.Mail;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.MagicLinks;

public class MagicLinkService(
    TimeProvider timeProvider,
    ITenantStorage tenantStorage,
    IFido2Service fido2Service,
    IMailProvider mailProvider,
    IEventLogger eventLogger)
{
    private async Task EnforceQuotaAsync(MagicLinkTokenRequest request)
    {
        var now = timeProvider.GetUtcNow();
        var account = await tenantStorage.GetAccountInformation();
        var accountAge = now - account.CreatedAt;

        // Applications created less than 24 hours ago can only send magic links to the admin email address
        if (accountAge < TimeSpan.FromHours(24) &&
            !IsAdminConsole(account) &&
            !account.AdminEmails.Contains(request.EmailAddress.Address, StringComparer.OrdinalIgnoreCase))
        {
            throw new ApiException(
                "magic_link_email_admin_address_only",
                "Because your application has been created less than 24 hours ago, " +
                "you can only request magic links to the admin email address.",
                403
            );
        }

        var maxQuota = (await tenantStorage.GetAppFeaturesAsync()).MagicLinkEmailMonthlyQuota;

        // Reduce the quota for newly created applications
        var quotaModifier = accountAge.TotalDays switch
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

        var quota = (int)(maxQuota * quotaModifier);

        var emailsDispatchedIn30Days = await tenantStorage.GetDispatchedEmailCountAsync(TimeSpan.FromDays(30));

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

    private static bool IsAdminConsole(PerTenant account) =>
        string.Equals(account.Tenant, "admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(account.Tenant, "adminconsole", StringComparison.OrdinalIgnoreCase);

    public async Task SendMagicLinkAsync(MagicLinkTokenRequest request)
    {
        await EnforceQuotaAsync(request);

        var token = await fido2Service.CreateMagicLinkTokenAsync(request);
        var link = new Uri(request.LinkTemplate.Replace(SendMagicLinkRequest.TokenTemplate, token));

        await mailProvider.SendAsync(new MailMessage
        {
            To = [request.EmailAddress.ToString()],
            Subject = $"Sign in to {link.Host}",
            TextBody = $"Please click the link or copy into your browser of choice to log in to {link.Host}: {link}. If you did not request this email, it is safe to ignore.",
            HtmlBody =
                //lang=html
                $"""
                 <!DOCTYPE html5>
                 <html lang="en">
                    <head>
                        <title>Sign-In With Link Request</title>
                    </head>
                    <body style="background-color: white;
                                    font-family: DM Sans,system-ui,-apple-system,BlinkMacSystemFont,Segoe UI,Helvetica Neue,sans-serif;
                                    font-size: 16px;
                                    text-align: center;
                                    ">
                        <h3>Hello</h3>
                        <p style="padding-bottom: 4px">Please click the button below to sign in to {link.Host}</p>
                        <a href="{link}" style="width: auto; margin: auto; padding: 10px 25px; background-color: #1252A3; border-radius: 999px; color: white; text-decoration: none;">Click here to sign in</a>
                        <p style="padding-top: 4px">Having trouble? Copy this link to your browser: {link}</p>
                        <p>If you did not request this email, it is safe to ignore.</p>
                    </body>
                 </html>
                 """,
            MessageType = "magic-links",
            FromDisplayName = $"Sign in to {link.Host}"
        });

        await tenantStorage.AddDispatchedEmailAsync(request.UserId, request.EmailAddress.Address, request.LinkTemplate);
        eventLogger.LogMagicLinkCreatedEvent(request.UserId);
    }
}