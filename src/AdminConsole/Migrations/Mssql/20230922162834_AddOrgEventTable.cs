using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.Mssql;

/// <inheritdoc />
public partial class AddOrgEventTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OrganizationEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventType = table.Column<int>(type: "int", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Severity = table.Column<int>(type: "int", nullable: false),
                PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationEvents", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrganizationEvents");
    }
}