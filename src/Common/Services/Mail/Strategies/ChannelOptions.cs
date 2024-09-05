namespace Passwordless.Common.Services.Mail.Strategies;

public class ChannelOptions
{
    /// <summary>
    /// The default email address to use as the sender.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// The default name to use as the sender.
    /// </summary>
    public string? FromName { get; set; }
}