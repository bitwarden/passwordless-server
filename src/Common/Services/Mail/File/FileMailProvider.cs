using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.File;

// ReSharper disable once UnusedType.Global
public class FileMailProvider : IMailProvider
{
    private readonly string _path;
    private readonly FileEmailChannelStrategy _emailChannelStrategy;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<IMailProvider> _logger;

    public FileMailProvider(
        TimeProvider timeProvider,
        FileMailProviderOptions options,
        ILogger<IMailProvider> logger)
    {
        _emailChannelStrategy = new FileEmailChannelStrategy(options);
        _timeProvider = timeProvider;
        _path = options.Path;
        _logger = logger;
    }

    public async Task SendAsync(MailMessage message)
    {
        _emailChannelStrategy.SetSenderInfo(message);

        var content =
            $"""
             # New message {_timeProvider.GetLocalNow()}

             {message.TextBody}

             """;

        await System.IO.File.AppendAllTextAsync(_path, content);

        _logger.LogInformation("Saved email contents to '{Path}'", _path);
    }
}