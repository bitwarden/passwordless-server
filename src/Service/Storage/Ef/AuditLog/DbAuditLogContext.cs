using Microsoft.EntityFrameworkCore;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.Storage.Ef.AuditLog;

public class DbAuditLogContext : DbContext
{
    public DbAuditLogContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<ApplicationAuditEvent> AppEvents => Set<ApplicationAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationAuditEvent>()
            .HasKey(x => x.Id);

        base.OnModelCreating(modelBuilder);
    }
}