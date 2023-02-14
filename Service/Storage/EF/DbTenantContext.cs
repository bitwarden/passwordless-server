using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Models;
using Service.Storage;
using static Service.Storage.TableStorage;


public class DbTenantContext : DbContext
{
    private string _tenant;

    public DbTenantContext(
        DbContextOptions<DbTenantContext> options,
        ITenantProvider tenantProvider
        ) : base(options)
    {
        _tenant = tenantProvider.Tenant;
    }

    public DbSet<EFStoredCredential> Credentials => Set<EFStoredCredential>();
    public DbSet<AliasPointer> Aliases => Set<AliasPointer>();
    public DbSet<TokenKey> TokenKeys => Set<TokenKey>();
    public DbSet<ApiKeyDesc> ApiKeys => Set<ApiKeyDesc>();
    public DbSet<AccountMetaInformation> AccountInfo => Set<AccountMetaInformation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOptions = new System.Text.Json.JsonSerializerOptions();

        modelBuilder.Entity<EFStoredCredential>(b =>
        {
            b.HasQueryFilter(c => c.Tenant == _tenant);
            b.HasKey(x => x.DescriptorId);
            b.Property(x => x.DescriptorTransports).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize<Fido2NetLib.Objects.AuthenticatorTransport[]>(v, jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<Fido2NetLib.Objects.AuthenticatorTransport[]>(v, jsonOptions));

        });

        modelBuilder.Entity<TokenKey>()
        .HasQueryFilter(c => c.Tenant == _tenant)
        .HasKey(c => c.KeyId);


        modelBuilder.Entity<AccountMetaInformation>()
        .Ignore(c => c.AdminEmails)
        .HasKey(x => x.AcountName);

        modelBuilder.Entity<ApiKeyDesc>(b =>
        {
            b.HasQueryFilter(c => c.Tenant == _tenant);
            b.HasKey(x => x.Id);
            b.Property(x => x.Scopes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

        });

        modelBuilder.Entity<AliasPointer>()
            .HasQueryFilter(c => c.Tenant == _tenant)
        .HasKey(x => new { x.UserId, x.Alias });

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
       bool acceptAllChangesOnSuccess,
       CancellationToken cancellationToken = default(CancellationToken)
    )
    {
        OnBeforeSaving();
        return (await base.SaveChangesAsync(acceptAllChangesOnSuccess,
                      cancellationToken));
    }

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // for entities that inherit from PerTenant
            // Automatically set tenant value
            if (entry.Entity is PerTenant trackable)
            {
                trackable.Tenant = _tenant;
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("Tenant").IsModified = false;
                }
            }
        }
    }
}