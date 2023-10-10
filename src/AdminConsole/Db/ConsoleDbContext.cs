using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passwordless.AdminConsole.EventLog.Models;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Db;

public class ConsoleDbContext : IdentityDbContext<ConsoleAdmin, IdentityRole, string>, IDataProtectionKeyContext
{
    public ConsoleDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Onboarding> Onboardings { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public DbSet<OrganizationEvent> OrganizationEvents => Set<OrganizationEvent>();
    public DbSet<DataProtectionKey> DataProtectionKeys { set; get; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        EntityTypeBuilder<Application> application = builder.Entity<Application>();
        application.HasOne<Onboarding>(a => a.Onboarding);

        builder.Entity<Onboarding>(o =>
        {
            o.HasKey(o => o.ApplicationId);
        });

        builder.Entity<Invite>(o =>
        {
            o.HasKey(i => i.HashedCode);
        });

        builder.Entity<OrganizationEvent>()
            .HasKey(x => x.Id);

        base.OnModelCreating(builder);
    }
}