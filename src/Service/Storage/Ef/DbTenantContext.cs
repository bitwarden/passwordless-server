using System.Text.Json;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class SqliteContext : DbTenantContext
{
    public SqliteContext(DbContextOptions<SqliteContext> options, ITenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }
}

public class MsSqlContext : DbTenantContext
{
    public MsSqlContext(DbContextOptions<MsSqlContext> options, ITenantProvider tenantProvider)
        : base(options, tenantProvider)
    {
    }
}

public class DbTenantContext : DbContext
{
    public string Tenant { get; }

    public DbTenantContext(
        DbContextOptions options,
        ITenantProvider tenantProvider
    ) : base(options)
    {
        Tenant = tenantProvider.Tenant;
    }



    public DbSet<EFStoredCredential> Credentials => Set<EFStoredCredential>();
    public DbSet<AliasPointer> Aliases => Set<AliasPointer>();
    public DbSet<TokenKey> TokenKeys => Set<TokenKey>();
    public DbSet<ApiKeyDesc> ApiKeys => Set<ApiKeyDesc>();
    public DbSet<AccountMetaInformation> AccountInfo => Set<AccountMetaInformation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions();

        modelBuilder.Entity<EFStoredCredential>(b =>
        {
            b.HasKey(x => new { x.Tenant, x.DescriptorId });
            b.Property(x => x.DescriptorTransports).HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<AuthenticatorTransport[]>(v, jsonOptions));
        });

        modelBuilder.Entity<TokenKey>()
            .HasKey(c => new { c.Tenant, c.KeyId });


        modelBuilder.Entity<AccountMetaInformation>()
            .Ignore(c => c.AdminEmails)
            .HasKey(x => x.AcountName);

        modelBuilder.Entity<ApiKeyDesc>(b =>
        {
            b.HasKey(x => new { x.Tenant, x.Id });
            b.Property(x => x.Scopes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
        });

        modelBuilder.Entity<AliasPointer>()
            .HasKey(x => new { x.Tenant, x.Alias });

        if (Tenant != null)
        {
            modelBuilder.Entity<EFStoredCredential>().HasQueryFilter(c => c.Tenant == Tenant);
            modelBuilder.Entity<TokenKey>().HasQueryFilter(c => c.Tenant == Tenant);
            modelBuilder.Entity<AccountMetaInformation>().HasQueryFilter(c => c.Tenant == Tenant);
            modelBuilder.Entity<ApiKeyDesc>().HasQueryFilter(c => c.Tenant == Tenant);
            modelBuilder.Entity<AliasPointer>().HasQueryFilter(c => c.Tenant == Tenant);
        }

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        OnBeforeSaving();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void OnBeforeSaving()
    {
        IEnumerable<EntityEntry> entries = ChangeTracker.Entries();

        foreach (EntityEntry entry in entries)
        {
            // for entities that inherit from PerTenant
            // Automatically set tenant value
            if (entry.Entity is PerTenant trackable)
            {
                trackable.Tenant = Tenant;
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("Tenant").IsModified = false;
                }
            }
        }
    }
}