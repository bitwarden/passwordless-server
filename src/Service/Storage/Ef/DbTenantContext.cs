using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public abstract class DbTenantContext : DbGlobalContext
{
    private readonly ITenantProvider _tenantProvider;

    protected DbTenantContext(
        DbContextOptions options,
        ITenantProvider tenantProvider
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EFStoredCredential>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<TokenKey>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<AccountMetaInformation>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<ApiKeyDesc>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<AliasPointer>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<AppFeature>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<ApplicationEvent>().HasQueryFilter(c => c.TenantId == _tenantProvider.Tenant);
        modelBuilder.Entity<PeriodicCredentialReport>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<PeriodicActiveUserReport>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);
        modelBuilder.Entity<Authenticator>().HasQueryFilter(c => c.Tenant == _tenantProvider.Tenant);

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
            // Make sure that Tenant is set on the entities that require it.
            // This should already be enforced by the compiler, but just in case
            // we'll do it here as well.
            if (entry.Entity is PerTenant trackable)
            {
                if (string.IsNullOrEmpty(trackable.Tenant) || trackable.Tenant != _tenantProvider.Tenant)
                {
                    // TODO: log a warning here
                }

                trackable.Tenant = _tenantProvider.Tenant;
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("Tenant").IsModified = false;
                }
            }
        }
    }
}