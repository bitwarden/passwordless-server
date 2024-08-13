using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Mail.File;

// ReSharper disable once UnusedType.Global
public class FileProvider : IMailProvider
{
    private readonly string _path;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<IMailProvider> _logger;

    public FileProvider(
        TimeProvider timeProvider,
        IOptions<FileProviderOptions> configuration,
        ILogger<IMailProvider> logger) : this(timeProvider, configuration.Value, logger)
    {
    }

    public FileProvider(
        TimeProvider timeProvider,
        FileProviderOptions options,
        ILogger<IMailProvider> logger)
    {
        _timeProvider = timeProvider;
        _path = options.Path;
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