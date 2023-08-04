using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Passwordless.Api.Helpers;
using Passwordless.Api.Models;
using Passwordless.Server.Endpoints;
using Passwordless.Service;

namespace Passwordless.Api.Tests.Endpoints;

public class AccountEndpointsTests
{
    #region MarkDeleteApplicationAsync
    [Fact]
    public async Task MarkDeleteApplicationAsync_Returns_ExpectedResult()
    {
        var payload = new MarkDeleteAppDto { AppId = "demo-application", DeletedBy = "admin@example.com" };
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var deletedAt = new DateTime(2023, 08, 02, 16, 13, 00);
        sharedManagementServiceMock.Setup(x => x.MarkDeleteApplicationAsync(
            It.Is<string>(p => p == "demo-application"),
            It.Is<string>(p => p == payload.DeletedBy),
            It.Is<string>(p => p == "http://localhost:7001")))
            .ReturnsAsync(new AppDeletionResult("Success!", true, deletedAt));
        var httpContextAccessorMock = new Mock<IRequestContext>();
        httpContextAccessorMock.Setup(x => x.GetBaseUrl()).Returns("http://localhost:7001");
        var loggerMock = new Mock<ILogger>();

        var actual = await AppsEndpoints.MarkDeleteApplicationAsync(
            payload,
            sharedManagementServiceMock.Object,
            httpContextAccessorMock.Object,
            loggerMock.Object);

        Assert.Equal(typeof(Ok<AppDeletionResult>), actual.GetType());
        var actualResult = (actual as Ok<AppDeletionResult>)?.Value;
        Assert.Equal("Success!", actualResult?.Message);
        Assert.True(actualResult?.IsDeleted);
        Assert.Equal(deletedAt, actualResult?.DeleteAt);
    }
    #endregion

    #region DeleteApplicationAsync
    [Fact]
    public async Task DeleteApplicationAsync_Returns_ExpectedResult()
    {
        var payload = new AppIdDTO() { AppId = "demo-application" };
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var deletedAt = new DateTime(2023, 08, 02, 16, 13, 00);
        sharedManagementServiceMock.Setup(x => x.DeleteApplicationAsync(
                It.Is<string>(p => p == "demo-application")))
            .ReturnsAsync(new AppDeletionResult("Success!", true, deletedAt));
        var loggerMock = new Mock<ILogger>();

        var actual = await AppsEndpoints.DeleteApplicationAsync(payload, sharedManagementServiceMock.Object, loggerMock.Object);

        Assert.Equal(typeof(Ok<AppDeletionResult>), actual.GetType());
        var actualResult = (actual as Ok<AppDeletionResult>)?.Value;
        Assert.Equal("Success!", actualResult?.Message);
        Assert.True(actualResult?.IsDeleted);
        Assert.Equal(deletedAt, actualResult?.DeleteAt);
    }
    #endregion

    #region GetApplicationsPendingDeletionAsync
    [Fact]
    public async Task GetApplicationsPendingDeletionAsync_Returns_ExpectedResult()
    {
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var expected = new List<string> { "app1", "app2" };
        sharedManagementServiceMock.Setup(x => x.GetApplicationsPendingDeletionAsync())
            .ReturnsAsync(expected);

        var actual = await AppsEndpoints.GetApplicationsPendingDeletionAsync(sharedManagementServiceMock.Object);

        Assert.Equal(typeof(Ok<IEnumerable<string>>), actual.GetType());
        var actualResult = (actual as Ok<IEnumerable<string>>)?.Value;
        Assert.Equal(expected, actualResult);
    }
    #endregion
}