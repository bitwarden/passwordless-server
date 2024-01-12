#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.Mssql
{
    /// <inheritdoc />
    public partial class AddMagicLinksSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMagicLinksEnabled",
                table: "AppFeatures",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMagicLinksEnabled",
                table: "AppFeatures");
        }
    }
}
