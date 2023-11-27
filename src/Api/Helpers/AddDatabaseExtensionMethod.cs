#nullable enable

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Passwordless.Api.Authorization;
using Passwordless.Service;
using Passwordless.Service.Storage.Ef;

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
            services.AddDbContext<DbGlobalContext, DbGlobalSqliteContext>((sp, builder) =>
            {
                // resolving config from SP to avoid capturing
                builder.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("sqlite:api"));
            });
            services.AddScoped<IGlobalStorageFactory, EfGlobalStorageFactory<DbGlobalSqliteContext>>();
            services.AddDbContext<DbTenantContext, DbTenantSqliteContext>((sp, builder) =>
            {
                // resolving config from SP to avoid capturing
                builder.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("sqlite:api"));
            });
            services.AddScoped<ITenantStorageFactory, EfTenantStorageFactory<DbTenantSqliteContext>>();
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            services.AddDbContext<DbGlobalContext, DbGlobalMsSqlContext>((sp, builder) =>
            {
                // resolving config from SP to avoid capturing
                builder.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString("mssql:api"));
            });
            services.AddScoped<IGlobalStorageFactory, EfGlobalStorageFactory<DbGlobalMsSqlContext>>();
            services.AddDbContext<DbTenantContext, DbTenantMsSqlContext>((sp, builder) =>
            {
                // resolving config from SP to avoid capturing
                builder.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString("mssql:api"));
            });
            services.AddScoped<ITenantStorageFactory, EfTenantStorageFactory<DbTenantMsSqlContext>>();
        }
        else
        {
            throw new InvalidOperationException("A database connection string must be supplied.");
        }

        services.AddScoped<ITenantProvider>(sp =>
        {
            var context = sp.GetService<IHttpContextAccessor>()?.HttpContext;
            var accountName = context?.User.FindFirstValue(CustomClaimTypes.AccountName);

            return !string.IsNullOrEmpty(accountName)
                ? new ManualTenantProvider(accountName, TimeProvider.System)
                : throw new InvalidOperationException("You should only request ITenantProvider from within an authenticated context");
        });

        // Add storage
        services.AddScoped<IGlobalStorage, EfGlobalGlobalStorage>();
        services.AddScoped<ITenantStorage, EfTenantStorage>();

        return services;
    }
}