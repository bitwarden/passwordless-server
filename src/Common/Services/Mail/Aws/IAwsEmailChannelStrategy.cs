using Amazon.SimpleEmailV2.Model;
using Passwordless.Common.Services.Mail.Strategies;

namespace Passwordless.Common.Services.Mail.Aws;

public interface IAwsEmailChannelStrategy
{
    void SetSenderInfo(SendEmailRequest message, Channel channel);
}