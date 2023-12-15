using Passwordless.Service.Models;

namespace Passwordless.Service.Tests.Models;

public class ApiDescTests
{
    [Fact]
    public void MaskedApiKey_Returns_ExpectedResultForPublicKey()
    {
        // Arrange
        var sut = new ApiKeyDesc
        {
            Tenant = "test",
            Id = "9795",
            ApiKey = "test:public:2e728aa5986f4ba8b073a5b28a939795",
        };

        // Act
        var result = sut.MaskedApiKey;

        // Assert
        Assert.Equal("test:public:****************************9795", result);
    }

    [Fact]
    public void MaskedApiKey_Returns_ExpectedResultForSecretKey()
    {
        // Arrange
        var sut = new ApiKeyDesc
        {
            Tenant = "test",
            Id = "6d02",
            ApiKey = "8572398573289572389573289572389572389572389"
        };

        // Act
        var result = sut.MaskedApiKey;

        // Assert
        Assert.Equal("test:secret:****************************6d02", result);
    }
}