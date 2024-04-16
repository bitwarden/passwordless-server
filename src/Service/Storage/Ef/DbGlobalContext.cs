using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;
using Passwordless.Common.Utils;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef.ValueComparers;

namespace Passwordless.Service.Storage.Ef;

public abstract class DbGlobalContext : DbContext
{
    protected DbGlobalContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EFStoredCredential> Credentials => Set<EFStoredCredential>();
    public DbSet<AliasPointer> Aliases => Set<AliasPointer>();
    public DbSet<TokenKey> TokenKeys => Set<TokenKey>();
    public DbSet<ApiKeyDesc> ApiKeys => Set<ApiKeyDesc>();
    public DbSet<AccountMetaInformation> AccountInfo => Set<AccountMetaInformation>();
    public DbSet<AppFeature> AppFeatures => Set<AppFeature>();
    public DbSet<Authenticator> Authenticators => Set<Authenticator>();
    public DbSet<ApplicationEvent> ApplicationEvents => Set<ApplicationEvent>();
    public DbSet<DispatchedEmail> DispatchedEmails => Set<DispatchedEmail>();
    public DbSet<PeriodicCredentialReport> PeriodicCredentialReports => Set<PeriodicCredentialReport>();
    public DbSet<PeriodicActiveUserReport> PeriodicActiveUserReports => Set<PeriodicActiveUserReport>();
    public DbSet<AuthenticationConfiguration> AuthenticationConfigurations => Set<AuthenticationConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions();

        modelBuilder.Entity<EFStoredCredential>(b =>
        {
            b.HasKey(x => new { x.Tenant, x.DescriptorId });
            b.Property(x => x.DescriptorTransports).HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<AuthenticatorTransport[]>(v, jsonOptions))
                .Metadata.SetValueComparer(new NullableArrayValueComparer<AuthenticatorTransport>());
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
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Metadata.SetValueComparer(new ArrayValueComparer<string>());
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

            b.Property(x => x.IsGenerateSignInTokenEndpointEnabled)
                .HasDefaultValue(true);

            b.Property(x => x.IsMagicLinksEnabled)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<Authenticator>(b =>
        {
            b.HasKey(x => new { x.Tenant, x.AaGuid });

            b.HasOne(x => x.AppFeature)
                .WithMany(x => x.Authenticators)
                .HasForeignKey(x => x.Tenant)
                .IsRequired();
        });

        modelBuilder.Entity<ApplicationEvent>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Application)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.TenantId)
                .IsRequired();
        });

        modelBuilder.Entity<DispatchedEmail>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasOne(x => x.Application)
                .WithMany(x => x.DispatchedEmails)
                .HasForeignKey(x => x.Tenant)
                .IsRequired();
        });

        modelBuilder.Entity<PeriodicCredentialReport>(builder =>
        {
            builder.HasKey(x => new { x.Tenant, x.CreatedAt });
            builder.HasOne(x => x.Application)
                .WithMany(x => x.PeriodicCredentialReports)
                .HasForeignKey(x => x.Tenant)
                .IsRequired();
        });

        modelBuilder.Entity<PeriodicActiveUserReport>(builder =>
        {
            builder.HasKey(x => new { x.Tenant, x.CreatedAt });
            builder.HasOne(x => x.Application)
                .WithMany(x => x.PeriodicActiveUserReports)
                .HasForeignKey(x => x.Tenant)
                .IsRequired();
        });

        modelBuilder.Entity<AuthenticationConfiguration>(builder =>
        {
            builder.HasKey(x => new { x.Tenant, x.Purpose });
            builder.Property(x => x.UserVerificationRequirement)
                .HasMaxLength(255)
                .HasConversion(
                    x => x.ToEnumMemberValue(),
                    x => x.ToEnum<UserVerificationRequirement>());
            builder.Property(x => x.TimeToLive)
                .HasConversion(
                    x => x.TotalSeconds,
                    x => TimeSpan.FromSeconds(x));
        });

        base.OnModelCreating(modelBuilder);
    }

    public Task SeedDefaultApplicationAsync(string appName, string publicKey, string privateKey)
    {
        ApiKeys.Add(new ApiKeyDesc
        {
            Tenant = appName,
            Id = publicKey[^4..],
            ApiKey = publicKey,
            Scopes = [PublicKeyScopes.Register.GetValue(), PublicKeyScopes.Login.GetValue()]
        });

        ApiKeys.Add(new ApiKeyDesc
        {
            Tenant = appName,
            Id = privateKey[^4..],
            ApiKey = ApiKeyUtils.HashPrivateApiKey(privateKey),
            Scopes = [SecretKeyScopes.TokenRegister.GetValue(), SecretKeyScopes.TokenVerify.GetValue()]
        });

        var application = new AccountMetaInformation
        {
            Tenant = appName,
            AcountName = appName,
            AdminEmails = ["test@test.com"],
            Features = new AppFeature
            {
                AllowAttestation = false,
                EventLoggingIsEnabled = false,
                IsGenerateSignInTokenEndpointEnabled = true,
                IsMagicLinksEnabled = true,
                MagicLinkEmailMonthlyQuota = 2000,
                MaxUsers = null,
                Tenant = appName,
            }
        };

        AccountInfo.Add(application);

        return Task.CompletedTask;
    }
}