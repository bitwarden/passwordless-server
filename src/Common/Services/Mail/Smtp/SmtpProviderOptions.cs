namespace Passwordless.Common.Services.Mail.Smtp;

public class SmtpProviderOptions : BaseProviderOptions
{
    public const string Provider = "smtp";

    public string? Host { get; set; }

    public int Port { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string From { get; set; }

    public bool StartTls { get; set; }

    public bool Ssl { get; set; }

    public bool SslOverride { get; set; }

    public bool TrustServer { get; set; }
}