using Passwordless.Service.Storage.Ef.ValueComparers;

namespace Passwordless.Service.Tests.Storage.Ef.ValueComparers;

public class EnumerableValueComparerTests
{
    [Fact]
    public void EnumerableValueComparer_ComparingSameArrays_ReturnsExpectedResult()
    {
        // Arrange
        var sut = new EnumerableValueComparer<string>();

        // Act
        var actual = sut.Equals(["a", "b"], ["a", "b"]);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EnumerableValueComparer_ComparingDifferentArrays_ReturnsExpectedResult()
    {
        // Arrange
        var sut = new EnumerableValueComparer<string>();

        // Act
        var actual = sut.Equals(["a", "b"], ["a", "c"]);

        // Assert
        Assert.False(actual);
    }
}