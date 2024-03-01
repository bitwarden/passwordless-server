using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Utils;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class SharedManagementServiceTests
{
    private readonly Mock<ITenantStorage> _tenantStorageMock = new();
    private readonly Mock<ITenantStorageFactory> _tenantStorageFactoryMock = new();
    private readonly Mock<IGlobalStorage> _storageMock = new();
    private readonly Mock<ISystemClock> _systemClockMock = new();
    private readonly Mock<ILogger<SharedManagementService>> _loggerMock = new();
    private readonly Mock<IEventLogger> _eventLogger = new();

    private readonly SharedManagementService _sut;

    private readonly DateTime _now = new(2023, 08, 02, 15, 10, 00);

    public SharedManagementServiceTests()
    {
        _sut = new SharedManagementService(
            _tenantStorageMock.Object,
            _tenantStorageFactoryMock.Object,
            _storageMock.Object,
            _systemClockMock.Object,
            _loggerMock.Object,
            _eventLogger.Object);
        _systemClockMock.Setup(x => x.UtcNow)
            .Returns(new DateTimeOffset(_now));
    }

    #region MarkDeleteApplicationAsync

    [Fact]
    public async Task MarkDeleteApplicationAsync_Throws_ApiException_WhenAppNotFound()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";
        const string baseUrl = "http://localhost:7001";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(null as AccountMetaInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await _sut.MarkDeleteApplicationAsync(appId, deletedBy, baseUrl));

        Assert.Equal("app_not_found", actual.ErrorCode);
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("App was not found.", actual.Message);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Never);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task MarkDeleteApplicationAsync_Deletes_Immediately_WhenLessThan3DaysOld()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";
        const string baseUrl = "http://localhost:7001";

        var tenantStorageMock = new Mock<ITenantStorage>();
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-1),
            Tenant = appId,
            AdminEmails = new[] { deletedBy }
        };

        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.MarkDeleteApplicationAsync(appId, deletedBy, baseUrl);

        Assert.True(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow, actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Never);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Once);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task MarkDeleteApplicationAsync_Deletes_Scheduled_WhenMoreThan3DaysOld()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";
        const string baseUrl = "http://localhost:7001";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(true);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-4),
            Tenant = appId,
            AdminEmails = new[] { deletedBy }
        };

        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.MarkDeleteApplicationAsync(appId, deletedBy, baseUrl);

        Assert.False(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow.AddMonths(1), actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
        tenantStorageMock.Verify(
            x => x.SetAppDeletionDate(It.Is<DateTime>(p => p == _systemClockMock.Object.UtcNow.AddMonths(1))),
            Times.Once);
    }

    [Fact]
    public async Task MarkDeleteApplicationAsync_Deletes_Immediately_WhenNoUsers()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";
        const string baseUrl = "http://localhost:7001";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(false);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-365),
            Tenant = appId,
            AdminEmails = new[] { deletedBy }
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.MarkDeleteApplicationAsync(appId, deletedBy, baseUrl);

        Assert.True(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow, actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Once);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Once);
        tenantStorageMock.Verify(x => x.SetAppDeletionDate(It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task MarkDeleteApplicationAsync_Deletes_Scheduled_WhenUsers()
    {
        const string appId = "mockedAppId";
        const string deletedBy = "admin@example.com";
        const string baseUrl = "http://localhost:7001";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(true);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            CreatedAt = _now.AddDays(-365),
            Tenant = appId,
            AdminEmails = new[] { deletedBy }
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.MarkDeleteApplicationAsync(appId, deletedBy, baseUrl);

        Assert.False(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow.AddMonths(1), actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.HasUsersAsync(), Times.Once);
        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
        tenantStorageMock.Verify(
            x => x.SetAppDeletionDate(It.Is<DateTime>(p => p == _systemClockMock.Object.UtcNow.AddMonths(1))),
            Times.Once);
    }

    #endregion

    #region DeleteApplicationAsync

    [Fact]
    public async Task DeleteApplicationAsync_Throws_ApiException_WhenAppNotFound()
    {
        const string appId = "mockedAppId";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(null as AccountMetaInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await _sut.DeleteApplicationAsync(appId));

        Assert.Equal("app_not_found", actual.ErrorCode);
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("App was not found.", actual.Message);

        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
    }

    [Fact]
    public async Task DeleteApplicationAsync_Deletes_Immediately_WhenDeletedAtIsSetInThePast()
    {
        const string appId = "mockedAppId";

        var tenantStorageMock = new Mock<ITenantStorage>();
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            DeleteAt = _now.AddDays(-1),
            Tenant = appId,
            AdminEmails = new[] { "admin@email.com" }
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await _sut.DeleteApplicationAsync(appId);

        Assert.True(actual.IsDeleted);
        Assert.Equal(_systemClockMock.Object.UtcNow, actual.DeleteAt.Value);

        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Once);
    }

    [Fact]
    public async Task DeleteApplicationAsync_Throws_ApiException_WhenDeletedAtIsSetInTheFuture()
    {
        const string appId = "mockedAppId";

        var tenantStorageMock = new Mock<ITenantStorage>();
        tenantStorageMock.Setup(x => x.HasUsersAsync()).ReturnsAsync(true);
        var accountInformation = new AccountMetaInformation
        {
            AcountName = appId,
            DeleteAt = _now.AddDays(1),
            Tenant = appId,
            AdminEmails = new[] { "admin@email.com" }
        };
        tenantStorageMock.Setup(x => x.GetAccountInformation())
            .ReturnsAsync(accountInformation);
        _tenantStorageFactoryMock
            .Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(tenantStorageMock.Object);

        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
            await _sut.DeleteApplicationAsync(appId));

        Assert.Equal("app_not_pending_deletion", actual.ErrorCode);
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("App was not scheduled for deletion.", actual.Message);

        tenantStorageMock.Verify(x => x.DeleteAccount(), Times.Never);
    }

    #endregion

    #region ListApplicationsPendingDeletionAsync

    [Fact]
    public async Task ListApplicationsPendingDeletionAsync_Returns_ExpectedResult()
    {
        var expected = new List<string> { "app1", "app2" };
        _storageMock.Setup(x => x.GetApplicationsPendingDeletionAsync()).ReturnsAsync(expected);

        var actual = await _sut.GetApplicationsPendingDeletionAsync();

        Assert.Equal(expected, actual);

        _tenantStorageFactoryMock.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
        _storageMock.Verify(x => x.GetApplicationsPendingDeletionAsync(), Times.Once);
    }

    #endregion

    #region SetFeaturesAsync

    [Fact]
    public async Task SetFeaturesAsync_Throws_ApiException_WhenPayloadIsNull()
    {
        const string appId = "myappid";
        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId))).Returns(storageMock.Object);

        var actual = await Assert.ThrowsAsync<ApiException>(async () => await _sut.SetFeaturesAsync(appId, null));

        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("No 'body' or 'parameters' were passed.", actual.Message);

        _tenantStorageFactoryMock.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
        storageMock.Verify(x => x.SetFeaturesAsync(It.IsAny<ManageFeaturesRequest>()), Times.Never);
    }

    [Fact]
    public async Task SetFeaturesAsync_Throws_ApiException_WhenAppIdIsNull()
    {
        var payload = new ManageFeaturesRequest();

        var actual = await Assert.ThrowsAsync<ApiException>(async () => await _sut.SetFeaturesAsync(null, payload));

        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("'appId' is required.", actual.Message);

        _tenantStorageFactoryMock.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetFeaturesAsync_Throws_ApiException_WhenTenantsIsEmpty()
    {
        var payload = new ManageFeaturesRequest();

        var actual =
            await Assert.ThrowsAsync<ApiException>(async () => await _sut.SetFeaturesAsync(string.Empty, payload));

        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("'appId' is required.", actual.Message);

        _tenantStorageFactoryMock.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetFeaturesAsync_Returns_ExpectedResult()
    {
        const string appId = "myappid";
        var payload = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = true,
            EventLoggingRetentionPeriod = 7,
            MaxUsers = 69L
        };
        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId)))
            .Returns(storageMock.Object);

        await _sut.SetFeaturesAsync(appId, payload);

        _tenantStorageFactoryMock.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
        storageMock.Verify(x => x.SetFeaturesAsync(
            It.Is<ManageFeaturesRequest>(p => p == payload)), Times.Once);
    }

    #endregion

    #region ListApiKeysAsync

    [Fact]
    public async Task ListApiKeysAsync_Returns_ExpectedResult()
    {
        // Arrange
        const string appId = "test";
        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId))).Returns(storageMock.Object);
        storageMock.Setup(x => x.GetAllApiKeys()).ReturnsAsync(new List<ApiKeyDesc>
        {
            new()
            {
                Tenant = "test",
                ApiKey = "test:public:2e728aa5986f4ba8b073a5b28a939795",
                Id = "9795",
                Scopes = new[] { "register", "login" },
                IsLocked = false,
                LastLockedAt = new DateTime(2023, 10, 1),
                LastUnlockedAt = new DateTime(2023, 10, 2)
            },
            new()
            {
                Tenant = "test",
                ApiKey = "Wx9XPDW1cp1Jb0LxElrh+g==:/7E93JhN30boFyNyVbCW/g==",
                Id = "6d02",
                Scopes = new[] { "token_register", "token_verify" },
                IsLocked = true,
                LastLockedAt = new DateTime(2023, 11, 1),
                LastUnlockedAt = null
            }
        });

        // act
        var actual = await _sut.ListApiKeysAsync(appId);

        // assert
        Assert.Equal(2, actual.Count);

        var actualPublicKey = actual.First();
        Assert.Equal("9795", actualPublicKey.Id);
        Assert.Equal(ApiKeyTypes.Public, actualPublicKey.Type);
        Assert.Equal("test:public:2e728aa5986f4ba8b073a5b28a939795", actualPublicKey.ApiKey);
        Assert.False(actualPublicKey.IsLocked);
        Assert.Equal(new DateTime(2023, 10, 1), actualPublicKey.LastLockedAt);
        Assert.Equal(2, actualPublicKey.Scopes.Count);
        Assert.Contains(PublicKeyScopes.Register.GetValue(), actualPublicKey.Scopes);
        Assert.Contains(PublicKeyScopes.Login.GetValue(), actualPublicKey.Scopes);

        var actualSecretKey = actual.Last();
        Assert.Equal("6d02", actualSecretKey.Id);
        Assert.Equal(ApiKeyTypes.Secret, actualSecretKey.Type);
        Assert.Equal("test:secret:****************************6d02", actualSecretKey.ApiKey);
        Assert.True(actualSecretKey.IsLocked);
        Assert.Equal(new DateTime(2023, 11, 1), actualSecretKey.LastLockedAt);
        Assert.Equal(2, actualSecretKey.Scopes.Count);
        Assert.Contains(SecretKeyScopes.TokenRegister.GetValue(), actualSecretKey.Scopes);
        Assert.Contains(SecretKeyScopes.TokenVerify.GetValue(), actualSecretKey.Scopes);
    }

    #endregion

    #region CreatePublicKeyAsync

    [Fact]
    public async Task CreatePublicKeyAsync_CreatesPublicKey()
    {
        // arrange 
        const string appId = "appId";
        var scopes = new HashSet<PublicKeyScopes>() { PublicKeyScopes.Register, PublicKeyScopes.Login };
        var createPublicKey = new CreatePublicKeyRequest(scopes);

        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId))).Returns(storageMock.Object);

        // act
        var actual = await _sut.CreateApiKeyAsync(appId, createPublicKey);

        // assert
        actual.ApiKey[..13]
            .Should()
            .Be("appId:public:");

        storageMock.Verify(x => x.StoreApiKeyAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.Is<string[]>(p => p == scopes.Select(s => s.GetValue()).ToArray())), Times.Once);
    }

    [Fact]
    public async Task CreatePublicKeyAsync_Throws_ApiException()
    {
        // arrange 
        const string appId = "appId";
        var scopes = new HashSet<PublicKeyScopes>();
        var createPublicKey = new CreatePublicKeyRequest(scopes);

        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId))).Returns(storageMock.Object);

        // act
        var actual = () => _sut.CreateApiKeyAsync(appId, createPublicKey);

        // assert
        await actual
            .Should()
            .ThrowAsync<ApiException>("create_api_key_scopes_required", "Please select at least one scope.", 400);
    }

    #endregion

    #region CreateSecretKeyAsync

    [Fact]
    public async Task CreateSecretKeyAsync_CreatesSecretKey()
    {
        // arrange 
        const string appId = "appId";
        var scopes = new HashSet<SecretKeyScopes>() { SecretKeyScopes.TokenRegister, SecretKeyScopes.TokenVerify };
        var createSecretKey = new CreateSecretKeyRequest(scopes);

        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId))).Returns(storageMock.Object);

        // act
        var actual = await _sut.CreateApiKeyAsync(appId, createSecretKey);

        // assert
        actual.ApiKey[..13]
            .Should()
            .Be("appId:secret:");
    }

    [Fact]
    public async Task CreateSecretKeyAsync_Throws_ApiException()
    {
        // arrange 
        const string appId = "appId";
        var scopes = new HashSet<SecretKeyScopes>();
        var createPublicKey = new CreateSecretKeyRequest(scopes);

        var storageMock = new Mock<ITenantStorage>();
        _tenantStorageFactoryMock.Setup(x => x.Create(It.Is<string>(p => p == appId))).Returns(storageMock.Object);

        // act
        var actual = () => _sut.CreateApiKeyAsync(appId, createPublicKey);

        // assert
        await actual
            .Should()
            .ThrowAsync<ApiException>("create_api_key_scopes_required", "Please select at least one scope.", 400);
    }

    #endregion
}