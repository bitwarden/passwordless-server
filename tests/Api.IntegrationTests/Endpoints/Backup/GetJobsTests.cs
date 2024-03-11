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

public class GetJobsTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_can_retrieve_all_backup_jobs()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        client.AddPublicKey(accountKeysCreation!.ApiKey1);

        var scheduledJobResponse = await client.PostAsJsonAsync("/backup/schedule", (string?)null);
        var scheduledJob = await scheduledJobResponse.Content.ReadFromJsonAsync<ScheduleBackupResponse>();

        // Act
        var actual = await client.GetAsync("/backup/jobs");

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResult = await actual.Content.ReadFromJsonAsync<IReadOnlyCollection<StatusResponse>>();
        actualResult!.Count.Should().Be(1);
        actualResult.First().JobId.Should().Be(scheduledJob!.JobId);
        actualResult.First().Status.Should().Be(JobStatusResponse.Pending);
    }
}