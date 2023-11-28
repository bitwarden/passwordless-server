using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using Xunit;

namespace Passwordless.Api.Integration.Tests;

public class PasswordlessApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithPassword("P455w0rd1355!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseSetting("ConnectionStrings:sqlite:api", string.Empty)
            .UseSetting("ConnectionStrings:mssql:api", _dbContainer.GetConnectionString())
            .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
            .ConfigureTestServices(services =>
                services.RemoveAll(typeof(IHostedService)));
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Perform migrations and make sure the API is ready to receive requests
        using var httpClient = this.CreateClient();
        using var response = await httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}