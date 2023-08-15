using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddTableAppFeatures : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppFeatures",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "TEXT", nullable: false),
                AuditLoggingIsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                AuditLoggingRetentionPeriod = table.Column<bool>(type: "INTEGER", nullable: false),
                DeveloperLoggingEndsAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppFeatures", x => x.Tenant);
                table.ForeignKey(
                    name: "FK_AppFeatures_AccountInfo_Tenant",
                    column: x => x.Tenant,
                    principalTable: "AccountInfo",
                    principalColumn: "AcountName",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppFeatures");
    }
}