using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace Passwordless.Service.Models;

public class RegistrationCompleteDTO : RequestBase
{
    public required string Session { get; set; }

    public required AuthenticatorAttestationRawResponse Response { get; set; }

    [MaxLength(64, ErrorMessage = "Nickname cannot be longer than 64 characters.")]
    public string? Nickname { get; set; }
}