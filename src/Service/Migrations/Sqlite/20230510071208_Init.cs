using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountInfo",
                columns: table => new
                {
                    AcountName = table.Column<string>(type: "TEXT", nullable: false),
                    SubscriptionTier = table.Column<string>(type: "TEXT", nullable: true),
                    AdminEmailsSerialized = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeleteAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tenant = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountInfo", x => x.AcountName);
                });

            migrationBuilder.CreateTable(
                name: "Aliases",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "TEXT", nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Plaintext = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aliases", x => new { x.Tenant, x.Alias });
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    Scopes = table.Column<string>(type: "TEXT", nullable: true),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLockedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUnlockedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => new { x.Tenant, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "TEXT", nullable: false),
                    DescriptorId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DescriptorType = table.Column<int>(type: "INTEGER", nullable: true),
                    DescriptorTransports = table.Column<string>(type: "TEXT", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "BLOB", nullable: true),
                    UserHandle = table.Column<byte[]>(type: "BLOB", nullable: true),
                    SignatureCounter = table.Column<uint>(type: "INTEGER", nullable: false),
                    AttestationFmt = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AaGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RPID = table.Column<string>(type: "TEXT", nullable: true),
                    Origin = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    Device = table.Column<string>(type: "TEXT", nullable: true),
                    Nickname = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => new { x.Tenant, x.DescriptorId });
                });

            migrationBuilder.CreateTable(
                name: "TokenKeys",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "TEXT", nullable: false),
                    KeyId = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyMaterial = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenKeys", x => new { x.Tenant, x.KeyId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountInfo");

            migrationBuilder.DropTable(
                name: "Aliases");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "TokenKeys");
        }
    }
}
