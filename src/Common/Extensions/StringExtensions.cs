namespace Passwordless.Common.Extensions;

public static class StringExtensions
{
    public static string GetLast(this string input, int charactersToReturn) =>
        charactersToReturn >= input.Length
            ? input
            : input[^charactersToReturn..];
}