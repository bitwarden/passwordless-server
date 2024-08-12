namespace Passwordless.Common.Services.Mail;

public class MailConfiguration
{
    /// <summary>
    /// The default email address to use as the sender.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// The ordered list of mail providers to use.
    /// </summary>
    public List<BaseProviderOptions> Providers { get; set; } = new();
}