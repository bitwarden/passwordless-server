using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public record WhitelistAuthenticatorsRequest(
    [Required, MinLength(1)]
    IEnumerable<Guid> AaGuids);