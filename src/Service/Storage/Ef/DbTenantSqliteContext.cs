using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class DbTenantSqliteContext : DbTenantContext
{
    public DbTenantSqliteContext(
        DbContextOptions<DbTenantSqliteContext> options,
        ITenantProvider tenantProvider,
        TimeProvider timeProvider)
        : base(options, tenantProvider, timeProvider)
    {
    }
}