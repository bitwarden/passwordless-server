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

    public IMailProvider Create(string name, BaseProviderOptions options)
    {
        switch (name)
        {
            case AwsProviderOptions.Provider:
                var awsOptions = (AwsProviderOptions)options;
                var awsMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<AwsProvider>>();
                return new AwsProvider(awsOptions, awsMailProviderLogger);
            case SendGridProviderOptions.Provider:
                var sendGridOptions = (SendGridProviderOptions)options;
                var sendGridMailProviderLogger = _serviceProvider.GetRequiredService<ILogger<SendGridProvider>>();
                return new SendGridProvider(sendGridOptions, sendGridMailProviderLogger);
            case SmtpProviderOptions.Provider:
                var smtpOptions = (SmtpProviderOptions)options;
                return new SmtpProvider(smtpOptions);
            case FileProviderOptions.Provider:
                // fall back to using the file mail provider.
                var timeProvider = _serviceProvider.GetRequiredService<TimeProvider>();
                var fileOptions = (FileProviderOptions)options;
                var fileProviderLogger = _serviceProvider.GetRequiredService<ILogger<FileProvider>>();
                return new FileProvider(timeProvider, fileOptions, fileProviderLogger);
            default:
                throw new NotSupportedException($"Unknown mail provider type '{name}'");
        }
    }
}