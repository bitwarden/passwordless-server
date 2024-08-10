using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Mail.File;

// ReSharper disable once UnusedType.Global
public class FileMailProvider : IMailProvider
{
    public const string DefaultPath = "mail.md";

    private readonly string _path;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<IMailProvider> _logger;

    public FileMailProvider(
        TimeProvider timeProvider,
        IOptions<FileMailProviderConfiguration> configuration,
        ILogger<IMailProvider> logger) : this(timeProvider, configuration.Value, logger)
    {
    }

    public FileMailProvider(
        TimeProvider timeProvider,
        FileMailProviderConfiguration configuration,
        ILogger<IMailProvider> logger)
    {
        _timeProvider = timeProvider;
        _path = string.IsNullOrEmpty(configuration.Path) ? DefaultPath : configuration.Path;
        _logger = logger;
    }

    public async Task SendAsync(MailMessage message)
    {
        var content =
            $"""
            # New message {_timeProvider.GetLocalNow()}
            
            {message.TextBody}

            """;

        await System.IO.File.AppendAllTextAsync(_path, content);

        _logger.LogInformation("Saved email contents to '{Path}'", _path);
    }
}