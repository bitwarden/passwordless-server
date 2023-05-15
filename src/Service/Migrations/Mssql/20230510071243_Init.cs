using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

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
                AcountName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                SubscriptionTier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AdminEmailsSerialized = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                DeleteAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Tenant = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountInfo", x => x.AcountName);
            });

        migrationBuilder.CreateTable(
            name: "Aliases",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Alias = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Plaintext = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Aliases", x => new { x.Tenant, x.Alias });
            });

        migrationBuilder.CreateTable(
            name: "ApiKeys",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsLocked = table.Column<bool>(type: "bit", nullable: false),
                LastLockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastUnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiKeys", x => new { x.Tenant, x.Id });
            });

        migrationBuilder.CreateTable(
            name: "Credentials",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                DescriptorId = table.Column<byte[]>(type: "varbinary(900)", nullable: false),
                DescriptorType = table.Column<int>(type: "int", nullable: true),
                DescriptorTransports = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                UserHandle = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                SignatureCounter = table.Column<long>(type: "bigint", nullable: false),
                AttestationFmt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                AaGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                RPID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Origin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Device = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Nickname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Credentials", x => new { x.Tenant, x.DescriptorId });
            });

        migrationBuilder.CreateTable(
            name: "TokenKeys",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                KeyId = table.Column<int>(type: "int", nullable: false),
                KeyMaterial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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