using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace HostedFidoApi.IntegrationTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {        
        builder.ConfigureServices(services =>
        {
            // While testing, we use InMemory database
            services.AddOptions<SqliteTenantOptions>().Configure(c => c.InMemory = true);

            //var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TodoGroupDbContext>));

            //if (descriptor != null)
            //{
            //    services.Remove(descriptor);
            //}

            //services.AddDbContext<TodoGroupDbContext>(options =>
            //{
            //    var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //    options.UseSqlite($"Data Source={Path.Join(path, "WebMinRouteGroup_tests.db")}");
            //});
        });

        return base.CreateHost(builder);
    }
}
