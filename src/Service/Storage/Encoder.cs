using System.Text;

namespace Service.Storage;

public static class Encoder
{
    public static string ToBase64StringStorageSafe(byte[] value)
    {
        // remove illegal table storage characters: https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model?redirectedfrom=MSDN#characters-disallowed-in-key-fields
        return Convert.ToBase64String(value).Replace('/', '_');
    }
    public static string Base64(this string value)
    {
        return ToBase64StringStorageSafe(Encoding.UTF8.GetBytes(value));
    }

    public static string Base64(this byte[] value)
    {
        return ToBase64StringStorageSafe(value);
    }

    public static string Base64Decode(this string base64EncodedData)
    {
        base64EncodedData = base64EncodedData.Replace("_", "/");
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}