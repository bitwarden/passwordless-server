using Fido2NetLib.Objects;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public class AuthenticationConfigurationDto
{
    public required SignInPurpose Purpose { get; set; }
    public UserVerificationRequirement UserVerificationRequirement { get; set; }
    public TimeSpan TimeToLive { get; set; }

    public required string Tenant { get; set; }

    public static AuthenticationConfigurationDto SignIn(string tenant) =>
        new()
        {
            Purpose = SignInPurposes.SignIn,
            UserVerificationRequirement = UserVerificationRequirement.Preferred,
            Tenant = tenant,
            TimeToLive = TimeSpan.FromMinutes(2)
        };

    public static AuthenticationConfigurationDto StepUp(string tenant) =>
        new()
        {
            Purpose = SignInPurposes.StepUp,
            UserVerificationRequirement = UserVerificationRequirement.Preferred,
            Tenant = tenant,
            TimeToLive = TimeSpan.FromMinutes(2)
        };
}