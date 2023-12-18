namespace Passwordless.Common.Services.Licensing.Serializers;

public interface ILicenseSerializer
{
    string Serialize<T>(T? obj);
    T? Deserialize<T>(string json);
}