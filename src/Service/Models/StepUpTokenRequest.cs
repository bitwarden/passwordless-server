using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace Passwordless.Service.Models;

/// <summary>
/// Represents a request to step up the authentication process using a token.
/// </summary>
public class StepUpTokenRequest : RequestBase
{
    [Required]
    public AuthenticatorAssertionRawResponse Response { get; set; }
    [Required]
    public required string Session { get; set; }
    [Required]
    public required StepUpOptions Context { get; set; }
}

/// <summary>
/// Options available for setting the context and TTL for the token.
/// </summary>
/// <param name="TimeToLive">(Optional) Length of time token will be valid for in seconds. Default is 900s (15 min)</param>
/// <param name="Context"></param>
public record StepUpOptions(
    [Range(1, 86400)] int? TimeToLive,
    [MinLength(1, ErrorMessage = "Context must not be an empty string.")] string Context);