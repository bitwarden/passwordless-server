using System.ComponentModel.DataAnnotations;
using Fido2NetLib.Objects;
using MessagePack;
using Passwordless.Common.Validation;

namespace Passwordless.Service.Models;

[MessagePackObject]
public class RegisterToken : Token
{
    [MessagePack.Key(10)]
    [Required(AllowEmptyStrings = false)]
    public string UserId { get; set; }

    [MessagePack.Key(11)]
    public string DisplayName { get; set; }

    [MessagePack.Key(12)]
    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; }

    [MessagePack.Key(13)]
    public string Attestation { get; set; } = "None";

    [MessagePack.Key(14)]
    public string? AuthenticatorType { get; set; }

    [MessagePack.Key(15)]
    public bool Discoverable { get; set; } = true;

    [MessagePack.Key(16)]
    public string UserVerification { get; set; } = "Preferred";

    [MessagePack.Key(17)]
    [MaxLength(10), MaxLengthCollection(250), RequiredCollection(AllowEmptyStrings = false)]
    public HashSet<string>? Aliases { get; set; }

    [MessagePack.Key(18)]
    public bool AliasHashing { get; set; } = true;

    [MessagePack.Key(19)]
    [MaxLength(3)]
    public IReadOnlyList<PublicKeyCredentialHint> Hints { get; set; } = [];
}

[MessagePackObject]
public class VerifySignInToken : Token
{
    [MessagePack.Key(10)]
    public string UserId { get; set; }

    [MessagePack.Key(11)]
    public DateTime Timestamp { get; set; }

    [MessagePack.Key(12)]
    public string RpId { get; set; }

    [MessagePack.Key(13)]
    public string Origin { get; set; }

    [MessagePack.Key(14)]
    public bool Success { get; set; }

    [MessagePack.Key(15)]
    public string Device { get; set; }

    [MessagePack.Key(16)]
    public string Country { get; set; }

    [MessagePack.Key(17)]
    public string? Nickname { get; set; }

    [MessagePack.Key(18)]
    public byte[] CredentialId { get; set; }

    [MessagePack.Key(19)]
    public string Purpose { get; set; }

    [MessagePack.Key(20)]
    public string? AuthenticatorDisplayName { get; set; }
}

[MessagePackObject]
public class Token
{
    [MessagePack.Key(0)]
    public DateTime ExpiresAt { get; set; }

    [MessagePack.Key(1)]
    public Guid TokenId { get; set; }

    [MessagePack.Key(2)]
    public string Type { get; set; }
}