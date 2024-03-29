using System.ComponentModel.DataAnnotations;
using Fido2NetLib.Objects;

namespace Passwordless.Service.Models;

public class AuthenticationConfiguration : PerTenant
{
    [MaxLength(255)]
    public required string Purpose { get; set; }
    public UserVerificationRequirement UserVerificationRequirement { get; set; }
    public TimeSpan TimeToLive { get; set; }
}