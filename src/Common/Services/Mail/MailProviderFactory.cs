using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.File;
using Passwordless.Common.Services.Mail.SendGrid;
using Passwordless.Common.Services.Mail.Smtp;

namespace Passwordless.Common.Services.Mail;

public class MailProviderFactory : IMailProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MailProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IMailProvider Create(string name, BaseMailProviderOptions options)
    {
        switch (name)
        {
            case AwsMailProviderOptions.Provider:
                var awsOptions = (AwsMailProviderOptions)options;
                var awsMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<AwsMailProvider>>();
                return new AwsMailProvider(awsOptions, awsMailProviderLogger);
            case SendGridMailProviderOptions.Provider:
                var sendGridOptions = (SendGridMailProviderOptions)options;
                var sendGridMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<SendGridMailProvider>>();
                return new SendGridMailProvider(sendGridOptions, sendGridMailProviderLogger);
            case SmtpMailProviderOptions.Provider:
                var smtpOptions = (SmtpMailProviderOptions)options;
                return new SmtpMailProvider(smtpOptions);
            case FileMailProviderOptions.Provider:
                // fall back to using the file mail provider.
                var timeProvider = _serviceProvider.GetRequiredService<TimeProvider>();
                var fileOptions = (FileMailProviderOptions)options;
                var fileProviderLogger = _serviceProvider.GetRequiredService<ILogger<FileMailProvider>>();
                return new FileMailProvider(timeProvider, fileOptions, fileProviderLogger);
            default:
                throw new NotSupportedException($"Unknown mail provider type '{name}'");
        }
    }
}