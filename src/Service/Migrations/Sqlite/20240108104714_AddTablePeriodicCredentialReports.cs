using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddTablePeriodicCredentialReports : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PeriodicCredentialReports",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                CredentialsCount = table.Column<int>(type: "INTEGER", nullable: false),
                UsersCount = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PeriodicCredentialReports", x => new { x.Tenant, x.CreatedAt });
                table.ForeignKey(
                    name: "FK_PeriodicCredentialReports_AccountInfo_Tenant",
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
            name: "PeriodicCredentialReports");
    }
}