using MailKit.Net.Smtp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Passwordless.Common.Services.Mail;

namespace Passwordless.Api.HealthChecks;

public class MailKitHealthCheck : IHealthCheck
{
    private readonly MailKitSmtpMailProvider _mailProvider;

    public MailKitHealthCheck(MailKitSmtpMailProvider mailProvider)
    {
        _mailProvider = mailProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        SmtpClient client = null;
        bool hasErrors = false;
        try
        {
            client = await _mailProvider.GetClient();
        }
        catch
        {
            hasErrors = true;
        }
        finally
        {
            if (client != null)
            {
                await client.DisconnectAsync(true, cancellationToken);
                client.Dispose();
            }
        }

        return hasErrors ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
    }
}