using System.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Billing;
using Passwordless.AdminConsole.EventLog;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Db;

public static class DatabaseBootstrap
{
    private static string ContextName;

    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        // if not present, try use sqlite
        var sqlite = builder.Configuration.GetConnectionString("sqlite:admin");
        var mssql = builder.Configuration.GetConnectionString("mssql:admin");

        // read "migrate_db" from env
        var migrating = builder.Configuration.GetValue<string>("ef_migrate");
        if (migrating == "1")
        {
            builder.Services.AddDbContext<ConsoleDbContext, MssqlConsoleDbContext>();
            builder.Services.AddDbContext<ConsoleDbContext, SqliteConsoleDbContext>();
        }

        // if name starts with sqlite, use sqlite, else use mssql
        if (!String.IsNullOrEmpty(sqlite))
        {
            ContextName = typeof(SqliteConsoleDbContext).FullName;
            builder.AddDatabaseContext<SqliteConsoleDbContext>((sp, o) =>
            {
                o.UseSqlite(sqlite);
            });
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            ContextName = typeof(MssqlConsoleDbContext).FullName;
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
        builder.Services.AddDbContext<ConsoleDbContext, TDbContext>(action);
        builder.Services.AddScoped<IDataService, DataService>();
        builder.Services.AddScoped<IUsageService, UsageService>();
        builder.Services.AddScoped<IInvitationService, InvitationService>();
        builder.Services.AddScoped<IApplicationService, ApplicationService>();
        builder.Services.AddScoped<IOrganizationFeatureService, OrganizationFeatureService>();
        builder.AddBilling();
        builder.Services.AddEventLogging();

        // Identity
        builder.Services
            .AddIdentity<ConsoleAdmin, IdentityRole>()
            .AddEntityFrameworkStores<TDbContext>()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders()
            .AddPasswordless(o =>
            {
                var managementOptions = builder.Configuration.GetSection("PasswordlessManagement").Get<PasswordlessManagementOptions>();
                var options = builder.Configuration.GetSection("Passwordless").Get<PasswordlessOptions>();
                o.ApiSecret = options.ApiSecret;
                o.ApiKey = options.ApiKey;
                o.ApiUrl = options.ApiUrl;

                if (builder.Configuration.GetValue("SelfHosted", false))
                {
                    // This will overwrite ApiUrl with the internal url for self-hosting, this is intentional.
                    if (string.IsNullOrEmpty(managementOptions.InternalApiUrl))
                    {
                        throw new ConfigurationErrorsException("Missing 'PasswordlessManagement:InternalApiUrl'.");
                    }
                    o.ApiUrl = managementOptions.InternalApiUrl;
                }
            });

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services
                .AddDataProtection()
                .PersistKeysToDbContext<TDbContext>();
        }
    }

    public static void ExecuteMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var type = Type.GetType(ContextName);
        if (type == null)
        {
            throw new ConfigurationErrorsException("Unknown database type");
        }
        var dbContext = (ConsoleDbContext)scope.ServiceProvider.GetRequiredService(type);
        dbContext.Database.Migrate();
    }
}