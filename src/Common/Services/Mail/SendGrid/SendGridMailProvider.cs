using SendGrid;
using SendGrid.Helpers.Mail;

namespace Passwordless.Common.Services.Mail.SendGrid;

public class SendGridMailProvider : IMailProvider
{
    private readonly ISendGridClient _client;
    private readonly ILogger<SendGridMailProvider> _logger;

    public SendGridMailProvider(
        SendGridMailProviderOptions options,
        ILogger<SendGridMailProvider> logger)
    {
        _client = new SendGridClient(options.ApiKey);
        _logger = logger;
    }

    public async Task SendAsync(MailMessage message)
    {
        var from = new EmailAddress(message.From, message.FromDisplayName);
        var subject = message.Subject;
        var recipients = message.To.Select(x => new EmailAddress(x)).ToList();
        var textContent = message.TextBody;
        var htmlContent = message.HtmlBody;
        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, recipients, subject, textContent, htmlContent);
        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Body.ReadAsStringAsync();
            _logger.LogError("Failed to send email with SendGrid. Status code: {StatusCode}. Error: {Error}", response.StatusCode, error);
            throw new Exception($"Failed to send email with SendGrid: {error}");
        }
    }
}