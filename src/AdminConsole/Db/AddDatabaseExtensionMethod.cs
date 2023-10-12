using Microsoft.AspNetCore.Identity;
using Passwordless.AdminConsole.Billing;
using Passwordless.AdminConsole.EventLog;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Db;

public static class AddDatabaseExtensionMethod
{
    public static void AddDatabase(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // if not present, try use sqlite
        var sqlite = builder.Configuration.GetConnectionString("sqlite:admin");
        var mssql = builder.Configuration.GetConnectionString("mssql:admin");

        // read "migrate_db" from env
        var migrating = builder.Configuration.GetValue<string>("ef_migrate");
        if (migrating == "1")
        {
            services.AddDbContextFactory<MssqlConsoleDbContext>();
            services.AddDbContextFactory<SqliteConsoleDbContext>();
        }

        // if name starts with sqlite, use sqlite, else use mssql
        if (!String.IsNullOrEmpty(sqlite))
        {
            builder.AddDatabaseContext<SqliteConsoleDbContext>();
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            builder.AddDatabaseContext<MssqlConsoleDbContext>();
        }
        else
        {
            throw new Exception("Unknown database type");
        }
    }

    private static void AddDatabaseContext<TDbContext>(this WebApplicationBuilder builder) where TDbContext : ConsoleDbContext
    {
        builder.Services.AddDbContextFactory<TDbContext>();
        builder.Services.AddScoped<IDataService, DataService<TDbContext>>();
        builder.Services.AddScoped<IUsageService, UsageService<TDbContext>>();
        builder.Services.AddScoped<IInvitationService, InvitationService<TDbContext>>();
        builder.Services.AddScoped<IApplicationService, ApplicationService<TDbContext>>();
        builder.Services.AddScoped<IOrganizationFeatureService, OrganizationFeatureService<TDbContext>>();
        builder.AddBilling<TDbContext>();
        builder.Services.AddEventLogging<TDbContext>();

        // Identity
        builder.Services
            .AddIdentity<ConsoleAdmin, IdentityRole>()
            .AddEntityFrameworkStores<TDbContext>()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory<TDbContext>>()
            .AddDefaultTokenProviders()
            .AddPasswordless(builder.Configuration.GetSection("Passwordless"));

    }
}