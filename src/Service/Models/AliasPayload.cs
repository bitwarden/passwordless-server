namespace Passwordless.Service.Models;

using Aliases = HashSet<string>;

public class AliasPayload
{
    public required string UserId { get; set; }
    public required Aliases Aliases { get; set; }
    public required bool Hashing { get; set; } = true;
}