using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;

namespace Passwordless.Common.Tests.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void GetDescription_GivenEnumValue_ReturnsExpectedDescription()
    {
        // Arrange
        var scope = PublicKeyScopes.Register;

        // Act
        var result = scope.GetDescription();

        // Assert
        Assert.Equal("register", result);
    }
}