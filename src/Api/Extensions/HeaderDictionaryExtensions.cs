namespace Passwordless.Api.Extensions;

public static class HeaderDictionaryExtensions
{
    public static string? GetApiSecret(this IHeaderDictionary headerDictionary) => headerDictionary.GetHeaderValue("ApiSecret");

    public static string? GetPublicApiKey(this IHeaderDictionary headerDictionary) => headerDictionary.GetHeaderValue("ApiKey");

    private static string? GetHeaderValue(this IHeaderDictionary headerDictionary, string key)
    {
        headerDictionary.TryGetValue(key, out var value);
        return value.SingleOrDefault();
    }
}