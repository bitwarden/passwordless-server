namespace Passwordless.Common.Extensions;

public static class StringExtensions
{
    public static string GetLast(this string input, int charactersToReturn) =>
        input == null || input.Length <= charactersToReturn
            ? input
            : input[^charactersToReturn..];
}