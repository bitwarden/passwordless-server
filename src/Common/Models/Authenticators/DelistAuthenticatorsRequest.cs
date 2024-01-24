using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Authenticators;

public record DelistAuthenticatorsRequest(
    [Required, MinLength(1)]
    IEnumerable<Guid> AaGuids);