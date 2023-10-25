using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class RenameAuditLogToEventLog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "AuditLoggingRetentionPeriod",
            table: "AppFeatures",
            newName: "EventLoggingRetentionPeriod");

        migrationBuilder.RenameColumn(
            name: "AuditLoggingIsEnabled",
            table: "AppFeatures",
            newName: "EventLoggingIsEnabled");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "EventLoggingRetentionPeriod",
            table: "AppFeatures",
            newName: "AuditLoggingRetentionPeriod");

        migrationBuilder.RenameColumn(
            name: "EventLoggingIsEnabled",
            table: "AppFeatures",
            newName: "AuditLoggingIsEnabled");
    }
}