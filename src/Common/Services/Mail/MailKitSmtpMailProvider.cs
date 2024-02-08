using MailKit.Net.Smtp;
using MimeKit;

namespace Passwordless.Common.Services.Mail;

public class MailKitSmtpMailProvider : IMailProvider
{
    private readonly string? _fromEmail;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;
    private readonly int _smtpPort;
    private readonly string? _smtpHost;
    private readonly bool _smtpStartTls;
    private readonly bool _smtpSsl;
    private readonly bool _smtpSslOverride;
    private readonly bool _smtpTrustServer;

    public MailKitSmtpMailProvider(IConfiguration configuration)
    {
        IConfigurationSection mailOptions = configuration.GetSection("Mail");
        IConfigurationSection smtpOptions = mailOptions.GetSection("Smtp");
        _fromEmail = smtpOptions.GetValue<string>("From", null!);
        _smtpUsername = smtpOptions.GetValue<string>("Username", null!);
        _smtpPassword = smtpOptions.GetValue<string>("Password", null!);
        _smtpHost = smtpOptions.GetValue<string>("Host", null!);
        _smtpPort = smtpOptions.GetValue<int>("Port");
        _smtpStartTls = smtpOptions.GetValue<bool>("StartTls");
        _smtpSsl = smtpOptions.GetValue<bool>("Ssl");
        _smtpSslOverride = smtpOptions.GetValue<bool>("SslOverride");
        _smtpTrustServer = smtpOptions.GetValue<bool>("TrustServer");
    }

    public async Task SendAsync(MailMessage message)
    {
        var mimeMessage = new MimeMessage();
        var from = _fromEmail ?? message.From;
        mimeMessage.From.Add(MailboxAddress.Parse(from));
        mimeMessage.Subject = message.Subject;
        var toAddresses = message.To.Select(MailboxAddress.Parse).ToList();
        mimeMessage.To.AddRange(toAddresses);
        if (message.Bcc.Any())
        {
            var bccAddresses = message.Bcc.Select(MailboxAddress.Parse).ToList();
            mimeMessage.Bcc.AddRange(bccAddresses);
        }
        var builder = new BodyBuilder();
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            builder.TextBody = message.TextBody;
        }
        builder.HtmlBody = message.HtmlBody;
        mimeMessage.Body = builder.ToMessageBody();

        using var client = await GetClient();

        await client.SendAsync(mimeMessage);
        await client.DisconnectAsync(true);
    }

    public async Task<SmtpClient> GetClient()
    {
        var client = new SmtpClient();
        if (_smtpTrustServer)
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        if (!_smtpStartTls && !_smtpSsl && _smtpPort == 25)
        {
            await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.None);
        }
        else
        {
            var useSsl = (_smtpPort != 587 || _smtpSslOverride) && _smtpSsl;
            await client.ConnectAsync(_smtpHost, _smtpPort, useSsl);
        }

        if (!string.IsNullOrWhiteSpace(_smtpUsername) && !string.IsNullOrWhiteSpace(_smtpPassword))
        {
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
        }

        return client;
    }
}