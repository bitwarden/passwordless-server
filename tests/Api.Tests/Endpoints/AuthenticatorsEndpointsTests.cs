using System.Collections.Immutable;
using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Passwordless.Api.Endpoints;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;

namespace Passwordless.Api.Tests.Endpoints;

public class AuthenticatorsEndpointsTests
{
    private readonly Fixture _fixture = new();

    #region ListConfiguredAuthenticatorsAsync
    [Fact]
    public async Task ListConfiguredAuthenticatorsAsync_Throws_Forbidden_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<ConfiguredAuthenticatorRequest>();

        var serviceMock = new Mock<IApplicationService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await AuthenticatorsEndpoints.ListConfiguredAuthenticatorsAsync(
                request,
                serviceMock.Object,
                featureContextProviderMock.Object));

        // Assert
        Assert.Equal("attestation_not_supported_on_plan", actual.ErrorCode);
        Assert.Equal("Attestation is not supported on your plan.", actual.Message);
        Assert.Equal(403, actual.StatusCode);
        serviceMock.Verify(x => x.ListConfiguredAuthenticatorsAsync(It.IsAny<ConfiguredAuthenticatorRequest>()), Times.Never);
    }

    [Fact]
    public async Task ListConfiguredAuthenticatorsAsync_Returns_Ok_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<ConfiguredAuthenticatorRequest>();

        var serviceMock = new Mock<IApplicationService>();
        var expectedResponse = _fixture.CreateMany<ConfiguredAuthenticatorResponse>().ToImmutableList();
        serviceMock
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
        var actual = await AuthenticatorsEndpoints.ListConfiguredAuthenticatorsAsync(
                request,
                serviceMock.Object,
                featureContextProviderMock.Object);

        // Assert
        Assert.Equal(typeof(Ok<IEnumerable<ConfiguredAuthenticatorResponse>>), actual.GetType());
        serviceMock.Verify(x => x.ListConfiguredAuthenticatorsAsync(It.Is<ConfiguredAuthenticatorRequest>(p => p == request)), Times.Once);
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

        var serviceMock = new Mock<IApplicationService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await AuthenticatorsEndpoints.WhitelistAuthenticatorsAsync(
                request,
                serviceMock.Object,
                featureContextProviderMock.Object));

        // Assert
        Assert.Equal("attestation_not_supported_on_plan", actual.ErrorCode);
        Assert.Equal("Attestation is not supported on your plan.", actual.Message);
        Assert.Equal(403, actual.StatusCode);
        serviceMock.Verify(x => x.WhitelistAuthenticatorsAsync(It.IsAny<WhitelistAuthenticatorsRequest>()), Times.Never);
    }

    [Fact]
    public async Task WhitelistAuthenticatorsAsync_Returns_NoContent()
    {
        // Arrange
        var request = _fixture.Create<WhitelistAuthenticatorsRequest>();

        var serviceMock = new Mock<IApplicationService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        featureContextProviderMock
            .Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await AuthenticatorsEndpoints.WhitelistAuthenticatorsAsync(
            request,
            serviceMock.Object,
            featureContextProviderMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        serviceMock.Verify(x => x.WhitelistAuthenticatorsAsync(It.Is<WhitelistAuthenticatorsRequest>(p => p == request)), Times.Once);
    }
    #endregion

    #region DelistAuthenticatorsAsync
    [Fact]
    public async Task DelistAuthenticatorsAsync_Throws_Forbidden_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var request = _fixture.Create<DelistAuthenticatorsRequest>();

        var serviceMock = new Mock<IApplicationService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        featureContextProviderMock.Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await AuthenticatorsEndpoints.DelistAuthenticatorsAsync(
                request,
                serviceMock.Object,
                featureContextProviderMock.Object));

        // Assert
        Assert.Equal("attestation_not_supported_on_plan", actual.ErrorCode);
        Assert.Equal("Attestation is not supported on your plan.", actual.Message);
        Assert.Equal(403, actual.StatusCode);
        serviceMock.Verify(x => x.DelistAuthenticatorsAsync(It.IsAny<DelistAuthenticatorsRequest>()), Times.Never);
    }

    [Fact]
    public async Task DelistAuthenticatorsAsync_Returns_NoContent()
    {
        // Arrange
        var request = _fixture.Create<DelistAuthenticatorsRequest>();

        var serviceMock = new Mock<IApplicationService>();

        var featureContextProviderMock = new Mock<IFeatureContextProvider>();
        var expectedFeaturesContext = _fixture
            .Build<FeaturesContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        featureContextProviderMock
            .Setup(x => x.UseContext())
            .ReturnsAsync(expectedFeaturesContext);

        // Act
        var actual = await AuthenticatorsEndpoints.DelistAuthenticatorsAsync(
            request,
            serviceMock.Object,
            featureContextProviderMock.Object);

        // Assert
        Assert.Equal(typeof(NoContent), actual.GetType());
        serviceMock.Verify(x => x.DelistAuthenticatorsAsync(It.Is<DelistAuthenticatorsRequest>(p => p == request)), Times.Once);
    }
    #endregion
}