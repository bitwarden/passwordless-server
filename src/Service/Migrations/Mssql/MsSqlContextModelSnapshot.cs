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
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Passwordless.Service.Models.AccountMetaInformation", b =>
                {
                    b.Property<string>("AcountName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AdminEmailsSerialized")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("SubscriptionTier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AcountName");

                    b.ToTable("AccountInfo");
                });

            modelBuilder.Entity("Passwordless.Service.Models.AppFeature", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("AuditLoggingIsEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("AuditLoggingRetentionPeriod")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("DeveloperLoggingEndsAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Tenant");

                    b.ToTable("AppFeatures");
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
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "KeyId");

                    b.ToTable("TokenKeys");
                });

            modelBuilder.Entity("Passwordless.Service.Storage.ApiKeyDesc", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ApiKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLockedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastUnlockedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Scopes")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "Id");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Passwordless.Service.Storage.Ef.AliasPointer", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Alias")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Plaintext")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "Alias");

                    b.ToTable("Aliases");
                });

            modelBuilder.Entity("Passwordless.Service.Storage.Ef.EFStoredCredential", b =>
                {
                    b.Property<string>("Tenant")
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("DescriptorId")
                        .HasColumnType("varbinary(900)");

                    b.Property<Guid>("AaGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AttestationFmt")
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<DateTime>("LastUsedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nickname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Origin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PublicKey")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("RPID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("SignatureCounter")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("UserHandle")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Tenant", "DescriptorId");

                    b.ToTable("Credentials");
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

            modelBuilder.Entity("Passwordless.Service.Models.AccountMetaInformation", b =>
                {
                    b.Navigation("Features");
                });
#pragma warning restore 612, 618
        }
    }
}
