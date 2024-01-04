using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

/// <inheritdoc />
public partial class AddTablePeriodicCredentialReports : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PeriodicCredentialReport_AccountInfo_Tenant",
            table: "PeriodicCredentialReport");

        migrationBuilder.RenameTable(
            name: "PeriodicCredentialReport",
            newName: "PeriodicCredentialReports");

        migrationBuilder.AlterColumn<string>(
            name: "Tenant",
            table: "PeriodicCredentialReports",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldNullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "PeriodicCredentialReports",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<int>(
            name: "CredentialsCount",
            table: "PeriodicCredentialReports",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "UsersCount",
            table: "PeriodicCredentialReports",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddPrimaryKey(
            name: "PK_PeriodicCredentialReports",
            table: "PeriodicCredentialReports",
            columns: new[] { "Tenant", "CreatedAt" });

        migrationBuilder.AddForeignKey(
            name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
            table: "PeriodicCredentialReports",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
            table: "PeriodicCredentialReports");

        migrationBuilder.DropPrimaryKey(
            name: "PK_PeriodicCredentialReports",
            table: "PeriodicCredentialReports");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "PeriodicCredentialReports");

        migrationBuilder.DropColumn(
            name: "CredentialsCount",
            table: "PeriodicCredentialReports");

        migrationBuilder.DropColumn(
            name: "UsersCount",
            table: "PeriodicCredentialReports");

        migrationBuilder.RenameTable(
            name: "PeriodicCredentialReports",
            newName: "PeriodicCredentialReport");

        migrationBuilder.AlterColumn<string>(
            name: "Tenant",
            table: "PeriodicCredentialReport",
            type: "nvarchar(450)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AddForeignKey(
            name: "FK_PeriodicCredentialReport_AccountInfo_Tenant",
            table: "PeriodicCredentialReport",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);
    }
}