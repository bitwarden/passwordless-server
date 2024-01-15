using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

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
                Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                CredentialsCount = table.Column<int>(type: "int", nullable: false),
                UsersCount = table.Column<int>(type: "int", nullable: false)
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