using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Backup;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Backup;

public class ScheduleTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_can_schedule_a_backup_for_an_existing_application()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        client.AddPublicKey(accountKeysCreation!.ApiKey1);

        // Act
        var actual = await client.PostAsJsonAsync("/backup/schedule", (string?)null);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualScheduleResponse = await actual.Content.ReadFromJsonAsync<ScheduleBackupResponse>();
        actualScheduleResponse.Should().NotBeNull();
        actualScheduleResponse!.JobId.Should().NotBeEmpty();
    }
}