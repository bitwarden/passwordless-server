using System;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Models;
using Service.Storage;
using static Service.Storage.TableStorage;


public class DbTenantContext : DbContext
{
    public DbTenantContext(string tenant) : base()
    {
    }

    public DbTenantContext(
        DbContextOptions<DbTenantContext> options,
        ITenantProvider tenantProvider
        ) : base(options)
    {
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
            b.HasKey(x => x.DescriptorId);
            b.Property(x => x.DescriptorTransports).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize<Fido2NetLib.Objects.AuthenticatorTransport[]>(v, jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<Fido2NetLib.Objects.AuthenticatorTransport[]>(v, jsonOptions));

        });

        modelBuilder.Entity<TokenKey>()
        .HasKey(c => c.KeyId);


        modelBuilder.Entity<AccountMetaInformation>()
        .Ignore(c => c.AdminEmails)
        .HasKey(x => x.AcountName);

        modelBuilder.Entity<ApiKeyDesc>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Scopes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

        });

        modelBuilder.Entity<AliasPointer>()
        .HasKey(x => new { x.UserId, x.Alias });

        base.OnModelCreating(modelBuilder);
    }
}