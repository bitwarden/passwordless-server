using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

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
        
        modelBuilder.Entity<AuthenticationConfiguration>()
            .Property(c => c.TimeToLive)
            .HasConversion(
                c => c.TotalSeconds,
                c => TimeSpan.FromSeconds(c));
    }
}