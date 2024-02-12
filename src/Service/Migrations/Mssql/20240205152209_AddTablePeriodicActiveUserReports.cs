using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

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
                Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                DailyActiveUsersCount = table.Column<int>(type: "int", nullable: false),
                WeeklyActiveUsersCount = table.Column<int>(type: "int", nullable: false),
                TotalUsersCount = table.Column<int>(type: "int", nullable: false)
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