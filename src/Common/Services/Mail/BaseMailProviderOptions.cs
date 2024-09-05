using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail;

public abstract class BaseMailProviderOptions
{
    /// <summary>
    /// The name of the provider.
    /// </summary>
    public string Name { get; set; }

    public Dictionary<Channel, ChannelOptions> Channels { get; set; } = new();

}