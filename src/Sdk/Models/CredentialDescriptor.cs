using System.Text.Json.Serialization;

using static Passwordless.Net.PasswordlessClient;

namespace Passwordless.Net;

public class CredentialDescriptor
{
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] Id { get; set; }
}