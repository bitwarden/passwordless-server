using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddTablePeriodicActiveUserReports : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PeriodicActiveUserReports",
            columns: table => new
            {
                Tenant = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateOnly>(type: "TEXT", nullable: false),
                DailyActiveUsersCount = table.Column<int>(type: "INTEGER", nullable: false),
                WeeklyActiveUsersCount = table.Column<int>(type: "INTEGER", nullable: false),
                TotalUsersCount = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PeriodicActiveUserReports", x => new { x.Tenant, x.CreatedAt });
                table.ForeignKey(
                    name: "FK_PeriodicActiveUserReports_AccountInfo_Tenant",
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
            name: "PeriodicActiveUserReports");
    }
}