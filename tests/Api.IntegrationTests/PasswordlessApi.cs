using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Common.Services.Mail;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests;

public class PasswordlessApi : ITestOutputHelperAccessor, IDisposable, IAsyncDisposable
{
    public const string OriginUrl = "https://bitwarden.com/products/passwordless/";
    public const string RpId = "bitwarden.com";

    private readonly WebApplicationFactory<Program> _factory;

    public PasswordlessApi(
        ITestOutputHelper? testOutput,
        string databaseConnectionString)
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => host
                .UseSetting("ConnectionStrings:sqlite:api", string.Empty)
                .UseSetting("ConnectionStrings:mssql:api", databaseConnectionString)
                .UseSetting("Flags:IsRateLimitBypassEnabled", "true")
                .ConfigureLogging(logging => logging.ClearProviders().AddXUnit(this))
                .ConfigureTestServices(services =>
                {
                    // Disable background services
                    services.RemoveAll<IHostedService>();

                    // Replace time
                    services.RemoveAll<TimeProvider>();
                    services.AddSingleton<TimeProvider>(Time);

                    // Replace mail provider
                    services.RemoveAll<IMailProvider>();
                    services.AddSingleton<IMailProvider, NoopMailProvider>();
                }));

        TestOutput = testOutput;
        Services = _factory.Services;
    }

    public ITestOutputHelper? TestOutput { get; }

    // Don't use this unless there is no other way to test something
    public IServiceProvider Services { get; }

    // Initialize the time provider with the current time instead of the default.
    // We need this because not all code paths rely on the time provider, so initializing
    // with the current time helps maintain consistency.
    public FakeTimeProvider Time { get; } = new(DateTimeOffset.Now);

    public HttpClient CreateClient() => _factory.CreateClient();

    public void ResetCache() => ((MemoryCache)Services.GetRequiredService<IMemoryCache>()).Clear();

    public void Dispose() => _factory.Dispose();

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    // I don't like this name, so hide it via explicit interface implementation
    ITestOutputHelper? ITestOutputHelperAccessor.OutputHelper
    {
        get => TestOutput;
        // This is immutable
        set => throw new NotSupportedException();
    }
}