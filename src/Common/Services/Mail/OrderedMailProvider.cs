using Microsoft.Extensions.Options;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.File;
using Passwordless.Common.Services.Mail.SendGrid;
using Passwordless.Common.Services.Mail.Smtp;

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
        if (message.From == null)
        {
            message.From = _options.Value.From;
        }
        foreach (var providerConfiguration in _options.Value.Providers)
        {
            try
            {
                var name = providerConfiguration.Name;
                _logger.LogDebug("Attempting to send message using provider '{Provider}'", name);
                IMailProvider provider;
                switch (name)
                {
                    case AwsProviderOptions.Provider:
                        var awsOptions = (AwsProviderOptions)providerConfiguration;
                        var awsMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<AwsProvider>>();
                        provider = new AwsProvider(awsOptions, awsMailProviderLogger);
                        break;
                    case SendGridProviderOptions.Provider:
                        var sendGridOptions = (SendGridProviderOptions)providerConfiguration;
                        var sendGridMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<SendGridProvider>>();
                        provider = new SendGridProvider(sendGridOptions, sendGridMailProviderLogger);
                        break;
                    case SmtpProviderOptions.Provider:
                        var smtpOptions = (SmtpProviderOptions)providerConfiguration;
                        provider = new SmtpProvider(smtpOptions);
                        break;
                    default:
                        // fall back to using the file mail provider.
                        var timeProvider = _serviceProvider.GetRequiredService<TimeProvider>();
                        var fileOptions = (FileProviderOptions)providerConfiguration;
                        var fileProviderLogger = _serviceProvider.GetRequiredService<ILogger<FileProvider>>();
                        provider = new FileProvider(timeProvider, fileOptions, fileProviderLogger);
                        break;
                }

                await provider.SendAsync(message);
                _logger.LogInformation("Sent message using provider '{Provider}'", name);
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send message using provider '{Provider}'", providerConfiguration.Name);
            }
        }

        const string fallBackMessage = "No registered mail provider was able to send the message";
        _logger.LogCritical(fallBackMessage);
        throw new InvalidOperationException(fallBackMessage);
    }
}