using Passwordless.Common.Services.Mail;

namespace Passwordless.Api.IntegrationTests.Helpers;

// If needed by tests in the future, this can be an InMemoryMailProvider,
// where you can read the sent emails.
public class NoopMailProvider : IMailProvider
{
    public Task SendAsync(MailMessage message) => Task.CompletedTask;
}