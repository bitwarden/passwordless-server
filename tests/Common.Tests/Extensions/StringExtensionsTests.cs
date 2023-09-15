using Passwordless.Common.Extensions;

namespace Passwordless.Common.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void GetLast_GivenStringAndAnyNumberOfCharactersToReturn_WhenStringIsNull_ThenResultShouldBeNull()
    {
        var numberOfCharacters = new Random().Next();
        var sut = (string)null;

        var actual = sut.GetLast(numberOfCharacters);

        Assert.Null(actual);
    }

    [Fact]
    public void GetLast_GivenStringAndAnyNumberOfCharactersToReturn_WhenStringIsEmpty_ThenShouldBeAnEmptyString()
    {
        var numberOfCharacters = new Random().Next();
        var sut = string.Empty;

        var actual = sut.GetLast(numberOfCharacters);

        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetLast_GivenStringOfTwoCharacters_WhenLastTwoCharactersAreRequested_ThenShouldReturnFullString()
    {
        var numberOfCharacters = 2;
        var sut = "ab";

        var actual = sut.GetLast(numberOfCharacters);

        Assert.Equal(sut, actual);
    }

    [Fact]
    public void GetLast_GivenStringOfTwoCharacters_WhenLastThreeCharactersAreRequested_ThenShouldReturnFullString()
    {
        var numberOfCharacters = 2;
        var sut = "ab";

        var actual = sut.GetLast(numberOfCharacters);

        Assert.Equal(sut, actual);
    }
}