using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef.Tenant;

public class DbTenantSqliteContext : DbTenantContext
{
    public DbTenantSqliteContext(DbContextOptions<DbTenantSqliteContext> options, ITenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }
}