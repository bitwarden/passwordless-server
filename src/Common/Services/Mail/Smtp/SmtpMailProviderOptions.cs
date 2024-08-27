namespace Passwordless.Common.Services.Mail.Smtp;

public class SmtpMailProviderOptions : BaseMailProviderOptions
{
    public const string Provider = "smtp";

    public SmtpMailProviderOptions()
    {
        Name = Provider;
    }

    public string? Host { get; set; }

    public int Port { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool StartTls { get; set; }

    public bool Ssl { get; set; }

    public bool SslOverride { get; set; }

    public bool TrustServer { get; set; }
}