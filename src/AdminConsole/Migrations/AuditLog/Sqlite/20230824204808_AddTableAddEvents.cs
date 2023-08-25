using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.AuditLog.Sqlite;

/// <inheritdoc />
public partial class AddTableAddEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OrganizationEvents",
            columns: table => new
            {
                OrganizationId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                EventType = table.Column<int>(type: "INTEGER", nullable: false),
                Message = table.Column<string>(type: "TEXT", nullable: false),
                Severity = table.Column<int>(type: "INTEGER", nullable: false),
                PerformedBy = table.Column<string>(type: "TEXT", nullable: false),
                Subject = table.Column<string>(type: "TEXT", nullable: false),
                ManagementKeyId = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationEvents", x => x.OrganizationId);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrganizationEvents");
    }
}