using Moq;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class ApplicationServiceTests
{
    private readonly Mock<ITenantStorage> _storageMock = new();
    private readonly Mock<IEventLogger> _eventLoggerMock = new();

    private readonly ApplicationService _sut;

    public ApplicationServiceTests()
    {
        _sut = new ApplicationService(_storageMock.Object, _eventLoggerMock.Object);
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
}