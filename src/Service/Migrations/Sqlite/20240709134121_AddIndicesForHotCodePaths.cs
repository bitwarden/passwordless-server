using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddIndicesForHotCodePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EFStoredCredential_Tenant_UserId",
                table: "Credentials",
                columns: new[] { "Tenant", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EFStoredCredential_Tenant_UserId",
                table: "Credentials");
        }
    }
}
