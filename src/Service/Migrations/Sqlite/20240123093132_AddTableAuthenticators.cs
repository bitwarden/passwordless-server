using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddTableAuthenticators : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Authenticators",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "TEXT", nullable: false),
                AaGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                IsAllowed = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Authenticators", x => new { x.Tenant, x.AaGuid });
                table.ForeignKey(
                    name: "FK_Authenticators_AppFeatures_Tenant",
                    column: x => x.Tenant,
                    principalTable: "AppFeatures",
                    principalColumn: "Tenant",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Authenticators");
    }
}