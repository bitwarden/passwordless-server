using System.Text.Json;

namespace Passwordless.Common.Services.Licensing.Serializers;

public class LicenseSerializer : ILicenseSerializer
{
    private readonly JsonSerializerOptions _options = new();

    public string Serialize<T>(T? obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}