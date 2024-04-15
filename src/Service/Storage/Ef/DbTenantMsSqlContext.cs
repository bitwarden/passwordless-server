using Microsoft.EntityFrameworkCore;

namespace Passwordless.Service.Storage.Ef;

public class DbTenantMsSqlContext : DbTenantContext
{
    public DbTenantMsSqlContext(DbContextOptions<DbTenantMsSqlContext> options, ITenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}