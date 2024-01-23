namespace Passwordless.Common.Models.Apps;

public record DelistAuthenticatorsRequest(IEnumerable<Guid> AaGuids);