namespace Passwordless.AdminConsole.Helpers;

internal static class StringExtensions
{
    public static string? NullIfEmpty(this string? value) =>
        !string.IsNullOrEmpty(value) ? value : null;
}