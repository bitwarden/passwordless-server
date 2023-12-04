using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class Fido2ServiceTests
{
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IEventLogger> _mockEventLogger;

    private readonly Fido2Service _sut;

    public Fido2ServiceTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTokenService = new Mock<ITokenService>();
        _mockEventLogger = new Mock<IEventLogger>();

        _sut = new Fido2Service(new ManualTenantProvider("test"),
            NullLogger.Instance,
            _mockTenantStorage.Object,
            _mockTokenService.Object,
            _mockEventLogger.Object);
    }

    [Fact]
    public async Task CreateToken_Works()
    {
        _mockTokenService
            // TODO: Assert more details about the register token passed in
            .Setup(t => t.EncodeTokenAsync(It.IsAny<RegisterToken>(), "register_", false))
            .ReturnsAsync("test_token");

        var token = await _sut.CreateRegisterToken(new RegisterToken
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