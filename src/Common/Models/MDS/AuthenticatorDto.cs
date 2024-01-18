namespace Passwordless.Common.Models.MDS;

public record AuthenticatorDto(
    Guid AaGuid,
    string Name,
    IEnumerable<string> Certification,
    IEnumerable<string> AttestationTypes);