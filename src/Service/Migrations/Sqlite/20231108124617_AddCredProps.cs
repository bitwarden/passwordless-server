using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite
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
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackupEligible",
                table: "Credentials",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscoverable",
                table: "Credentials",
                type: "INTEGER",
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
