using System.Text.Json;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public abstract class DbGlobalContext : DbContext
{
    public DbGlobalContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EFStoredCredential> Credentials => Set<EFStoredCredential>();
    public DbSet<AliasPointer> Aliases => Set<AliasPointer>();
    public DbSet<TokenKey> TokenKeys => Set<TokenKey>();
    public DbSet<ApiKeyDesc> ApiKeys => Set<ApiKeyDesc>();
    public DbSet<AccountMetaInformation> AccountInfo => Set<AccountMetaInformation>();
    public DbSet<AppFeature> AppFeatures => Set<AppFeature>();

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

        modelBuilder.Entity<AppFeature>(b =>
        {
            b.HasKey(x => x.Tenant);
            b.HasOne(x => x.Application)
                .WithOne(x => x.Features)
                .HasForeignKey<AppFeature>(x => x.Tenant)
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}