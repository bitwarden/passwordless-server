using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Passwordless.Api.Endpoints;
using Passwordless.Common.Models.Reporting;
using Passwordless.Service;

namespace Passwordless.Api.Tests.Endpoints;

public class ReportingEndpointsTests
{
    private readonly Fixture _fixture = new();

    #region GetPeriodicCredentialReportsAsync
    [Fact]
    public async Task GetPeriodicCredentialReportsAsync_Returns_ExpectedResult()
    {
        // arrange
        var request = new PeriodicCredentialReportRequest(null, null);
        var reportingServiceMock = new Mock<IReportingService>();
        var expectedResponse = _fixture.CreateMany<PeriodicCredentialReportResponse>();
        reportingServiceMock.Setup(x =>
            x.GetPeriodicCredentialReportsAsync(
                It.Is<PeriodicCredentialReportRequest>(p => p == request)))
            .ReturnsAsync(expectedResponse);

        // act
        var actual = await ReportingEndpoints.GetPeriodicCredentialReportsAsync(
            request, reportingServiceMock.Object);

        // assert
        Assert.Equal(typeof(Ok<IEnumerable<PeriodicCredentialReportResponse>>), actual.GetType());
        var actualResult = (actual as Ok<IEnumerable<PeriodicCredentialReportResponse>>)!.Value;
        Assert.Equal(expectedResponse, actualResult);
        reportingServiceMock.Verify(x =>
            x.GetPeriodicCredentialReportsAsync(
                It.Is<PeriodicCredentialReportRequest>(p => p == request)));
    }
    #endregion
}