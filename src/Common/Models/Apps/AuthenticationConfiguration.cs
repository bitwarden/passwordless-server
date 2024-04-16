namespace Passwordless.Common.Models.Apps;

public record AuthenticationConfiguration(
    string Purpose,
    int TimeToLive,
    string UserVerificationRequirement,
    string CreatedBy,
    DateTimeOffset? CreatedOn,
    string? EditedBy,
    DateTimeOffset? EditedOn,
    DateTimeOffset? LastUsedOn);