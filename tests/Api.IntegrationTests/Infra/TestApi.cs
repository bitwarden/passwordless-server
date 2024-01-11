using Microsoft.Extensions.Time.Testing;

namespace Passwordless.Api.IntegrationTests.Infra;

public class TestApi : IDisposable
{
    public const string OriginUrl = "https://bitwarden.com/products/passwordless/";
    public const string RpId = "bitwarden.com";

    public IServiceProvider Services { get; }

    public HttpClient Client { get; }

    public FakeTimeProvider TimeProvider { get; }

    public TestApi(IServiceProvider services, HttpClient client, FakeTimeProvider timeProvider)
    {
        Services = services;
        Client = client;
        TimeProvider = timeProvider;
    }

    public void Dispose() => Client.Dispose();
}