namespace Passwordless.Common.Models.Apps;

public record AuthenticationConfiguration(string Purpose, int TimeToLive, string UserVerificationRequirement);