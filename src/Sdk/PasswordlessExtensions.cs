namespace Passwordless.Net;

public static class PasswordlessExtensions
{
    public static string ToBase64Url(this byte[] bytes)
    {
        return Base64Url.Encode(bytes);
    }
}