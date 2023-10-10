using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Passwordless.Common.Helpers;

namespace Passwordless.Common.Utils;

public static class ApiKeyUtils
{
    /// <summary>
    /// Get the 'appId' or 'tenant' from an API key.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Validates private API keys
    /// </summary>
    /// <param name="hashedApiKey">The hashed private API key</param>
    /// <param name="apiKey">The private API key you want to validate</param>
    /// <returns></returns>
    public static bool Validate(string hashedApiKey, string apiKey)
    {
        try
        {
            var parts = hashedApiKey.Split(':');
            var salt = Convert.FromBase64String(parts[0]);
            var bytes = KeyDerivation.Pbkdf2(apiKey, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);
            return parts[1].Equals(Convert.ToBase64String(bytes));
        }
        catch
        {
            return false;
        }
    }

    public static string GeneratePublicApiKey(string accountName, string prefix)
    {
        var key = GuidHelper.CreateCryptographicallySecureRandomRFC4122Guid().ToString("N");

        return accountName + ":" + prefix + ":" + key;
    }

    public static (string originalApiKey, string hashedApiKey) GeneratePrivateApiKey(string accountName, string prefix)
    {
        var key = GuidHelper.CreateCryptographicallySecureRandomRFC4122Guid().ToString("N");
        var original = accountName + ":" + prefix + ":" + key;
        var hashed = HashPrivateApiKey(original);
        return (original, hashed);
    }

    /// <summary>
    /// Hashes a private API key or secret
    /// </summary>
    /// <param name="privateApiKey">Private API key or secret</param>
    /// <returns>Hashed private API key or secret</returns>
    public static string HashPrivateApiKey(string privateApiKey)
    {
        var salt = GenerateSalt(16);
        var bytes = KeyDerivation.Pbkdf2(privateApiKey, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(bytes)}";
    }

    private static byte[] GenerateSalt(int length)
    {
        var salt = new byte[length];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(salt);
        return salt;
    }
}