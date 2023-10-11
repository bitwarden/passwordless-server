using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class Fido2ServiceEndpointsTests
{
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IEventLogger> _mockEventLogger;
    private readonly Mock<IEventLogContext> _mockEventLogContext;

    private readonly Fido2ServiceEndpoints _sut;

    public Fido2ServiceEndpointsTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTokenService = new Mock<ITokenService>();
        _mockEventLogger = new Mock<IEventLogger>();
        _mockEventLogContext = new Mock<IEventLogContext>();

        _sut = new Fido2ServiceEndpoints("test",
            NullLogger.Instance,
            _mockTenantStorage.Object,
            _mockTokenService.Object,
            _mockEventLogger.Object,
            _mockEventLogContext.Object);
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