using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddTableAddEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<string>(type: "TEXT", nullable: true),
                ApiKeyId = table.Column<string>(type: "TEXT", nullable: true),
                PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                EventType = table.Column<int>(type: "INTEGER", nullable: false),
                Message = table.Column<string>(type: "TEXT", nullable: false),
                Severity = table.Column<int>(type: "INTEGER", nullable: false),
                PerformedBy = table.Column<string>(type: "TEXT", nullable: false),
                Subject = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationEvents", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApplicationEvents");
    }
}