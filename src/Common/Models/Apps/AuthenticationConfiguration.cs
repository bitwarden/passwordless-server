namespace Passwordless.Common.Models.Apps;

public record AuthenticationConfiguration(
    string Purpose,
    string UserVerificationRequirement,
    int TimeToLive,
    string Hints,
    string CreatedBy,
    DateTimeOffset? CreatedOn,
    string? EditedBy,
    DateTimeOffset? EditedOn,
    DateTimeOffset? LastUsedOn);