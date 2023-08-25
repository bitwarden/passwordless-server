using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.AuditLog.Sqlite;

/// <inheritdoc />
public partial class AddTableAddEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                EventType = table.Column<int>(type: "INTEGER", nullable: false),
                Message = table.Column<string>(type: "TEXT", nullable: true),
                Severity = table.Column<int>(type: "INTEGER", nullable: false),
                PerformedBy = table.Column<string>(type: "TEXT", nullable: true),
                Subject = table.Column<string>(type: "TEXT", nullable: true),
                TenantId = table.Column<string>(type: "TEXT", nullable: true),
                OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                ApiKeyId = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppEvents", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppEvents");
    }
}