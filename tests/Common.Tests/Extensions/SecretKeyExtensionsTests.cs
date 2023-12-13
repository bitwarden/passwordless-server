using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;

namespace Passwordless.Common.Tests.Extensions;

public class SecretKeyExtensionsTests
{
    [Fact]
    public void GetValue_GivenSecretKeyScope_ReturnsDescription()
    {
        // Arrange
        var scope = SecretKeyScopes.TokenRegister;

        // Act
        var result = scope.GetValue();

        // Assert
        Assert.Equal("token_register", result);
    }

    [Fact]
    public void AsSecretKeyScope_GivenDescription_ReturnsSecretKeyScope()
    {
        // Arrange
        var description = "token_register";

        // Act
        var result = description.AsSecretKeyScope();

        // Assert
        Assert.Equal(SecretKeyScopes.TokenRegister, result);
    }

    [Fact]
    public void AsSecretKeyScope_GivenInvalidDescription_ThrowsArgumentException()
    {
        // Arrange
        var description = "invalid";

        // Act
        var exception = Assert.Throws<ArgumentException>(() => description.AsSecretKeyScope());

        // Assert
        Assert.Equal("Invalid scope. (Parameter 'value')", exception.Message);
    }
}