using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;
using AuthenticationConfiguration = Passwordless.Service.Models.AuthenticationConfiguration;

namespace Passwordless.Service.Extensions.Models;

public static class AuthenticationConfigurationExtensions
{
    public static AuthenticationConfigurationDto ToDto(this AuthenticationConfiguration configuration) =>
        new()
        {
            Purpose = new SignInPurpose(configuration.Purpose),
            UserVerificationRequirement = configuration.UserVerificationRequirement,
            TimeToLive = configuration.TimeToLive,
            Tenant = configuration.Tenant
        };
}