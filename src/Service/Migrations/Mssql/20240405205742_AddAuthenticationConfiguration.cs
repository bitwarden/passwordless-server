#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.Mssql;

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
                Purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                UserVerificationRequirement = table.Column<int>(type: "int", nullable: false),
                TimeToLive = table.Column<long>(type: "bigint", nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuthenticationConfigurations", x => x.Purpose);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuthenticationConfigurations");
    }
}