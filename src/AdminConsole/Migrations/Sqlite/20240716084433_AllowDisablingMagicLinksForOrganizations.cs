using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.Sqlite;

/// <inheritdoc />
public partial class AllowDisablingMagicLinksForOrganizations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsMagicLinksEnabled",
            table: "Organizations",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsMagicLinksEnabled",
            table: "Organizations");
    }
}