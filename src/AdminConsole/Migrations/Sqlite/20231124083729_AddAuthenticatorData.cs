using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddAuthenticatorData : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Authenticators",
            columns: table => new
            {
                AaGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Icon = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Authenticators", x => x.AaGuid);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Authenticators");
    }
}