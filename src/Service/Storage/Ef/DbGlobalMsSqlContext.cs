using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class DbGlobalMsSqlContext : DbGlobalContext
{
    public DbGlobalMsSqlContext(DbContextOptions<DbGlobalMsSqlContext> options)
        : base(options)
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