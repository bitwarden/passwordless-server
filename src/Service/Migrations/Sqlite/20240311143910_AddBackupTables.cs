using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.Service.Migrations.Sqlite;

/// <inheritdoc />
public partial class AddBackupTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ArchiveJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                Status = table.Column<short>(type: "INTEGER", nullable: false),
                Tenant = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArchiveJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_ArchiveJobs_AccountInfo_Tenant",
                    column: x => x.Tenant,
                    principalTable: "AccountInfo",
                    principalColumn: "AcountName",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Archives",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                Entity = table.Column<string>(type: "TEXT", nullable: true),
                Data = table.Column<byte[]>(type: "BLOB", maxLength: 104857600, nullable: false),
                ArchiveJobId = table.Column<Guid>(type: "TEXT", nullable: true),
                Tenant = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Archives", x => x.Id);
                table.ForeignKey(
                    name: "FK_Archives_AccountInfo_Tenant",
                    column: x => x.Tenant,
                    principalTable: "AccountInfo",
                    principalColumn: "AcountName",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Archives_ArchiveJobs_ArchiveJobId",
                    column: x => x.ArchiveJobId,
                    principalTable: "ArchiveJobs",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ArchiveJobs_Tenant",
            table: "ArchiveJobs",
            column: "Tenant");

        migrationBuilder.CreateIndex(
            name: "IX_Archives_ArchiveJobId",
            table: "Archives",
            column: "ArchiveJobId");

        migrationBuilder.CreateIndex(
            name: "IX_Archives_Tenant_JobId_Id",
            table: "Archives",
            columns: new[] { "Tenant", "JobId", "Id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Archives");

        migrationBuilder.DropTable(
            name: "ArchiveJobs");
    }
}