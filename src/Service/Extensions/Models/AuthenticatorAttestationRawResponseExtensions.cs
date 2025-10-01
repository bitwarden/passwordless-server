using Fido2NetLib;
using Fido2NetLib.Objects;
using Passwordless.Service.Models;

namespace Passwordless.Service.Extensions.Models;

public static class Fido2MappingExtensions
{
    public static AuthenticatorAttestationRawResponse ToRawResponse(this AuthenticatorAttestationResponseDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Id);

        if (dto.RawId is { Length: 0 }) throw new ArgumentException("RawId was empty");

        if (dto.Type == PublicKeyCredentialType.Invalid) throw new ArgumentException("PublicKeyCredentialType was invalid");

        return new AuthenticatorAttestationRawResponse
        {
            Id = dto.Id,
            RawId = dto.RawId,
            Type = dto.Type,
            Response = dto.Response.ToResponse(),
            Extensions = dto.ClientExtensionResults
        };
    }

    public static AuthenticatorAttestationRawResponse.AttestationResponse ToResponse(
        this AuthenticatorAttestationResponseDataDTO dto)
    {
        return new AuthenticatorAttestationRawResponse.AttestationResponse
        {
            AttestationObject = dto.AttestationObject,
            ClientDataJson = dto.ClientDataJson,
            Transports = dto.Transports
        };
    }
}