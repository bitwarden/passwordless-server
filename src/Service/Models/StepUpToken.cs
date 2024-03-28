using MessagePack;

namespace Passwordless.Service.Models;


[MessagePackObject]
public class StepUpToken : Token
{
    private const string StepUp = "step_up";

    public StepUpToken()
    {
        Type = StepUp;
    }

    [Key(10)]
    public required string UserId { get; set; }

    [Key(11)]
    public required DateTime CreatedAt { get; set; }

    [Key(12)]
    public required string RpId { get; set; }

    [Key(13)]
    public required string Origin { get; set; }

    [Key(14)]
    public bool Success { get; set; }

    [Key(15)]
    public string Device { get; set; }

    [Key(16)]
    public string Country { get; set; }

    [Key(17)]
    public required string Context { get; set; }
}