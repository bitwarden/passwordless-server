using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Passwordless.Api.Endpoints;
using Passwordless.Common.Models.Backup;
using Passwordless.Service.Backup;

namespace Passwordless.Api.Tests.Endpoints;

public class BackupEndpointsTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task ScheduleBackupAsync_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IBackupService>();
        var expected = _fixture.Create<ScheduleBackupResponse>();
        service.Setup(x => x.ScheduleAsync()).ReturnsAsync(expected.JobId);

        // Act
        var actual = await BackupEndpoints.ScheduleAsync(service.Object);

        // Assert
        Assert.Equal(typeof(Ok<ScheduleBackupResponse>), actual.GetType());
        var actualResult = ((Ok<ScheduleBackupResponse>)actual).Value;
        Assert.Equal(expected.JobId, actualResult?.JobId);
    }

    [Fact]
    public async Task GetJobsAsync_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IBackupService>();
        var expected = _fixture.CreateMany<StatusResponse>().ToList();
        service.Setup(x => x.GetJobsAsync()).ReturnsAsync(expected);

        // Act
        var actual = await BackupEndpoints.GetJobsAsync(service.Object);

        // Assert
        Assert.Equal(typeof(Ok<IReadOnlyCollection<StatusResponse>>), actual.GetType());
        var actualResult = ((Ok<IReadOnlyCollection<StatusResponse>>)actual).Value;
        Assert.Equal(expected, actualResult);
    }
}