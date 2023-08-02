namespace Passwordless.Common.Services.Mail;

// ReSharper disable once UnusedType.Global
public class FileMailProvider : IMailProvider
{
    private readonly ILogger<IMailProvider> _logger;

    public FileMailProvider(ILogger<IMailProvider> logger)
    {
        _logger = logger;
    }
    
    public Task SendAsync(MailMessage message)
    {
        string msg = message.TextBody;
        msg = "# New message " + DateTime.Now + Environment.NewLine + Environment.NewLine + msg + Environment.NewLine + Environment.NewLine;
        File.AppendAllText("mail.md", msg);
        _logger.LogInformation("Sent email to mail.md");
        return Task.CompletedTask;
    }
}