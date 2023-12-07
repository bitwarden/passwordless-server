using Microsoft.Extensions.Options;

namespace Passwordless.Common.Services.Mail;

// ReSharper disable once UnusedType.Global
public class FileMailProvider : IMailProvider
{
    private readonly string _path;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<IMailProvider> _logger;

    public FileMailProvider(
        TimeProvider timeProvider,
        IOptions<FileMailProviderConfiguration> configuration,
        ILogger<IMailProvider> logger)
    {
        _timeProvider = timeProvider;
        _path = string.IsNullOrEmpty(configuration.Value.Path) ? "mail.md" : $"{configuration.Value.Path}/mail.md";
        _logger = logger;
    }

    public Task SendAsync(MailMessage message)
    {
        string msg = message.TextBody;
        msg =
        $"""
        # New message {_timeProvider.GetLocalNow()}
        
        {msg}
        
        """;
        File.AppendAllText(_path, msg);
        _logger.LogInformation("Sent email to '{Path}'", _path);
        return Task.CompletedTask;
    }
}