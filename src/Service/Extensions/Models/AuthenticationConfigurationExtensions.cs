using Passwordless.Common.Models.Apps;
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
            Tenant = configuration.Tenant,
            CreatedBy = configuration.CreatedBy,
            CreatedOn = configuration.CreatedOn,
            EditedBy = configuration.EditedBy,
            EditedOn = configuration.EditedOn,
            LastUsedOn = configuration.LastUsedOn
        };
}