using Moq;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.MDS;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class ApplicationServiceTests
{
    private readonly Mock<ITenantStorage> _storageMock = new();
    private readonly Mock<IEventLogger> _eventLoggerMock = new();
    private readonly Mock<IMetaDataService> _metaDataServiceMock = new();

    private readonly ApplicationService _sut;

    public ApplicationServiceTests()
    {
        _sut = new ApplicationService(_storageMock.Object, _eventLoggerMock.Object, _metaDataServiceMock.Object);
    }

    #region SetFeaturesAsync
    [Fact]
    public async Task SetFeaturesAsync_Throws_ApiException_WhenPayloadIsNull()
    {
        var actual = await Assert.ThrowsAsync<ApiException>(async () => await _sut.SetFeaturesAsync(null));

        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("No 'body' or 'parameters' were passed.", actual.Message);

        _storageMock.Verify(x => x.SetFeaturesAsync(It.IsAny<ManageFeaturesRequest>()), Times.Never);
    }

    [Fact]
    public async Task SetFeaturesAsync_Returns_ExpectedResult()
    {
        var payload = new SetFeaturesRequest
        {
            EventLoggingRetentionPeriod = 7
        };

        await _sut.SetFeaturesAsync(payload);

        _storageMock.Verify(x => x.SetFeaturesAsync(
            It.Is<SetFeaturesRequest>(p => p == payload)), Times.Once);
    }
    #endregion

    #region AddAuthenticatorsAsync
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddAuthenticatorsAsync_Throws_ApiException_WhenAuthenticatorDoesNotExist(bool isAllowed)
    {
        // Arrange
        var payload = new AddAuthenticatorsRequest(new List<Guid> { Guid.NewGuid() }, isAllowed);

        _metaDataServiceMock.Setup(x =>
                x.ExistsAsync(
                    It.Is<IReadOnlyCollection<Guid>>(p => p == payload.AaGuids)))
            .ReturnsAsync(false);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(async () => await _sut.AddAuthenticatorsAsync(payload));

        // Assert
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("One or more authenticators do not exist in the FIDO2 MDS.", actual.Message);

        _metaDataServiceMock.Verify(x =>
                x.ExistsAsync(
                    It.Is<IReadOnlyCollection<Guid>>(p => p == payload.AaGuids)),
            Times.Once);

        _storageMock.Verify(x =>
            x.AddAuthenticatorsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task AddAuthenticatorsAsync_Returns_ExpectedResult()
    {
        // Arrange
        var payload = new AddAuthenticatorsRequest(new List<Guid> { Guid.NewGuid() }, true);

        _metaDataServiceMock.Setup(x =>
                x.ExistsAsync(
                    It.Is<IReadOnlyCollection<Guid>>(p => p == payload.AaGuids)))
            .ReturnsAsync(true);

        // Act
        await _sut.AddAuthenticatorsAsync(payload);

        // Assert
        _metaDataServiceMock.Verify(x =>
                x.ExistsAsync(
                    It.Is<IReadOnlyCollection<Guid>>(p => p == payload.AaGuids)),
            Times.Once);

        _storageMock.Verify(x =>
            x.AddAuthenticatorsAsync(
                It.Is<IReadOnlyCollection<Guid>>(p => p == payload.AaGuids),
                It.Is<bool>(p => p == payload.IsAllowed)),
            Times.Once);
    }
    #endregion
}