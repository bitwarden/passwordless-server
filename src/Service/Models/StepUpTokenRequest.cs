using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace Passwordless.Service.Models;

public class StepUpTokenRequest : RequestBase
{
    [Required]
    public AuthenticatorAssertionRawResponse Response { get; set; }
    [Required]
    public required string Session { get; set; }
    [Required]
    public required StepUpOptions Context { get; set; }
}

public record StepUpOptions(int TimeToLive, string Context);