using Fido2NetLib;
using System.Text.Json.Serialization;

public class CredentialsDeleteDTO
{
    public string ApiSecret { get; set; } = "";

    // TODO: Add Base64Url converter
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] CredentialId { get; set; } = Array.Empty<byte>();
}
