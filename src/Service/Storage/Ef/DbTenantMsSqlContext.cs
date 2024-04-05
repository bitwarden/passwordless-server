using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

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

        modelBuilder.Entity<AuthenticationConfiguration>()
            .Property(c => c.TimeToLive)
            .HasConversion(c => c.Ticks, c => TimeSpan.FromTicks(c));
    }
}