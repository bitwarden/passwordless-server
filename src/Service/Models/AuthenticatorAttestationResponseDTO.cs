using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Passwordless.Service.Models;

/// <summary>
/// DTO representing an AuthenticatorAttestationResponse from the WebAuthn API.
/// This contains the data returned by the authenticator during credential registration.
/// </summary>
public class AuthenticatorAttestationResponseDTO
{
    /// <summary>
    /// The credential ID as a base64url-encoded string.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The raw credential ID as a byte array.
    /// </summary>
    [JsonConverter(typeof(Base64UrlConverter))]
    [JsonPropertyName("rawId")]
    public required byte[] RawId { get; init; }

    /// <summary>
    /// The type of credential (typically "public-key").
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public PublicKeyCredentialType Type { get; init; }

    /// <summary>
    /// The authenticator's response data.
    /// </summary>
    [Required]
    [JsonPropertyName("response")]
    public AuthenticatorAttestationResponseDataDTO Response { get; init; }

    /// <summary>
    /// Optional authenticator attachment information.
    /// </summary>
    [JsonPropertyName("authenticatorAttachment")]
    public string AuthenticatorAttachment { get; init; }

    /// <summary>
    /// Client extension results (optional).
    /// </summary>
    [JsonPropertyName("clientExtensionResults")]
    public AuthenticationExtensionsClientOutputs ClientExtensionResults { get; init; }
}

/// <summary>
/// DTO representing the response data within an AuthenticatorAttestationResponse.
/// </summary>
public class AuthenticatorAttestationResponseDataDTO
{
    /// <summary>
    /// JSON-serialized client data as a byte array.
    /// Contains challenge, origin, and other client information.
    /// </summary>
    [JsonConverter(typeof(Base64UrlConverter))]
    [JsonPropertyName("clientDataJSON")]
    public required byte[] ClientDataJson { get; init; }

    /// <summary>
    /// The attestation object containing the authenticator data and attestation statement.
    /// This includes the public key and attestation information.
    /// </summary>
    [JsonConverter(typeof(Base64UrlConverter))]
    [JsonPropertyName("attestationObject")]
    public required byte[] AttestationObject { get; init; }

    /// <summary>
    /// Optional transports supported by the authenticator. (soon to be required)
    /// </summary>
    [JsonPropertyName("transports")]
    public AuthenticatorTransport[] Transports { get; init; } = [];
}