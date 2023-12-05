using Fido2NetLib;

namespace Passwordless.Service.Models;

public class RegisterSession
{
    public required CredentialCreateOptions Options { get; set; }
    public required HashSet<string> Aliases { get; set; }
    public required bool AliasHashing { get; set; } = true;
}