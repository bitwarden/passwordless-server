using MailKit.Net.Smtp;
using MimeKit;

namespace Passwordless.Common.Services.Mail.Smtp;

public class SmtpProvider : IMailProvider
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

    public SmtpProvider(SmtpProviderOptions options)
    {

        _fromEmail = options.From;
        _smtpUsername = options.Username;
        _smtpPassword = options.Password;
        _smtpHost = options.Host;
        _smtpPort = options.Port;
        _smtpStartTls = options.StartTls;
        _smtpSsl = options.Ssl;
        _smtpSslOverride = options.SslOverride;
        _smtpTrustServer = options.TrustServer;
    }

    public async Task SendAsync(MailMessage message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(GetFromAddress(message));
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

        using var client = await GetClientAsync();

        await client.SendAsync(mimeMessage);
        await client.DisconnectAsync(true);
    }

    private MailboxAddress GetFromAddress(MailMessage message)
    {
        var from = (_fromEmail ?? message.From)!;

        return string.IsNullOrEmpty(message.FromDisplayName) ? MailboxAddress.Parse(from) : new MailboxAddress(message.FromDisplayName, from);
    }

    public async Task<SmtpClient> GetClientAsync()
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