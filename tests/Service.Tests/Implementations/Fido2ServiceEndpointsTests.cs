using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef.Tenant;

namespace Passwordless.Service.Tests.Implementations;

public class Fido2ServiceEndpointsTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private readonly Mock<ITokenService> _mockTokenService;

    private readonly Fido2ServiceEndpoints _sut;

    public Fido2ServiceEndpointsTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTokenService = new Mock<ITokenService>();

        _sut = new Fido2ServiceEndpoints("test",
            NullLogger.Instance,
            _mockConfiguration.Object,
            _mockTenantStorage.Object,
            _mockTokenService.Object);
    }

    [Fact]
    public async Task CreateToken_Works()
    {
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeToken(It.IsAny<RegisterToken>(), "register_", false))
            .Returns("test_token");

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
}