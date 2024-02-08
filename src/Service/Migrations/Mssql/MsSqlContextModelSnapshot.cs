﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Passwordless.Service.Storage.Ef;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql
{
    [DbContext(typeof(DbGlobalMsSqlContext))]
    partial class MsSqlContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Passwordless.Service.EventLog.Models.ApplicationEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ApiKeyId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("PerformedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("PerformedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Severity")
                        .HasColumnType("int");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("ApplicationEvents");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AccountMetaInformation", b =>
                {
                    b.Property<string>("AcountName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AdminEmailsSerialized")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("SubscriptionTier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tenant")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AcountName");

                    b.ToTable("AccountInfo");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AliasPointer", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Alias")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Plaintext")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "Alias");

                    b.ToTable("Aliases");
                });

            modelBuilder.Entity("Passwordless.Service.Models.ApiKeyDesc", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ApiKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLockedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastUnlockedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Scopes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "Id");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AppFeature", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("AllowAttestation")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("DeveloperLoggingEndsAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("EventLoggingIsEnabled")
                        .HasColumnType("bit");

                    b.Property<int>("EventLoggingRetentionPeriod")
                        .HasColumnType("int");

                    b.Property<bool>("IsGenerateSignInTokenEndpointEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<bool>("IsMagicLinksEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<int>("MagicLinkEmailMaxMinutelyLimit")
                        .HasColumnType("int");

                    b.Property<int>("MagicLinkEmailMaxMonthlyLimit")
                        .HasColumnType("int");

                    b.Property<long?>("MaxUsers")
                        .HasColumnType("bigint");

                    b.HasKey("Tenant");

                    b.ToTable("AppFeatures");
                });

            modelBuilder.Entity("Passwordless.Service.Models.Authenticator", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("AaGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsAllowed")
                        .HasColumnType("bit");

                    b.HasKey("Tenant", "AaGuid");

                    b.ToTable("Authenticators");
                });

            modelBuilder.Entity("Passwordless.Service.Models.DispatchedEmail", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tenant")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Tenant");

                    b.ToTable("DispatchedEmails");
                });

            modelBuilder.Entity("Passwordless.Service.Models.EFStoredCredential", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("DescriptorId")
                        .HasColumnType("varbinary(900)");

                    b.Property<Guid?>("AaGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AttestationFmt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("BackupState")
                        .HasColumnType("bit");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DescriptorTransports")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DescriptorType")
                        .HasColumnType("int");

                    b.Property<string>("Device")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsBackupEligible")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDiscoverable")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastUsedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nickname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Origin")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PublicKey")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("RPID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("SignatureCounter")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("UserHandle")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "DescriptorId");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("Passwordless.Service.Models.PeriodicCredentialReport", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateOnly>("CreatedAt")
                        .HasColumnType("date");

                    b.Property<int>("CredentialsCount")
                        .HasColumnType("int");

                    b.Property<int>("UsersCount")
                        .HasColumnType("int");

                    b.HasKey("Tenant", "CreatedAt");

                    b.ToTable("PeriodicCredentialReports");
                });

            modelBuilder.Entity("Passwordless.Service.Models.TokenKey", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("KeyId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("KeyMaterial")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "KeyId");

                    b.ToTable("TokenKeys");
                });

            modelBuilder.Entity("Passwordless.Service.EventLog.Models.ApplicationEvent", b =>
                {
                    b.HasOne("Passwordless.Service.Models.AccountMetaInformation", "Application")
                        .WithMany("Events")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Application");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AppFeature", b =>
                {
                    b.HasOne("Passwordless.Service.Models.AccountMetaInformation", "Application")
                        .WithOne("Features")
                        .HasForeignKey("Passwordless.Service.Models.AppFeature", "Tenant")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Application");
                });

            modelBuilder.Entity("Passwordless.Service.Models.Authenticator", b =>
                {
                    b.HasOne("Passwordless.Service.Models.AppFeature", "AppFeature")
                        .WithMany("Authenticators")
                        .HasForeignKey("Tenant")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppFeature");
                });

            modelBuilder.Entity("Passwordless.Service.Models.DispatchedEmail", b =>
                {
                    b.HasOne("Passwordless.Service.Models.AccountMetaInformation", "Application")
                        .WithMany("DispatchedEmails")
                        .HasForeignKey("Tenant")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Application");
                });

            modelBuilder.Entity("Passwordless.Service.Models.PeriodicCredentialReport", b =>
                {
                    b.HasOne("Passwordless.Service.Models.AccountMetaInformation", "Application")
                        .WithMany("PeriodicCredentialReports")
                        .HasForeignKey("Tenant")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Application");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AccountMetaInformation", b =>
                {
                    b.Navigation("DispatchedEmails");

                    b.Navigation("Events");

                    b.Navigation("Features");

                    b.Navigation("PeriodicCredentialReports");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AppFeature", b =>
                {
                    b.Navigation("Authenticators");
                });
#pragma warning restore 612, 618
        }
    }
}
