#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddAuthenticationConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuthenticationConfigurations",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "TEXT", nullable: false),
                Purpose = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                UserVerificationRequirement = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TimeToLive = table.Column<double>(type: "REAL", nullable: false),
                CreatedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                EditedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                EditedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                LastUsedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuthenticationConfigurations", x => new { x.Tenant, x.Purpose });
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuthenticationConfigurations");
    }
}