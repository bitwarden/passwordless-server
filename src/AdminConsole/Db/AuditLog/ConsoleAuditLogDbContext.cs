using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.AuditLog.Models;

namespace AdminConsole.Db.AuditLog;

public class ConsoleAuditLogDbContext : DbContext
{
    public ConsoleAuditLogDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<OrganizationAuditEvent> OrganizationEvents => Set<OrganizationAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationAuditEvent>()
            .HasKey(x => x.Id);

        base.OnModelCreating(modelBuilder);
    }
}