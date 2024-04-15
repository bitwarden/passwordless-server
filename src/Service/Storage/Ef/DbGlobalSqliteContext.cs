using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class DbGlobalSqliteContext : DbGlobalContext
{
    public DbGlobalSqliteContext(DbContextOptions<DbGlobalSqliteContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthenticationConfiguration>()
            .Property(c => c.TimeToLive)
            .HasConversion(c => c.TotalSeconds, c => TimeSpan.FromSeconds(c));
        
        base.OnModelCreating(modelBuilder);
    }
}