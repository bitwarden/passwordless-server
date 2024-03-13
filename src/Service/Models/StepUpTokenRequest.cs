using System.ComponentModel.DataAnnotations;
using MessagePack;

namespace Passwordless.Service.Models;

public class StepUpTokenRequest : RequestBase
{
    [Required]
    public required string UserId { get; set; }
    public StepUpOptions Options { get; set; }
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
    public required DateTimeOffset CreatedAt { get; set; }
    
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
}