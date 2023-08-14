using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace Passwordless.Service.Models;

public class RegistrationCompleteDTO : RequestBase
{
    public string Session { get; set; }

    public AuthenticatorAttestationRawResponse Response { get; set; }

    [MaxLength(255)]
    public string Nickname { get; set; }
}