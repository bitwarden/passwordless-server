using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;

namespace Passwordless.Common.Tests.Extensions;

public class PublicKeyExtensionsTests
{
    [Fact]
    public void GetValue_GivenPublicKeyScope_ReturnsDescription()
    {
        // Arrange
        var scope = PublicKeyScopes.Register;

        // Act
        var result = scope.GetValue();

        // Assert
        Assert.Equal("register", result);
    }

    [Fact]
    public void AsPublicKeyScope_GivenDescription_ReturnsPublicKeyScope()
    {
        // Arrange
        var description = "register";

        // Act
        var result = description.AsPublicKeyScope();

        // Assert
        Assert.Equal(PublicKeyScopes.Register, result);
    }

    [Fact]
    public void AsPublicKeyScope_GivenInvalidDescription_ThrowsArgumentException()
    {
        // Arrange
        var description = "invalid";

        // Act
        var exception = Assert.Throws<ArgumentException>(() => description.AsPublicKeyScope());

        // Assert
        Assert.Equal("Invalid scope. (Parameter 'value')", exception.Message);
    }
}