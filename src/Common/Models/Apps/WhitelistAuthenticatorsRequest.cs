namespace Passwordless.Common.Models.Apps;

public record WhitelistAuthenticatorsRequest(IEnumerable<Guid> AaGuids);