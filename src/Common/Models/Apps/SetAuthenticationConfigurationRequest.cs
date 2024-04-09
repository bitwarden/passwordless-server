using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Fido2NetLib.Objects;

namespace Passwordless.Common.Models.Apps;

public class SetAuthenticationConfigurationRequest
{
    [Required] public string Purpose { get; set; } = string.Empty;
    public UserVerificationRequirement UserVerificationRequirement { get; set; } = UserVerificationRequirement.Preferred;
    [PositiveTimeSpanValidator] public TimeSpan TimeToLive { get; set; }
}