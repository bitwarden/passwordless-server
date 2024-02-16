using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

/// <inheritdoc />
public partial class removedeprecatedcolumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppFeatures_AccountInfo_Tenant",
            table: "AppFeatures");

        migrationBuilder.DropForeignKey(
            name: "FK_ApplicationEvents_AccountInfo_TenantId",
            table: "ApplicationEvents");

        migrationBuilder.DropForeignKey(
            name: "FK_DispatchedEmails_AccountInfo_Tenant",
            table: "DispatchedEmails");

        migrationBuilder.DropForeignKey(
            name: "FK_PeriodicActiveUserReports_AccountInfo_Tenant",
            table: "PeriodicActiveUserReports");

        migrationBuilder.DropForeignKey(
            name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
            table: "PeriodicCredentialReports");

        migrationBuilder.DropPrimaryKey(
            name: "PK_AccountInfo",
            table: "AccountInfo");

        migrationBuilder.DropColumn(
            name: "AcountName",
            table: "AccountInfo");

        migrationBuilder.DropColumn(
            name: "SubscriptionTier",
            table: "AccountInfo");

        migrationBuilder.AlterColumn<string>(
            name: "Tenant",
            table: "AccountInfo",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AccountInfo",
            table: "AccountInfo",
            column: "Tenant");

        migrationBuilder.AddForeignKey(
            name: "FK_AppFeatures_AccountInfo_Tenant",
            table: "AppFeatures",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "Tenant",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ApplicationEvents_AccountInfo_TenantId",
            table: "ApplicationEvents",
            column: "TenantId",
            principalTable: "AccountInfo",
            principalColumn: "Tenant",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_DispatchedEmails_AccountInfo_Tenant",
            table: "DispatchedEmails",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "Tenant",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_PeriodicActiveUserReports_AccountInfo_Tenant",
            table: "PeriodicActiveUserReports",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "Tenant",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
            table: "PeriodicCredentialReports",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "Tenant",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AppFeatures_AccountInfo_Tenant",
            table: "AppFeatures");

        migrationBuilder.DropForeignKey(
            name: "FK_ApplicationEvents_AccountInfo_TenantId",
            table: "ApplicationEvents");

        migrationBuilder.DropForeignKey(
            name: "FK_DispatchedEmails_AccountInfo_Tenant",
            table: "DispatchedEmails");

        migrationBuilder.DropForeignKey(
            name: "FK_PeriodicActiveUserReports_AccountInfo_Tenant",
            table: "PeriodicActiveUserReports");

        migrationBuilder.DropForeignKey(
            name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
            table: "PeriodicCredentialReports");

        migrationBuilder.DropPrimaryKey(
            name: "PK_AccountInfo",
            table: "AccountInfo");

        migrationBuilder.AlterColumn<string>(
            name: "Tenant",
            table: "AccountInfo",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AddColumn<string>(
            name: "AcountName",
            table: "AccountInfo",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "SubscriptionTier",
            table: "AccountInfo",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_AccountInfo",
            table: "AccountInfo",
            column: "AcountName");

        migrationBuilder.AddForeignKey(
            name: "FK_AppFeatures_AccountInfo_Tenant",
            table: "AppFeatures",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ApplicationEvents_AccountInfo_TenantId",
            table: "ApplicationEvents",
            column: "TenantId",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_DispatchedEmails_AccountInfo_Tenant",
            table: "DispatchedEmails",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_PeriodicActiveUserReports_AccountInfo_Tenant",
            table: "PeriodicActiveUserReports",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
            table: "PeriodicCredentialReports",
            column: "Tenant",
            principalTable: "AccountInfo",
            principalColumn: "AcountName",
            onDelete: ReferentialAction.Cascade);
    }
}