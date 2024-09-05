using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail;

public class MailConfiguration
{
    [Obsolete]
    public string? From { get; set; }

    [Obsolete]
    public string? FromName { get; set; }

    /// <summary>
    /// The ordered list of mail providers to use.
    /// </summary>
    public IReadOnlyCollection<BaseMailProviderOptions> Providers { get; set; } = new List<BaseMailProviderOptions>();
}