using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Mail;

/// <summary>
/// Wraps multiple mail providers and tries to send the message using them in order.
/// </summary>
public class AggregateMailProvider(
    IOptionsSnapshot<MailConfiguration> options,
    IMailProviderFactory factory,
    ILogger<AggregateMailProvider> logger)
    : IMailProvider
{
    public async Task SendAsync(MailMessage message)
    {
        message.From ??= options.Value.From;
        message.FromDisplayName ??= options.Value.FromName;

        foreach (var providerConfiguration in options.Value.Providers)
        {
            try
            {
                logger.LogDebug("Attempting to send message using provider '{Provider}'", providerConfiguration.Name);
                var provider = factory.Create(providerConfiguration.Name, providerConfiguration);

                await provider.SendAsync(message);
                logger.LogInformation("Sent message using provider '{Provider}'", providerConfiguration.Name);
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send message using provider '{Provider}'", providerConfiguration.Name);
            }
        }
    }
}