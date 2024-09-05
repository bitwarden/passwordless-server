using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail;

public class MailConfiguration
{
    /// <summary>
    /// The ordered list of mail providers to use.
    /// </summary>
    public IReadOnlyCollection<BaseMailProviderOptions> Providers { get; set; } = new List<BaseMailProviderOptions>();
}