using System.ComponentModel.DataAnnotations;
using Fido2NetLib;
using MessagePack;
using Passwordless.Service.Helpers;
using Passwordless.Service.Validation;

namespace Passwordless.Service.Models;

public class StepUpTokenRequest : RequestBase
{
    [Required]
    public required AuthenticatorAssertionRawResponse Response { get; set; }
    [Required]
    public required string Session { get; set; }
    [Required]
    public required StepUpOptions Context { get; set; }
}

public record StepUpOptions(int TimeToLive, string Context);

[MessagePackObject]
public class StepUpToken : Token
{
    private const string StepUp = "step_up";

    public StepUpToken()
    {
        Type = StepUp;
    }

    [MessagePack.Key(10)]
    public required string UserId { get; set; }

    [MessagePack.Key(11)]
    public required DateTime CreatedAt { get; set; }

    [MessagePack.Key(12)]
    public required string RpId { get; set; }

    [MessagePack.Key(13)]
    public required string Origin { get; set; }

    [MessagePack.Key(14)]
    public bool Success { get; set; }

    [MessagePack.Key(15)]
    public string Device { get; set; }

    [MessagePack.Key(16)]
    public string Country { get; set; }

    [MessagePack.Key(17)]
    public required string Context { get; set; }
}

public record StepUpVerifyRequest(string Token, string Context);

public static class StepUpTokenValidator
{
    public static void Validate(this StepUpToken token, DateTimeOffset now, string context)
    {
        token.Validate(now);

        if (string.Equals(token.Context, context, StringComparison.OrdinalIgnoreCase)) return;

        throw new ApiException("invalid_context", $"The token was not for the given context ({context})", 403);
    }
}