using System.Text.Json.Serialization;
using Fido2NetLib;

namespace Passwordless.Api.Models;

public class CredentialsDeleteDTO
{
    // TODO: Add Base64Url converter
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] CredentialId { get; set; } = Array.Empty<byte>();
}