using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class DbTenantMsSqlContext : DbTenantContext
{
    public DbTenantMsSqlContext(
        DbContextOptions<DbTenantMsSqlContext> options,
        ITenantProvider tenantProvider,
        TimeProvider timeProvider)
        : base(options, tenantProvider, timeProvider)
    {
    }
}