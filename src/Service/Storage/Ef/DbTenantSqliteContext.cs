using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class DbTenantSqliteContext : DbTenantContext
{
    public DbTenantSqliteContext(DbContextOptions<DbTenantSqliteContext> options, ITenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }
}