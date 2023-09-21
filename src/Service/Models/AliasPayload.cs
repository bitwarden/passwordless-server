namespace Passwordless.Service.Models;

using Aliases = HashSet<string>;

public class AliasPayload
{
    public string UserId { get; set; }
    public Aliases Aliases { get; set; }
    public bool Hashing { get; set; } = true;
}