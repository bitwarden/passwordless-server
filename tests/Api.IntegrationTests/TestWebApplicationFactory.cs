using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.IntegrationTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    public string DbName { get; set; } = Guid.NewGuid().ToString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.Sources.Clear();
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:sqlite", $"Data Source=test_{DbName}.db"),
                new KeyValuePair<string, string?>("PasswordlessManagement:ManagementKey", "dev_test_key"),
            });
        });

        builder.ConfigureServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = CreateDbContext(scope, null);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });

        return base.CreateHost(builder);
    }

    public DbTenantContext CreateDbContext(IServiceScope scope, string? appId)
    {
        var dbContextOptions = scope.ServiceProvider.GetRequiredService<DbContextOptions<SqliteContext>>();
        var context = new SqliteContext(dbContextOptions, new ManualTenantProvider(appId!));
        return context;
    }
}