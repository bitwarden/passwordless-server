namespace Passwordless.Common.Parsers;

public static class ApiKeyParser
{
    public static string GetAppId(string apiKey)
    {
        try
        {
            ReadOnlySpan<char> span = apiKey.AsSpan();
            var i = span.IndexOf(':');
            return span[..i].ToString();
        }
        catch (Exception)
        {
            throw new ArgumentException($"'{apiKey}' has a bad format.", nameof(apiKey));
        }
    }
}