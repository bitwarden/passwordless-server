using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public record DelistAuthenticatorsRequest(
    [Required, MinLength(1)]
    IEnumerable<Guid> AaGuids);