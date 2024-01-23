using System.Collections.Immutable;
using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Helpers;
using Passwordless.Common.Models.Apps;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Api.Tests.Endpoints;

public class AppsEndpointsTests
{
    private readonly Fixture _fixture = new();

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
        var payload = new SetFeaturesDto
        {
            EventLoggingRetentionPeriod = 14
        };
        var applicationServiceMock = new Mock<IApplicationService>();

        var actual = await AppsEndpoints.SetFeaturesAsync(payload, applicationServiceMock.Object);

        Assert.Equal(typeof(NoContent), actual.GetType());
    }
    #endregion

    #region ListConfiguredAuthenticatorsAsync
    [Fact]
    public async Task ListConfiguredAuthenticatorsAsync_Throws_Forbidden_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<ConfiguredAuthenticatorRequest>();

        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await AppsEndpoints.ListConfiguredAuthenticatorsAsync(
                request,
                sharedManagementServiceMock.Object,
                featureContextProviderMock.Object));

        // Assert
        Assert.Equal("attestation_not_supported_on_plan", actual.ErrorCode);
        Assert.Equal("Attestation is not supported on your plan.", actual.Message);
        Assert.Equal(403, actual.StatusCode);
        sharedManagementServiceMock.Verify(x => x.ListConfiguredAuthenticatorsAsync(It.IsAny<ConfiguredAuthenticatorRequest>()), Times.Never);
    }

    [Fact]
    public async Task ListConfiguredAuthenticatorsAsync_Returns_Ok_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<ConfiguredAuthenticatorRequest>();

        var sharedManagementServiceMock = new Mock<ISharedManagementService>();
        var expectedResponse = _fixture.CreateMany<ConfiguredAuthenticatorResponse>().ToImmutableList();
        sharedManagementServiceMock
            .Setup(x => x.ListConfiguredAuthenticatorsAsync(It.Is<ConfiguredAuthenticatorRequest>(p => p == request)))
            .ReturnsAsync(expectedResponse);

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        featureContextProviderMock
            .Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await AppsEndpoints.ListConfiguredAuthenticatorsAsync(
                request,
                sharedManagementServiceMock.Object,
                featureContextProviderMock.Object);

        // Assert
        Assert.Equal(typeof(Ok<IEnumerable<ConfiguredAuthenticatorResponse>>), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.ListConfiguredAuthenticatorsAsync(It.Is<ConfiguredAuthenticatorRequest>(p => p == request)), Times.Once);
        var actualResult = ((Ok<IEnumerable<ConfiguredAuthenticatorResponse>>)actual).Value!;
        Assert.Equal(expectedResponse, actualResult);
    }
    #endregion

    #region WhitelistAuthenticatorsAsync
    [Fact]
    public async Task WhitelistAuthenticatorsAsync_Throws_Forbidden_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<WhitelistAuthenticatorsRequest>();

        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await AppsEndpoints.WhitelistAuthenticatorsAsync(
                request,
                sharedManagementServiceMock.Object,
                featureContextProviderMock.Object));

        // Assert
        Assert.Equal("attestation_not_supported_on_plan", actual.ErrorCode);
        Assert.Equal("Attestation is not supported on your plan.", actual.Message);
        Assert.Equal(403, actual.StatusCode);
        sharedManagementServiceMock.Verify(x => x.WhitelistAuthenticatorsAsync(It.IsAny<WhitelistAuthenticatorsRequest>()), Times.Never);
    }

    [Fact]
    public async Task WhitelistAuthenticatorsAsync_Returns_NoContent()
    {
        // Arrange
        var request = _fixture.Create<WhitelistAuthenticatorsRequest>();

        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        featureContextProviderMock
            .Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await AppsEndpoints.WhitelistAuthenticatorsAsync(
            request,
            sharedManagementServiceMock.Object,
            featureContextProviderMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.WhitelistAuthenticatorsAsync(It.Is<WhitelistAuthenticatorsRequest>(p => p == request)), Times.Once);
    }
    #endregion


    #region DelistAuthenticatorsAsync
    [Fact]
    public async Task DelistAuthenticatorsAsync_Throws_Forbidden_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<DelistAuthenticatorsRequest>();

        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await AppsEndpoints.DelistAuthenticatorsAsync(
                request,
                sharedManagementServiceMock.Object,
                featureContextProviderMock.Object));

        // Assert
        Assert.Equal("attestation_not_supported_on_plan", actual.ErrorCode);
        Assert.Equal("Attestation is not supported on your plan.", actual.Message);
        Assert.Equal(403, actual.StatusCode);
        sharedManagementServiceMock.Verify(x => x.DelistAuthenticatorsAsync(It.IsAny<DelistAuthenticatorsRequest>()), Times.Never);
    }

    [Fact]
    public async Task DelistAuthenticatorsAsync_Returns_NoContent()
    {
        // Arrange
        var request = _fixture.Create<DelistAuthenticatorsRequest>();

        var sharedManagementServiceMock = new Mock<ISharedManagementService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        featureContextProviderMock
            .Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await AppsEndpoints.DelistAuthenticatorsAsync(
            request,
            sharedManagementServiceMock.Object,
            featureContextProviderMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        sharedManagementServiceMock.Verify(x => x.DelistAuthenticatorsAsync(It.Is<DelistAuthenticatorsRequest>(p => p == request)), Times.Once);
    }
    #endregion
}