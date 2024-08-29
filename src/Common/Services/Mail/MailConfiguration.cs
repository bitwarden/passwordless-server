namespace Passwordless.Common.Services.Mail;

public class MailConfiguration
{
    /// <summary>
    /// The default email address to use as the sender.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// The default name to use as the sender.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// The ordered list of mail providers to use.
    /// </summary>
    public IReadOnlyCollection<BaseMailProviderOptions> Providers { get; set; } = new List<BaseMailProviderOptions>();
}