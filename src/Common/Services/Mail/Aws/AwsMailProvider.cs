using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

namespace Passwordless.Common.Services.Mail.Aws;

public class AwsMailProvider : IMailProvider
{
    private readonly IAmazonSimpleEmailServiceV2 _client;
    private readonly ILogger<AwsMailProvider> _logger;

    public AwsMailProvider(
        AwsProviderOptions options,
        ILogger<AwsMailProvider> logger)
    {
        var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretAccessKey);
        AmazonSimpleEmailServiceV2Config sesConfiguration = new AmazonSimpleEmailServiceV2Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region)
        };
        _client = new AmazonSimpleEmailServiceV2Client(credentials, sesConfiguration);
        _logger = logger;
    }

    public async Task SendAsync(MailMessage message)
    {
        var request = new SendEmailRequest
        {
            FromEmailAddress = message.From
        };

        if (message.To.Any())
        {
            request.Destination = new Destination
            {
                ToAddresses = message.To.ToList()
            };
        }

        request.Content = new EmailContent
        {
            Simple = new Message
            {
                Subject = new Content { Data = message.Subject },
                Body = new Body
                {
                    Html = new Content { Data = message.HtmlBody },
                    Text = new Content { Data = message.TextBody }
                }
            }
        };

        try
        {
            await _client.SendEmailAsync(request);
            _logger.LogInformation("Sent email with subject '{Subject}' to '{To}'", message.Subject, message.To);
        }
        catch (AccountSuspendedException ex)
        {
            _logger.LogError(ex, "The account's ability to send email has been permanently restricted.");
            throw;
        }
        catch (MailFromDomainNotVerifiedException ex)
        {
            _logger.LogError(ex, "The sending domain is not verified.");
            throw;
        }
        catch (MessageRejectedException ex)
        {
            _logger.LogError(ex, "The message content is invalid.");
            throw;
        }
        catch (SendingPausedException ex)
        {
            _logger.LogError(ex, "The account's ability to send email is currently paused.");
            throw;
        }
        catch (TooManyRequestsException ex)
        {
            _logger.LogError(ex, "Too many requests were made. Please try again later.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending the email.");
            throw;
        }
    }
}