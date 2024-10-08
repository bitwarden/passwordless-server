using Passwordless.Common.Services.Mail;

namespace Passwordless.Api.IntegrationTests.Helpers;

public class FakeMailProviderFactory : IMailProviderFactory
{
    public IMailProvider Create(string name, BaseMailProviderOptions options)
    {
        return new NoopMailProvider();
    }
}