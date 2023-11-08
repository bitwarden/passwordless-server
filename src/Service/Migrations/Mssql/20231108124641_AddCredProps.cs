using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql
{
    /// <inheritdoc />
    public partial class AddCredProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BackupState",
                table: "Credentials",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackupEligible",
                table: "Credentials",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscoverable",
                table: "Credentials",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupState",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "IsBackupEligible",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "IsDiscoverable",
                table: "Credentials");
        }
    }
}
