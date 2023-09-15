namespace Passwordless.Common.Extensions;

public static class StringExtensions
{
    public static string GetLast(this string input, int characterCountToReturn) =>
        string.IsNullOrWhiteSpace(input) || characterCountToReturn >= input.Length
            ? input
            : input[^characterCountToReturn..];
}