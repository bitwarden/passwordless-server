﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Passwordless.Service.Storage.Ef;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite
{
    [DbContext(typeof(DbGlobalSqliteContext))]
    partial class SqliteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Passwordless.Service.EventLog.Models.ApplicationEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApiKeyId")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PerformedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("PerformedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Severity")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("ApplicationEvents");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AccountMetaInformation", b =>
                {
                    b.Property<string>("AcountName")
                        .HasColumnType("TEXT");

                    b.Property<string>("AdminEmailsSerialized")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("SubscriptionTier")
                        .HasColumnType("TEXT");

                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.HasKey("AcountName");

                    b.ToTable("AccountInfo");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AliasPointer", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.Property<string>("Alias")
                        .HasColumnType("TEXT");

                    b.Property<string>("Plaintext")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Tenant", "Alias");

                    b.ToTable("Aliases");
                });

            modelBuilder.Entity("Passwordless.Service.Models.ApiKeyDesc", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AccountName")
                        .HasColumnType("TEXT");

                    b.Property<string>("ApiKey")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastLockedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUnlockedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Scopes")
                        .HasColumnType("TEXT");

                    b.HasKey("Tenant", "Id");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AppFeature", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AllowAttestation")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DeveloperLoggingEndsAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("EventLoggingIsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EventLoggingRetentionPeriod")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsGenerateSignInTokenEndpointEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.Property<long?>("MaxUsers")
                        .HasColumnType("INTEGER");

                    b.HasKey("Tenant");

                    b.ToTable("AppFeatures");
                });

            modelBuilder.Entity("Passwordless.Service.Models.EFStoredCredential", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("DescriptorId")
                        .HasColumnType("BLOB");

                    b.Property<Guid>("AaGuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("AttestationFmt")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("BackupState")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("DescriptorTransports")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DescriptorType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Device")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("IsBackupEligible")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("IsDiscoverable")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUsedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Nickname")
                        .HasColumnType("TEXT");

                    b.Property<string>("Origin")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("PublicKey")
                        .HasColumnType("BLOB");

                    b.Property<string>("RPID")
                        .HasColumnType("TEXT");

                    b.Property<uint>("SignatureCounter")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("UserHandle")
                        .HasColumnType("BLOB");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Tenant", "DescriptorId");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("Passwordless.Service.Models.PeriodicCredentialReport", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("CredentialsCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UsersCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Tenant", "CreatedAt");

                    b.ToTable("PeriodicCredentialReports");
                });

            modelBuilder.Entity("Passwordless.Service.Models.TokenKey", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("TEXT");

                    b.Property<int>("KeyId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("KeyMaterial")
                        .HasColumnType("TEXT");

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
                    b.Navigation("Events");

                    b.Navigation("Features");

                    b.Navigation("PeriodicCredentialReports");
                });
#pragma warning restore 612, 618
        }
    }
}
