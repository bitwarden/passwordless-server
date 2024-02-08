using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql
{
    /// <inheritdoc />
    public partial class DispatchedEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MagicLinkEmailMonthlyQuota",
                table: "AppFeatures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DispatchedEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tenant = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchedEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchedEmails_AccountInfo_Tenant",
                        column: x => x.Tenant,
                        principalTable: "AccountInfo",
                        principalColumn: "AcountName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchedEmails_Tenant",
                table: "DispatchedEmails",
                column: "Tenant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchedEmails");

            migrationBuilder.DropColumn(
                name: "MagicLinkEmailMonthlyQuota",
                table: "AppFeatures");
        }
    }
}
