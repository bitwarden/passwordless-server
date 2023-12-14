#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddSignInTokenEndpointSetting : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "SignInTokenEndpointEnabled",
            table: "AppFeatures",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SignInTokenEndpointEnabled",
            table: "AppFeatures");
    }
}