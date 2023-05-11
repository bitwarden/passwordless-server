using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.IntegrationTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    public string DbName { get; set; } = Guid.NewGuid().ToString();
    public string ApiKey { get; set; } = "test:public:2e728aa5986f4ba8b073a5b28a939795";
    public string ApiSecret { get; set; } = "test:secret:a679563b331846c79c20b114a4f56d02";

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
            var context = CreateDbContext(scope, "test");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            // seed with test keys
            // hash of secret
            context.ApiKeys.Add(new ApiKeyDesc() { Tenant = "test", Id = "9795", ApiKey = "test:public:2e728aa5986f4ba8b073a5b28a939795" });
            context.ApiKeys.Add(new ApiKeyDesc() { Tenant = "test", Id = "6d02", ApiKey = "4RtmMr0hVknaQAIhaRtPHw==:xR7bg3NVsC80a8GDDhH39g==",Scopes = new[] { "token_register", "token_verify" }, });
            context.AccountInfo.Add(new AccountMetaInformation() { Tenant = "test", AcountName = "test" });
            context.SaveChanges();
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