using System.Collections.Immutable;
using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Helpers;
using Passwordless.Api.Models;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Models;

namespace Passwordless.Api.Tests.Endpoints;

public class AccountEndpointsTests
{
    private readonly Fixture _fixture = new();

    #region MarkDeleteApplicationAsync
    [Fact]
    public async Task MarkDeleteApplicationAsync_Returns_ExpectedResult()
    {
        var appId = "demo-application";
        var payload = new MarkDeleteAppDto { DeletedBy = "admin@example.com" };
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
        var payload = new ManageFeaturesDto
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
                It.Is<ManageFeaturesDto>(p => p == payload)),
            Times.Once);
        Assert.Equal(typeof(NoContent), actual.GetType());
    }
    #endregion

    #region SetFeaturesAsync
    [Fact]
    public async Task SetFeaturesAsync_Returns_ExpectedResult()
    {
        var payload = new SetFeaturesDto
        {
            EventLoggingRetentionPeriod = 14
        };
        var applicationServiceMock = new Mock<IApplicationService>();

        var actual = await AppsEndpoints.SetFeaturesAsync(payload, applicationServiceMock.Object);

        Assert.Equal(typeof(NoContent), actual.GetType());
    }
    #endregion

    #region ListApiKeysAsync
    [Fact]
    public async Task ListApiKeysAsync_Returns_Ok_WhenSuccessful()
    {
        // Arrange
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var expectedResult = _fixture.CreateMany<ApiKeyDto>().ToImmutableList();
        sharedManagementServiceMock.Setup(x => x.ListApiKeysAsync(It.Is<string>(p => p == "myapp1")))
            .ReturnsAsync(expectedResult);

        // Act
        var actual = await AppsEndpoints.ListApiKeysAsync("myapp1", sharedManagementServiceMock.Object);

        // Assert
        Assert.Equal(typeof(Ok<IReadOnlyCollection<ApiKeyDto>>), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.ListApiKeysAsync("myapp1"), Times.Once);
        var actualResult = (actual as Ok<IReadOnlyCollection<ApiKeyDto>>)?.Value;
        Assert.Equal(expectedResult, actualResult);
    }
    #endregion

    #region CreateApiKeyAsync
    [Fact]
    public async Task CreateApiKeyAsync_Returns_Ok_WhenSuccessful()
    {
        // Arrange
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var payload = _fixture.Create<CreateApiKeyDto>();

        // Act
        var actual = await AppsEndpoints.CreateApiKeysAsync("myapp1", payload, sharedManagementServiceMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        sharedManagementServiceMock.Verify(x =>
                x.CreateApiKeysAsync(
                    It.Is<string>(p => p == "myapp1"),
                    It.Is<CreateApiKeyDto>(p => p == payload)), Times.Once);
    }
    #endregion

    #region LockApiKeyAsync
    [Fact]
    public async Task LockApiKeyAsync_Returns_NoContent_WhenSuccessful()
    {
        // Arrange
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        // Act
        var actual = await AppsEndpoints.LockApiKeyAsync("myapp1", "1234", sharedManagementServiceMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.LockApiKeyAsync("myapp1", "1234"), Times.Once);
    }
    #endregion

    #region UnlockApiKeyAsync
    [Fact]
    public async Task UnlockApiKeyAsync_Returns_NoContent_WhenSuccessful()
    {
        // Arrange
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        // Act
        var actual = await AppsEndpoints.UnlockApiKeyAsync("myapp1", "1234", sharedManagementServiceMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.UnlockApiKeyAsync("myapp1", "1234"), Times.Once);
    }
    #endregion

    #region UnlockApiKeyAsync
    [Fact]
    public async Task DeleteApiKeyAsync_Returns_NoContent_WhenSuccessful()
    {
        // Arrange
        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        // Act
        var actual = await AppsEndpoints.DeleteApiKeyAsync("myapp1", "1234", sharedManagementServiceMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.DeleteApiKeyAsync("myapp1", "1234"), Times.Once);
    }
    #endregion
}