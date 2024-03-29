﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddOrgEventTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OrganizationEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                EventType = table.Column<int>(type: "INTEGER", nullable: false),
                Message = table.Column<string>(type: "TEXT", nullable: false),
                Severity = table.Column<int>(type: "INTEGER", nullable: false),
                PerformedBy = table.Column<string>(type: "TEXT", nullable: false),
                Subject = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationEvents", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrganizationEvents");
    }
}