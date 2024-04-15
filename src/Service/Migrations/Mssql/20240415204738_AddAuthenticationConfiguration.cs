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
            migrationBuilder.CreateTable(
                name: "AuthenticationConfigurations",
                columns: table => new
                {
                    Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserVerificationRequirement = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TimeToLive = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EditedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationConfigurations", x => new { x.Tenant, x.Purpose });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationConfigurations");
        }
    }
}
