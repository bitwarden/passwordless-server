using Fido2NetLib.Objects;

namespace Passwordless.Service.Models;

public class AuthenticationConfigurationDto
{
    public required SignInPurpose Purpose { get; set; }
    public UserVerificationRequirement UserVerificationRequirement { get; set; }
    public TimeSpan TimeToLive { get; set; }

    public required string Tenant { get; set; }

    public static AuthenticationConfigurationDto Default(string tenant) =>
        new()
        {
            Purpose = SignInPurposes.SignIn,
            UserVerificationRequirement = UserVerificationRequirement.Preferred,
            Tenant = tenant,
            TimeToLive = TimeSpan.FromMinutes(2)
        };
}