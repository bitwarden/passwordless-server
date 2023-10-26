using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwordless.AdminConsole.Migrations.Mssql;

/// <inheritdoc />
public partial class BillingRefactoring : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BillingPlan",
            table: "Organizations",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "BillingSubscriptionItemId",
            table: "Organizations",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.Sql("UPDATE Organizations SET BillingPlan = a.BillingPlan, BillingSubscriptionItemId = a.BillingSubscriptionItemId FROM Organizations o INNER JOIN Applications a ON o.Id = a.OrganizationId;");

        migrationBuilder.DropColumn(
            name: "BillingPlan",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "BillingPriceId",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "BillingSubscriptionItemId",
            table: "Applications");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new InvalidOperationException("Cannot revert this migration.");
    }
}