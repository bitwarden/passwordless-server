using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Passwordless.AdminConsole.Endpoints;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Endpoints;

public class ApplicationEndpointsTests
{
    private readonly Fixture _fixture = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsAppIdAvailableAsync_Returns_Ok(bool isAvailable)
    {
        // Arrange
        var request = _fixture.Create<GetAppIdAvailabilityRequest>();
        var clientMock = new Mock<IPasswordlessManagementClient>();
        var expectedResponse = new GetAppIdAvailabilityResponse(isAvailable);
        clientMock.Setup(x => x.IsApplicationIdAvailableAsync(It.Is<GetAppIdAvailabilityRequest>(r => r == request)))
            .ReturnsAsync(expectedResponse);

        // Act
        var actual = await ApplicationEndpoints.IsAppIdAvailableAsync(request, clientMock.Object);

        // Assert
        Assert.IsType<Ok<GetAppIdAvailabilityResponse>>(actual);
        var actualResult = (Ok<GetAppIdAvailabilityResponse>)actual;
        Assert.Equal(200, actualResult.StatusCode);
        Assert.Equal(expectedResponse, actualResult.Value);
    }

    [Fact]
    public async Task IsAppIdAvailableAsync_Returns_ExpectedResult_FromPasswordlessApiException()
    {
        // Arrange
        var request = _fixture.Create<GetAppIdAvailabilityRequest>();
        var clientMock = new Mock<IPasswordlessManagementClient>();
        var expectedProblemDetails = new PasswordlessProblemDetails("mytype", "mytitle", 400, "mydetail", "myinstance");
        var expectedException = new PasswordlessApiException(expectedProblemDetails);
        clientMock.Setup(x => x.IsApplicationIdAvailableAsync(It.Is<GetAppIdAvailabilityRequest>(r => r == request)))
            .ThrowsAsync(expectedException);

        // Act
        var actual = await ApplicationEndpoints.IsAppIdAvailableAsync(request, clientMock.Object);

        // Assert
        Assert.IsType<JsonHttpResult<PasswordlessProblemDetails>>(actual);
        var actualResult = (JsonHttpResult<PasswordlessProblemDetails>)actual;
        Assert.Equal(400, actualResult.StatusCode);
        Assert.Equal("application/problem+json", actualResult.ContentType);
        Assert.Equal(expectedProblemDetails, actualResult.Value);
    }
}