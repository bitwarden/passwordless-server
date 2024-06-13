using Fido2NetLib;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class Fido2ServiceTests
{
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IEventLogger> _mockEventLogger;
    private readonly Mock<IFeatureContextProvider> _mockFeatureContextProvider;
    private readonly Mock<IMetadataService> _mockMetadataService;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly Mock<IAuthenticationConfigurationService> _mockAuthenticationConfigurationService;

    private readonly Fido2Service _sut;

    public Fido2ServiceTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTokenService = new Mock<ITokenService>();
        _mockEventLogger = new Mock<IEventLogger>();
        _mockFeatureContextProvider = new Mock<IFeatureContextProvider>();
        _mockMetadataService = new Mock<IMetadataService>();
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _mockAuthenticationConfigurationService = new Mock<IAuthenticationConfigurationService>();

        _sut = new Fido2Service(new ManualTenantProvider("test"),
            NullLogger.Instance,
            _mockTenantStorage.Object,
            _mockTokenService.Object,
            _mockEventLogger.Object,
            _mockFeatureContextProvider.Object,
            _mockMetadataService.Object,
            _fakeTimeProvider,
            _mockAuthenticationConfigurationService.Object);
    }

    [Fact]
    public async Task CreateRegisterToken_Works_WhenMaxUsersNotExceeded()
    {
        // arrange
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeTokenAsync(It.IsAny<RegisterToken>(), "register_", false))
            .ReturnsAsync("test_token");
        _mockFeatureContextProvider.Setup(x => x.UseContext()).ReturnsAsync(new FeaturesContext(false, 0, null, null, false, true, true));

        // act
        var actual = await _sut.CreateRegisterTokenAsync(new RegisterToken
        {
            UserId = "test",
            ExpiresAt = default,
            Username = "test_user"
        });

        // assert
        Assert.Equal("test_token", actual);
    }

    [Fact]
    public async Task CreateRegisterToken_Throws_ApiException_WhenMaxUsersExceededForNewUser()
    {
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeTokenAsync(It.IsAny<RegisterToken>(), "register_", false))
            .ReturnsAsync("test_token");
        _mockFeatureContextProvider.Setup(x => x.UseContext()).ReturnsAsync(new FeaturesContext(false, 0, null, 10000, false, true, true));
        _mockTenantStorage.Setup(x => x.GetUsersCount()).ReturnsAsync(10000);
        _mockTenantStorage.Setup(x => x.GetCredentialsByUserIdAsync(It.Is<string>(p => p == "test"))).ReturnsAsync(new List<StoredCredential>(0));

        // act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
        {
            await _sut.CreateRegisterTokenAsync(new RegisterToken
            {
                UserId = "test",
                ExpiresAt = default,
                Username = "test_user"
            });
        });

        // assert
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("max_users_reached", actual.ErrorCode);
        Assert.Equal("Maximum number of users reached", actual.Message);
    }

    [Fact]
    public async Task CreateRegisterToken_Works_WhenMaxUsersExceededForExistingUser()
    {
        // arrange
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeTokenAsync(It.IsAny<RegisterToken>(), "register_", false))
            .ReturnsAsync("test_token");
        _mockFeatureContextProvider.Setup(x => x.UseContext()).ReturnsAsync(new FeaturesContext(false, 0, null, 10000, false, true, true));
        _mockTenantStorage.Setup(x => x.GetUsersCount()).ReturnsAsync(10000);
        _mockTenantStorage.Setup(x => x.GetCredentialsByUserIdAsync(It.Is<string>(p => p == "test"))).ReturnsAsync(
            new List<StoredCredential>(1) { new() { UserHandle = "test"u8.ToArray(), Descriptor = null!, Origin = null!, AttestationFmt = null!, CreatedAt = DateTime.UtcNow, PublicKey = null!, SignatureCounter = 123, RPID = null! } });

        // act
        var actual = await _sut.CreateRegisterTokenAsync(new RegisterToken
        {
            UserId = "test",
            ExpiresAt = default,
            Username = "test_user"
        });

        // assert
        Assert.Equal("test_token", actual);
    }
}