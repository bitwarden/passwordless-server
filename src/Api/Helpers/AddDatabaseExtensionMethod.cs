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
        var sqlite = configuration.GetConnectionString("sqlite:api");
        var mssql = configuration.GetConnectionString("mssql:api");

        if (!string.IsNullOrEmpty(sqlite))
        {
            services.AddDbContext<DbTenantContext, SqliteContext>((sp, builder) =>
            {
                
                builder.UseSqlite(configuration.GetConnectionString("sqlite:api"));
            });
            services.AddScoped<ITenantStorageFactory, EfTenantStorageFactory<SqliteContext>>();
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            services.AddDbContext<DbTenantContext, MsSqlContext>((sp, builder) =>
            {
                builder.UseSqlServer(configuration.GetConnectionString("mssql:api"));
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

            // This exception allows running migrations either when developing or when self hosting
            if (context == null || (environment.IsDevelopment() && (context?.Request.Path == "/" || context?.Request.Path == "/ApplyDatabaseMigrations")))
            {
                return new ManualTenantProvider("_efmigrations");
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