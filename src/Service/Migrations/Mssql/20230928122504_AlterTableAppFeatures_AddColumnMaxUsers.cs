#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.Mssql;

/// <inheritdoc />
public partial class AlterTableAppFeatures_AddColumnMaxUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "MaxUsers",
            table: "AppFeatures",
            type: "int",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MaxUsers",
            table: "AppFeatures");
    }
}