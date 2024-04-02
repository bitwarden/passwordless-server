using Testcontainers.MsSql;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests;

public class PasswordlessApiFixture : IAsyncDisposable, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public async Task<PasswordlessApi> CreateApiAsync(
        IReadOnlyDictionary<string, string?> settings = null,
        ITestOutputHelper? testOutput = null)
    {
        var api = new PasswordlessApi(
            _dbContainer.GetConnectionString(),
            settings,
            testOutput
        );

        // Perform migrations and make sure the API is ready to receive requests
        using var httpClient = api.CreateClient();
        using var response = await httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();

        return api;
    }

    public async ValueTask DisposeAsync() => await _dbContainer.DisposeAsync();

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}