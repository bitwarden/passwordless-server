using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class Fido2ServiceTests
{
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IFeatureContextProvider> _mockFeatureContextProvider;

    private readonly Fido2Service _sut;

    public Fido2ServiceTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTokenService = new Mock<ITokenService>();
        _mockFeatureContextProvider = new Mock<IFeatureContextProvider>();

        _sut = new Fido2Service("test",
            NullLogger.Instance,
            _mockTenantStorage.Object,
            _mockTokenService.Object,
            _mockFeatureContextProvider.Object);
    }

    #region CreateToken
    [Fact]
    public async Task CreateToken_Works()
    {
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeToken(It.IsAny<RegisterToken>(), "register_", false))
            .Returns("test_token");
        _mockFeatureContextProvider.Setup(x => x.UseContext())
            .ReturnsAsync(new FeaturesContext(false, 0, null, null));

        var token = await _sut.CreateToken(new RegisterToken
        {
            UserId = "test",
            ExpiresAt = default,
            TokenId = Guid.NewGuid(),
            Type = "thing",
            Username = "test_user",
        });

        Assert.Equal("test_token", token);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task CreateToken_Throws_ApiException_WhenMaxUsersExceededForNewUser(int userCount)
    {
        // Arrange
        _mockTokenService
            .Setup(t => t.EncodeToken(It.IsAny<RegisterToken>(), "register_", false))
            .Returns("test_token");
        _mockFeatureContextProvider.Setup(x => x.UseContext())
            .ReturnsAsync(new FeaturesContext(true, 5, null, 1));
        _mockTenantStorage.Setup(x => x.IsUserExistsAsync(It.Is<string>(p => p == "test")))
            .ReturnsAsync(false);
        _mockTenantStorage.Setup(x => x.GetUsersCount())
            .ReturnsAsync(userCount);

        // Act
        var actual = await Assert.ThrowsAsync<ApiException>(() => _sut.CreateToken(new RegisterToken
        {
            UserId = "test",
            ExpiresAt = default,
            TokenId = Guid.NewGuid(),
            Type = "thing",
            Username = "test_user",
        }));

        // Assert
        Assert.Equal("max_users_reached", actual.ErrorCode);
        Assert.Equal("Maximum number of users reached", actual.Message);
        Assert.Equal(400, actual.StatusCode);
    }
    #endregion
}