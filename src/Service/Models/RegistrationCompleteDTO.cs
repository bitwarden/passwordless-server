using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.Models;

public class RegistrationCompleteDTO : RequestBase
{
    public string Session { get; set; }

    public AuthenticatorAttestationResponseDTO Response { get; set; }

    [MaxLength(64, ErrorMessage = "Nickname cannot be longer than 64 characters.")]
    public string Nickname { get; set; }
}