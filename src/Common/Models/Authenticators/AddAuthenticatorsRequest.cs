using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Authenticators;

public record AddAuthenticatorsRequest(
    [Required, MinLength(1)]
    IReadOnlyCollection<Guid> AaGuids,

    bool IsAllowed);