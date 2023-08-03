using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Service.Helpers;
using Passwordless.Service.Mail;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class SharedManagementServiceTests
{
    private readonly Mock<ITenantStorageFactory> _tenantStorageFactoryMock = new();
    private readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<ISystemClock> _systemClockMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<ILogger<SharedManagementService>> _loggerMock = new();

    private readonly SharedManagementService _sut;

    private readonly DateTime _now = new(2023, 08, 02, 15, 10, 00);

    public SharedManagementServiceTests()
    {
        _sut = new SharedManagementService(
            _tenantStorageFactoryMock.Object,
            _mailServiceMock.Object,
            _configurationMock.Object,
            _systemClockMock.Object,
            _loggerMock.Object);
        _systemClockMock.Setup(x => x.UtcNow)
            .Returns(new DateTimeOffset(_now));
    }

    #region DeleteApplicationAsync
    [Fact]
    public async Task DeleteApplicationAsync_Throws_ApiException_WhenAppNotFound()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(null as AccountMetaInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await _sut.DeleteApplicationAsync(appId, deletedBy));

        Assert.Equal("app_not_found", actual.ErrorCode);
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("App was not found.", actual.Message);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Never);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task DeleteApplicationAsync_Deletes_Immediately_WhenLessThan3DaysOld()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";

        var tenantStorageMock = new Mock<ITenantStorage>();
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-1)
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.DeleteApplicationAsync(appId, deletedBy);

        Assert.True(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow, actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Never);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Once);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task DeleteApplicationAsync_Deletes_Scheduled_WhenMoreThan3DaysOld()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(true);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-4)
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.DeleteApplicationAsync(appId, deletedBy);

        Assert.False(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow.AddDays(14), actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.Is<DateTime>(p => p == _systemClockMock.Object.UtcNow.AddDays(14))), Times.Once);
    }

    [Fact]
    public async Task DeleteApplicationAsync_Deletes_Immediately_WhenNoUsers()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(false);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-365)
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.DeleteApplicationAsync(appId, deletedBy);

        Assert.True(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow, actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Once);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Once);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task DeleteApplicationAsync_Deletes_Scheduled_WhenUsers()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(true);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-365)
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.DeleteApplicationAsync(appId, deletedBy);

        Assert.False(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow.AddDays(14), actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Once);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.Is<DateTime>(p => p == _systemClockMock.Object.UtcNow.AddDays(14))), Times.Once);
    }
    #endregion
}