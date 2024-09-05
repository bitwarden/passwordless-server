using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.File;

public class FileMailProviderOptions : BaseMailProviderOptions
{
    public const string Provider = "file";
    public const string DefaultPath = "../mail.md";

    public FileMailProviderOptions()
    {
        Name = Provider;
    }

    public string Path { get; set; } = DefaultPath;

    public Dictionary<Channel, ChannelOptions> Channels { get; set; } = new();

}