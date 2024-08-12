namespace Passwordless.Common.Services.Mail;

public class MailConfiguration
{
    /// <summary>
    /// The ordered list of mail providers to use.
    /// </summary>
    public List<KeyValuePair<string, IProviderOptions>> Providers { get; set; } = new();
}