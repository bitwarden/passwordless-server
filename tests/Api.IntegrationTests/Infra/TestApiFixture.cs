using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Serilog;
using Testcontainers.MsSql;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Infra;

public class TestApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Perform migrations and make sure the API is ready to receive requests
        using var api = CreateApi();
        using var response = await api.Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }

    public TestApi CreateApi(Action<IServiceCollection>? configure = null, ITestOutputHelper? testOutput = null)
    {
        var timeProvider = new FakeTimeProvider();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => host
                .UseSetting("ConnectionStrings:sqlite:api", string.Empty)
                .UseSetting("ConnectionStrings:mssql:api", _dbContainer.GetConnectionString())
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();

                    // We use both M.E.Logging and Serilog for logging in the API,
                    // so weed to configure sinks for both here.
                    if (testOutput is not null)
                    {
                        logging
                            .AddXUnit(testOutput)
                            .AddSerilog(new LoggerConfiguration()
                                .MinimumLevel.Verbose()
                                .WriteTo.TestOutput(testOutput)
                                .CreateLogger()
                            );
                    }
                })
                .ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IHostedService));
                    services.RemoveAll(typeof(TimeProvider));
                    services.AddSingleton<TimeProvider>(timeProvider);

                    configure?.Invoke(services);
                })
            );

        return new TestApi(factory.Services, factory.CreateClient(), timeProvider);
    }

    public async Task DisposeAsync() => await _dbContainer.DisposeAsync();
}