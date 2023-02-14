using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tenant",
                table: "TokenKeys",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tenant",
                table: "Credentials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tenant",
                table: "ApiKeys",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tenant",
                table: "Aliases",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tenant",
                table: "AccountInfo",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tenant",
                table: "TokenKeys");

            migrationBuilder.DropColumn(
                name: "Tenant",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "Tenant",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "Tenant",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "Tenant",
                table: "AccountInfo");
        }
    }
}
