using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Helpers;
using Passwordless.Common.Models.Apps;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;

namespace Passwordless.Api.Tests.Endpoints;

public class AppsEndpointsTests
{
    private readonly Fixture _fixture = new();

    #region IsAppIdAvailableAsync
    [Fact]
    public async Task IsAppIdAvailableAsync_Returns_Ok_WhenAppIdIsAvailable()
    {
        var payload = new GetAppIdAvailabilityRequest("myapp");
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        sharedManagementServiceMock.Setup(x => x.IsAvailableAsync(
                It.Is<string>(p => p == "myapp")))
            .ReturnsAsync(true);

        var actual = await AppsEndpoints.IsAppIdAvailableAsync(
            payload,
            sharedManagementServiceMock.Object);

        Assert.Equal(typeof(Ok<GetAppIdAvailabilityResponse>), actual.GetType());
        var actualResult = (actual as Ok<GetAppIdAvailabilityResponse>)?.Value;
        Assert.True(actualResult!.Available);
        sharedManagementServiceMock.Verify(x => x.IsAvailableAsync(It.Is<string>(p => p == "myapp")), Times.Once());
    }

    [Fact]
    public async Task IsAppIdAvailableAsync_Returns_Ok_WhenAppIdIsUnavailable()
    {
        var payload = new GetAppIdAvailabilityRequest("myapp");
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        sharedManagementServiceMock.Setup(x => x.IsAvailableAsync(
                It.Is<string>(p => p == "myapp")))
            .ReturnsAsync(false);

        var actual = await AppsEndpoints.IsAppIdAvailableAsync(
            payload,
            sharedManagementServiceMock.Object);

        Assert.Equal(typeof(Ok<GetAppIdAvailabilityResponse>), actual.GetType());
        var actualResult = (actual as Ok<GetAppIdAvailabilityResponse>)?.Value;
        Assert.False(actualResult!.Available);
        sharedManagementServiceMock.Verify(x => x.IsAvailableAsync(It.Is<string>(p => p == "myapp")), Times.Once());
    }
    #endregion

    #region MarkDeleteApplicationAsync
    [Fact]
    public async Task MarkDeleteApplicationAsync_Returns_ExpectedResult()
    {
        var appId = "demo-application";
        var payload = new MarkDeleteApplicationRequest(appId, "admin@example.com");
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var deletedAt = new DateTime(2023, 08, 02, 16, 13, 00);
        sharedManagementServiceMock.Setup(x => x.MarkDeleteApplicationAsync(
            It.Is<string>(p => p == "demo-application"),
            It.Is<string>(p => p == payload.DeletedBy),
            It.Is<string>(p => p == "http://localhost:7001")))
            .ReturnsAsync(new AppDeletionResult("Success!", true, deletedAt, new[] { "jonas@bw.com" }));
        var httpContextAccessorMock = new Mock<IRequestContext>();
        httpContextAccessorMock.Setup(x => x.GetBaseUrl()).Returns("http://localhost:7001");
        var loggerMock = new Mock<ILogger>();
        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(new NullFeaturesContext());
        var eventLoggerMock = new Mock<IEventLogger>();

        var actual = await AppsEndpoints.MarkDeleteApplicationAsync(
            appId,
            payload,
            sharedManagementServiceMock.Object,
            httpContextAccessorMock.Object,
            loggerMock.Object,
            eventLoggerMock.Object);

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
        var appId = "demo-application";
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var deletedAt = new DateTime(2023, 08, 02, 16, 13, 00);
        sharedManagementServiceMock.Setup(x => x.DeleteApplicationAsync(
                It.Is<string>(p => p == "demo-application")))
            .ReturnsAsync(new AppDeletionResult("Success!", true, deletedAt, new[] { "jonas@bw.com" }));
        var loggerMock = new Mock<ILogger>();

        var actual = await AppsEndpoints.DeleteApplicationAsync(appId, sharedManagementServiceMock.Object, loggerMock.Object);

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

    #region ManageFeaturesAsync
    [Fact]
    public async Task ManageFeaturesAsync_Returns_ExpectedResult()
    {
        const string appId = "myappid";
        var payload = new ManageFeaturesRequest
        {
            EventLoggingRetentionPeriod = 14,
            EventLoggingIsEnabled = true,
            MaxUsers = 69L
        };
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        var actual = await AppsEndpoints.ManageFeaturesAsync(appId, payload, sharedManagementServiceMock.Object);

        sharedManagementServiceMock.Verify(x =>
            x.SetFeaturesAsync(
                It.Is<string>(p => p == appId),
                It.Is<ManageFeaturesRequest>(p => p == payload)),
            Times.Once);
        Assert.Equal(typeof(NoContent), actual.GetType());
    }
    #endregion

    #region SetFeaturesAsync
    [Fact]
    public async Task SetFeaturesAsync_Returns_ExpectedResult()
    {
        var payload = new SetFeaturesRequest
        {
            EventLoggingRetentionPeriod = 14
        };
        var applicationServiceMock = new Mock<IApplicationService>();

        var actual = await AppsEndpoints.SetFeaturesAsync(payload, applicationServiceMock.Object);

        Assert.Equal(typeof(NoContent), actual.GetType());
    }
    #endregion
}