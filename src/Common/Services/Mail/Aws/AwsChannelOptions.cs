using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.Aws;

public class AwsChannelOptions : ChannelOptions
{
    public string? ConfigurationSet { get; set; }
}