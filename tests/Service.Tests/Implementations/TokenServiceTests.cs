using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Passwordless.Service.Storage.Ef.Tenant;

namespace Passwordless.Service.Tests.Implementations;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ITenantStorage> _mockTenantStorage;

    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockTenantStorage = new Mock<ITenantStorage>();

        _sut = new TokenService(NullLogger.Instance,
            _mockConfiguration.Object,
            _mockTenantStorage.Object);
    }

    [Fact]
    public void Test()
    {
        Assert.NotNull(_sut);
    }
}