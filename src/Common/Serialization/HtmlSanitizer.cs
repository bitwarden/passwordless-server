using Ganss.Xss;

namespace Passwordless.Common.Serialization;

public static class HtmlSanitizer
{
    private static readonly Ganss.Xss.HtmlSanitizer _htmlSanitizer = new(new HtmlSanitizerOptions
    {
        AllowedTags = new HashSet<string>(0)
    });

    public static string Sanitize(string input)
    {
        return _htmlSanitizer.Sanitize(input);
    }
}