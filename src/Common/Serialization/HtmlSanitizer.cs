namespace Passwordless.Common.Serialization;

public static class HtmlSanitizer
{
    private static readonly Ganss.Xss.HtmlSanitizer _htmlSanitizer = new();

    public static string Sanitize(string input)
    {
        return _htmlSanitizer.Sanitize(input);
    }
}