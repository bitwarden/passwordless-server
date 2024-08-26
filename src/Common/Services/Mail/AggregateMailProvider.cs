using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Mail;

/// <summary>
/// Wraps multiple mail providers and tries to send the message using them in order.
/// </summary>
public class AggregateMailProvider : IMailProvider
{
    private readonly IOptionsSnapshot<MailConfiguration> _options;
    private readonly IMailProviderFactory _factory;
    private readonly ILogger<AggregateMailProvider> _logger;

    public const string FallBackFailedMessage = "No registered mail provider was able to send the message";

    public AggregateMailProvider(
        IOptionsSnapshot<MailConfiguration> options,
        IMailProviderFactory factory,
        ILogger<AggregateMailProvider> logger)
    {
        _options = options;
        _factory = factory;
        _logger = logger;
    }

    public async Task SendAsync(MailMessage message)
    {
        if (message.From == null)
        {
            message.From = _options.Value.From;
        }
        foreach (var providerConfiguration in _options.Value.Providers)
        {
            try
            {
                _logger.LogDebug("Attempting to send message using provider '{Provider}'", providerConfiguration.Name);
                var provider = _factory.Create(providerConfiguration.Name, providerConfiguration);

                await provider.SendAsync(message);
                _logger.LogInformation("Sent message using provider '{Provider}'", providerConfiguration.Name);
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send message using provider '{Provider}'", providerConfiguration.Name);
            }
        }

        _logger.LogCritical(FallBackFailedMessage);
        throw new InvalidOperationException(FallBackFailedMessage);
    }
}