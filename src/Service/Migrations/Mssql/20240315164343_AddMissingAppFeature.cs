#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Passwordless.Service.Migrations.MsSql;

/// <inheritdoc />
public partial class AddMissingAppFeature : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
                             insert into AppFeatures (Tenant,
                                                      EventLoggingIsEnabled,
                                                      EventLoggingRetentionPeriod,
                                                      DeveloperLoggingEndsAt,
                                                      MaxUsers,
                                                      IsGenerateSignInTokenEndpointEnabled,
                                                      IsMagicLinksEnabled,
                                                      AllowAttestation,
                                                      MagicLinkEmailMonthlyQuota)
                             SELECT ai.Tenant, 0, 0, NULL, NULL, 1, 1, 0, 2000
                             FROM AccountInfo ai
                             LEFT JOIN AppFeatures af ON ai.Tenant = af.Tenant
                             WHERE af.Tenant IS NULL
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}