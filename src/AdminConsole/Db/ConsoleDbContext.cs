using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Passwordless.AdminConsole.EventLog.Models;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.Db.Converters;

namespace Passwordless.AdminConsole.Db;

public class ConsoleDbContext(DbContextOptions options)
    : IdentityDbContext<ConsoleAdmin, IdentityRole, string>(options), IDataProtectionKeyContext
{
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

        builder.Entity<Organization>(e =>
        {
            e.Property(p => p.IsMagicLinksEnabled)
                .HasDefaultValue(true);
            e.Property(p => p.InfoOrgType)
                .HasConversion<EnumToStringConverter<OrganizationType>>();
            e.Property(p => p.InfoUseCase)
                .HasConversion<EnumToStringConverter<UseCaseType>>();
        });

        base.OnModelCreating(builder);
    }
}