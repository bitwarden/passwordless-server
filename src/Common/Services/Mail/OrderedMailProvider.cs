using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Mail;

/// <summary>
/// Wraps multiple mail providers and tries to send the message using them in order.
/// </summary>
public class OrderedMailProvider : IMailProvider
{
    private readonly IOptionsSnapshot<MailConfiguration> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderedMailProvider> _logger;

    public OrderedMailProvider(
        IOptionsSnapshot<MailConfiguration> options,
        IServiceProvider serviceProvider,
        ILogger<OrderedMailProvider> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendAsync(MailMessage message)
    {
        for (var i = 0; i < _options.Value.Providers.Count; i++)
        {
            var providerConfiguration = _options.Value.Providers[i];
            var provider = _serviceProvider.GetKeyedService<IMailProvider>(i);

            if (provider == null)
            {
                _logger.LogError("No mail provider found for index {Index}", i);
                throw new InvalidOperationException($"No mail provider found for index {i}");
            }

            try
            {
                await provider.SendAsync(message);
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send message using provider '{Provider}'", providerConfiguration.Key);
            }
        }

        const string fallBackMessage = "No registered mail provider was able to send the message";
        _logger.LogCritical(fallBackMessage);
        throw new InvalidOperationException(fallBackMessage);
    }
}