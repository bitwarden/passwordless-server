using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql
{
    /// <inheritdoc />
    public partial class AppEventFkAccountInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ApplicationEvents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEvents_TenantId",
                table: "ApplicationEvents",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationEvents_AccountInfo_TenantId",
                table: "ApplicationEvents",
                column: "TenantId",
                principalTable: "AccountInfo",
                principalColumn: "AcountName",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationEvents_AccountInfo_TenantId",
                table: "ApplicationEvents");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationEvents_TenantId",
                table: "ApplicationEvents");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ApplicationEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
