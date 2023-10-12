using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Billing;
using Passwordless.AdminConsole.EventLog;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Db;

public static class DatabaseBootstrap
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        // if not present, try use sqlite
        var sqlite = builder.Configuration.GetConnectionString("sqlite:admin");
        var mssql = builder.Configuration.GetConnectionString("mssql:admin");

        // read "migrate_db" from env
        var migrating = builder.Configuration.GetValue<string>("ef_migrate");
        if (migrating == "1")
        {
            builder.Services.AddDbContextFactory<MssqlConsoleDbContext>();
            builder.Services.AddDbContextFactory<SqliteConsoleDbContext>();
        }

        // if name starts with sqlite, use sqlite, else use mssql
        if (!String.IsNullOrEmpty(sqlite))
        {
            builder.AddDatabaseContext<SqliteConsoleDbContext>((sp, o) =>
            {
                o.UseSqlite(sqlite);
            });
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            builder.AddDatabaseContext<MssqlConsoleDbContext>((sp, o) =>
            {
                o.UseSqlServer(mssql);
            });
        }
        else
        {
            throw new Exception("Unknown database type");
        }
    }

    /// <summary>
    /// Pooling is used to avoid issues with concurrent access to the same db context.
    /// Abstract services that use the db context, and make injecting them easier as we can no longer resolve ConsoleDbContext.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <typeparam name="TDbContext"></typeparam>
    private static void AddDatabaseContext<TDbContext>(this WebApplicationBuilder builder, Action<IServiceProvider, DbContextOptionsBuilder> action)
        where TDbContext : ConsoleDbContext
    {
        builder.Services.AddPooledDbContextFactory<TDbContext>(action);
        builder.Services.AddScoped<IDataService, DataService<TDbContext>>();
        builder.Services.AddScoped<IUsageService, UsageService<TDbContext>>();
        builder.Services.AddScoped<IInvitationService, InvitationService<TDbContext>>();
        builder.Services.AddScoped<IApplicationService, ApplicationService<TDbContext>>();
        builder.Services.AddScoped<IOrganizationFeatureService, OrganizationFeatureService<TDbContext>>();
        builder.AddBilling<TDbContext>();
        builder.Services.AddEventLogging<TDbContext>();

        builder.Services.AddScoped<TDbContext>(sp =>
        {
            var dbContextFactory = sp.GetRequiredService<IDbContextFactory<TDbContext>>();
            return dbContextFactory.CreateDbContext();
        });

        // Identity
        builder.Services
            .AddIdentity<ConsoleAdmin, IdentityRole>()
            .AddEntityFrameworkStores<TDbContext>()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory<TDbContext>>()
            .AddDefaultTokenProviders()
            .AddPasswordless(builder.Configuration.GetSection("Passwordless"));

    }
}