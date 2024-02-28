using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Mssql;

/// <inheritdoc />
public partial class RemoveObsoleteColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AccountName",
            table: "ApiKeys");

        migrationBuilder.DropColumn(
            name: "SubscriptionTier",
            table: "AccountInfo");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AccountName",
            table: "ApiKeys",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SubscriptionTier",
            table: "AccountInfo",
            type: "nvarchar(max)",
            nullable: true);
    }
}