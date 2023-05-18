using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Passwordless.Api.Authorization;
using Passwordless.Service;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

#nullable enable

namespace Passwordless.Api.Helpers;

public static class AddDatabaseExtensionMethod
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Database information
        var sqlite = configuration.GetConnectionString("sqlite");
        var mssql = configuration.GetConnectionString("mssql");
        if (!string.IsNullOrEmpty(sqlite))
        {
            services.AddDbContext<DbTenantContext, SqliteContext>((sp, builder) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                builder.UseSqlite(config.GetConnectionString("sqlite"));
            });
            services.AddScoped<ITenantStorageFactory, EfTenantStorageFactory<SqliteContext>>();
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            services.AddDbContext<DbTenantContext, MsSqlContext>((sp, builder) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                builder.UseSqlServer(config.GetConnectionString("mssql"));
            });
            services.AddScoped<ITenantStorageFactory, EfTenantStorageFactory<MsSqlContext>>();
        }
        else
        {
            throw new InvalidOperationException("A database connection string must be supplied.");
        }

        services.AddScoped<ITenantProvider>(sp =>
        {
            var context = sp.GetService<IHttpContextAccessor>()?.HttpContext;
            var accountName = context?.User
                .FindFirstValue(CustomClaimTypes.AccountName);

            var environment = sp.GetRequiredService<IWebHostEnvironment>();

            if (environment.IsDevelopment() && (context?.Request.Path == "/" || context?.Request.Path == "/ApplyDatabaseMigrations"))
            {
                return new ManualTenantProvider("test");
            }

            var environment = sp.GetRequiredService<IWebHostEnvironment>();

            if (environment.IsDevelopment() && (context?.Request.Path == "/" || context?.Request.Path == "/ApplyDatabaseMigrations"))
            {
                return new ManualTenantProvider("test");
            }

            return !string.IsNullOrEmpty(accountName)
                ? new ManualTenantProvider(accountName)
                : throw new InvalidOperationException("You should only request ITenantProvider from within an authenticated context");
        });

        // Add storage
        services.AddScoped<ITenantStorage, EfTenantStorage>();

        return services;
    }
}