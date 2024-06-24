using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Fido2NetLib.Objects;

namespace Passwordless.Common.Models.Apps;

public class SetAuthenticationConfigurationRequest
{
    [Required(AllowEmptyStrings = false), RegularExpression(@"^[\w\-]*$", ErrorMessage = "Characters are limited to A-z, 0-9, -, or _."), MaxLength(255)]
    public string Purpose { get; set; } = string.Empty;
    public UserVerificationRequirement UserVerificationRequirement { get; set; } = UserVerificationRequirement.Preferred;
    [PositiveTimeSpanValidator]
    public TimeSpan TimeToLive { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string PerformedBy { get; set; } = string.Empty;
}