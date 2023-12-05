using Microsoft.Extensions.Logging.Abstractions;
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

    private readonly Fido2Service _sut;

    public Fido2ServiceTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTokenService = new Mock<ITokenService>();
        _mockEventLogger = new Mock<IEventLogger>();
        _mockFeatureContextProvider = new Mock<IFeatureContextProvider>();

        _sut = new Fido2Service(new ManualTenantProvider("test"),
            NullLogger.Instance,
            _mockTenantStorage.Object,
            _mockTokenService.Object,
            _mockEventLogger.Object,
            _mockFeatureContextProvider.Object);
    }

    [Fact]
    public async Task CreateRegisterToken_Works_WhenMaxUsersNotExceeded()
    {
        // arrange
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeTokenAsync(It.IsAny<RegisterToken>(), "register_", false))
            .ReturnsAsync("test_token");
        _mockFeatureContextProvider.Setup(x => x.UseContext()).ReturnsAsync(new FeaturesContext(false, 0, null, null));

        // act
        var actual = await _sut.CreateRegisterToken(new RegisterToken
        {
            UserId = "test",
            ExpiresAt = default,
            TokenId = Guid.NewGuid(),
            Type = "thing",
            Username = "test_user",
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
        _mockFeatureContextProvider.Setup(x => x.UseContext()).ReturnsAsync(new FeaturesContext(false, 0, null, 10000));
        _mockTenantStorage.Setup(x => x.GetUsersCount()).ReturnsAsync(10000);
        _mockTenantStorage.Setup(x => x.GetCredentialsByUserIdAsync(It.Is<string>(p => p == "test"))).ReturnsAsync(new List<StoredCredential>(0));

        // act
        var actual = await Assert.ThrowsAsync<ApiException>(async () =>
        {
            await _sut.CreateRegisterToken(new RegisterToken
            {
                UserId = "test",
                ExpiresAt = default,
                TokenId = Guid.NewGuid(),
                Type = "thing",
                Username = "test_user",
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
        _mockFeatureContextProvider.Setup(x => x.UseContext()).ReturnsAsync(new FeaturesContext(false, 0, null, 10000));
        _mockTenantStorage.Setup(x => x.GetUsersCount()).ReturnsAsync(10000);
        _mockTenantStorage.Setup(x => x.GetCredentialsByUserIdAsync(It.Is<string>(p => p == "test"))).ReturnsAsync(
            new List<StoredCredential>(1) { new() { UserHandle = "test"u8.ToArray() } });

        // act
        var actual = await _sut.CreateRegisterToken(new RegisterToken
        {
            UserId = "test",
            ExpiresAt = default,
            TokenId = Guid.NewGuid(),
            Type = "thing",
            Username = "test_user",
        });

        // assert
        Assert.Equal("test_token", actual);
    }
}