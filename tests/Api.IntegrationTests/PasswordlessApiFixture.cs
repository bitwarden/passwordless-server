using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Xunit;

namespace Passwordless.Api.IntegrationTests;

public class PasswordlessApiFixture : IAsyncDisposable, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-C", "-Q", "SELECT 1;"))
        .Build();

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public PasswordlessApi CreateApi(PasswordlessApiOptions options) =>
        new(new PasswordlessApiOptions
        {
            TestOutput = options.TestOutput,
            // Add the connection string to the settings
            Settings = new Dictionary<string, string?>(options.Settings)
            {
                ["ConnectionStrings:sqlite:api"] = null,
                ["ConnectionStrings:mssql:api"] = _dbContainer.GetConnectionString()
            }
        });

    public async ValueTask DisposeAsync() => await _dbContainer.DisposeAsync();

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}