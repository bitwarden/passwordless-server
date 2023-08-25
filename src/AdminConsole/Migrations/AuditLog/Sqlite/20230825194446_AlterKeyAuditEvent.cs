using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.AuditLog.Sqlite;

/// <inheritdoc />
public partial class AlterKeyAuditEvent : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_OrganizationEvents",
            table: "OrganizationEvents");

        migrationBuilder.AlterColumn<int>(
            name: "OrganizationId",
            table: "OrganizationEvents",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_OrganizationEvents",
            table: "OrganizationEvents",
            column: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_OrganizationEvents",
            table: "OrganizationEvents");

        migrationBuilder.AlterColumn<int>(
            name: "OrganizationId",
            table: "OrganizationEvents",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_OrganizationEvents",
            table: "OrganizationEvents",
            column: "OrganizationId");
    }
}