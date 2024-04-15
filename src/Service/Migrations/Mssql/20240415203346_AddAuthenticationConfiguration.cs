#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.Mssql
{
    /// <inheritdoc />
    public partial class AddAuthenticationConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthenticationConfigurations",
                table: "AuthenticationConfigurations");

            migrationBuilder.AlterColumn<string>(
                name: "UserVerificationRequirement",
                table: "AuthenticationConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Tenant",
                table: "AuthenticationConfigurations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AuthenticationConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AuthenticationConfigurations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EditedBy",
                table: "AuthenticationConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedOn",
                table: "AuthenticationConfigurations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedOn",
                table: "AuthenticationConfigurations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthenticationConfigurations",
                table: "AuthenticationConfigurations",
                columns: new[] { "Tenant", "Purpose" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthenticationConfigurations",
                table: "AuthenticationConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AuthenticationConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AuthenticationConfigurations");

            migrationBuilder.DropColumn(
                name: "EditedBy",
                table: "AuthenticationConfigurations");

            migrationBuilder.DropColumn(
                name: "EditedOn",
                table: "AuthenticationConfigurations");

            migrationBuilder.DropColumn(
                name: "LastUsedOn",
                table: "AuthenticationConfigurations");

            migrationBuilder.AlterColumn<int>(
                name: "UserVerificationRequirement",
                table: "AuthenticationConfigurations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Tenant",
                table: "AuthenticationConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthenticationConfigurations",
                table: "AuthenticationConfigurations",
                column: "Purpose");
        }
    }
}
