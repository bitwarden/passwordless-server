using Passwordless.Service.Models;

namespace Passwordless.Service.Extensions.Models;

public static class AuthenticationConfigurationExtensions
{
    public static AuthenticationConfigurationDto ToDto(this AuthenticationConfiguration configuration) =>
        new()
        {
            Purpose = new SignInPurpose { Value = configuration.Purpose },
            UserVerificationRequirement = configuration.UserVerificationRequirement,
            TimeToLive = configuration.TimeToLive,
            Tenant = configuration.Tenant
        };
}