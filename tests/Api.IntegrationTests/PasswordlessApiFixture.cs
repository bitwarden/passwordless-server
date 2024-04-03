using Testcontainers.MsSql;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests;

public class PasswordlessApiFixture : IAsyncDisposable, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public async Task<PasswordlessApi> CreateApiAsync(PasswordlessApiOptions options)
    {
        var api = new PasswordlessApi(new PasswordlessApiOptions
        {
            TestOutput = options.TestOutput,
            // Add the connection string to the settings
            Settings = new Dictionary<string, string?>(options.Settings)
            {
                ["ConnectionStrings:sqlite:api"] = null,
                ["ConnectionStrings:mssql:api"] = _dbContainer.GetConnectionString()
            }
        });

        // Perform migrations and make sure the API is ready to receive requests
        using var httpClient = api.CreateClient();
        using var response = await httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();

        return api;
    }

    public async ValueTask DisposeAsync() => await _dbContainer.DisposeAsync();

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}