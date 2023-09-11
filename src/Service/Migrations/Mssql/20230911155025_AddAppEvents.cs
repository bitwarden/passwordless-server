using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

/// <inheritdoc />
public partial class AddAppEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ApiKeyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventType = table.Column<int>(type: "int", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Severity = table.Column<int>(type: "int", nullable: false),
                PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false)
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