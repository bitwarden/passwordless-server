using System.ComponentModel.DataAnnotations;
using Passwordless.Common.Validation;

namespace Passwordless.Service.Models;

public record AliasPayload
(
    [Required(AllowEmptyStrings = false)]
    string UserId,

    [MaxLength(10), MaxLengthCollection(250), RequiredCollection(AllowEmptyStrings = false)]
    HashSet<string> Aliases,

    bool Hashing = true
);