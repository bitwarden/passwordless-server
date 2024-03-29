using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Passwordless.Api.Endpoints;
using Passwordless.Common.Models.Reporting;
using Passwordless.Service;

namespace Passwordless.Api.Tests.Endpoints;

public class ReportingEndpointsTests
{
    private readonly Fixture _fixture = new();

    public ReportingEndpointsTests()
    {
        _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));
    }

    #region GetPeriodicCredentialReportsAsync
    [Fact]
    public async Task GetPeriodicCredentialReportsAsync_Returns_ExpectedResult()
    {
        // arrange
        var request = new PeriodicCredentialReportRequest(null, null);
        var reportingServiceMock = new Mock<IReportingService>();
        var expectedResponse = _fixture.CreateMany<PeriodicCredentialReportResponse>().ToList();
        reportingServiceMock.Setup(x =>
            x.GetPeriodicCredentialReportsAsync(
                It.Is<PeriodicCredentialReportRequest>(p => p == request)))
            .ReturnsAsync(expectedResponse);

        // act
        var actual = await ReportingEndpoints.GetPeriodicCredentialReportsAsync(
            request, reportingServiceMock.Object);

        // assert
        Assert.Equal(typeof(Ok<IEnumerable<PeriodicCredentialReportResponse>>), actual.GetType());
        var actualResult = (actual as Ok<IEnumerable<PeriodicCredentialReportResponse>>)!.Value!;
        Assert.Equal(expectedResponse, actualResult);
        reportingServiceMock.Verify(x =>
            x.GetPeriodicCredentialReportsAsync(
                It.Is<PeriodicCredentialReportRequest>(p => p == request)));
    }
    #endregion

    #region GetPeriodicActiveUserReportsAsync
    [Fact]
    public async Task GetPeriodicActiveUserReportsAsync_Returns_ExpectedResult()
    {
        // arrange
        var request = new PeriodicActiveUserReportRequest(null, null);
        var reportingServiceMock = new Mock<IReportingService>();
        var expectedResponse = _fixture.CreateMany<PeriodicActiveUserReportResponse>().ToList();
        reportingServiceMock.Setup(x =>
                x.GetPeriodicActiveUserReportsAsync(
                    It.Is<PeriodicActiveUserReportRequest>(p => p == request)))
            .ReturnsAsync(expectedResponse);

        // act
        var actual = await ReportingEndpoints.GetPeriodicActiveUserReportsAsync(
            request, reportingServiceMock.Object);

        // assert
        Assert.Equal(typeof(Ok<IEnumerable<PeriodicActiveUserReportResponse>>), actual.GetType());
        var actualResult = (actual as Ok<IEnumerable<PeriodicActiveUserReportResponse>>)!.Value!;
        Assert.Equal(expectedResponse, actualResult);
        reportingServiceMock.Verify(x =>
            x.GetPeriodicActiveUserReportsAsync(
                It.Is<PeriodicActiveUserReportRequest>(p => p == request)));
    }
    #endregion
}