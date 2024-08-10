using Microsoft.Extensions.Options;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.File;
using Passwordless.Common.Services.Mail.Smtp;

namespace Passwordless.Common.Services.Mail.Ordered;

public class OrderedMailProvider : IMailProvider
{
    private readonly IOptionsSnapshot<OrderedMailProviderConfiguration> _options;
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger<OrderedMailProvider> _logger;

    public OrderedMailProvider(
        IOptionsSnapshot<OrderedMailProviderConfiguration> options,
        ServiceProvider serviceProvider,
        ILogger<OrderedMailProvider> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task SendAsync(MailMessage message)
    {
        foreach (var providerConfiguration in _options.Value.Providers)
        {
            try
            {
                var name = providerConfiguration.GetName();
                IMailProvider provider;
                switch (name)
                {
                    case AwsMailProviderConfiguration.Name:
                        var awsMailProviderConfiguration = (AwsMailProviderConfiguration)providerConfiguration;
                        var awsMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<AwsMailProvider>>();
                        provider = new AwsMailProvider(awsMailProviderConfiguration, awsMailProviderLogger);
                        break;
                    case SmtpMailProviderConfiguration.Name:
                        var mailkitSmtpProviderConfiguration = (SmtpMailProviderConfiguration)providerConfiguration;
                        provider = new SmtpMailProvider(mailkitSmtpProviderConfiguration);
                        break;
                    default:
                        // fall back to using the file mail provider.
                        var timeProvider = _serviceProvider.GetRequiredService<TimeProvider>();
                        var fileMailProviderConfiguration = (FileMailProviderConfiguration)providerConfiguration;
                        var fileMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<FileMailProvider>>();
                        provider = new FileMailProvider(timeProvider, fileMailProviderConfiguration, fileMailProviderLogger);
                        break;
                }
                return provider.SendAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send message using provider '{Provider}'", providerConfiguration.GetName());
            }
        }

        const string fallBackMessage = "No registered mail provider was able to send the message";
        _logger.LogCritical(fallBackMessage);
        throw new InvalidOperationException(fallBackMessage);
    }
}