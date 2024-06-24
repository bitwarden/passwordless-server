using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Passwordless.Common.Models.Apps;

public class AuthenticationConfigurationDto
{
    public required SignInPurpose Purpose { get; set; }
    public UserVerificationRequirement UserVerificationRequirement { get; set; }
    public TimeSpan TimeToLive { get; set; }
    public required string Tenant { get; set; }

    public string? CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? CreatedOn { get; set; }

    public string? EditedBy { get; set; }
    public DateTimeOffset? EditedOn { get; set; }

    public DateTimeOffset? LastUsedOn { get; set; }

    public static AuthenticationConfigurationDto SignIn(string tenant) =>
        new()
        {
            Purpose = SignInPurpose.SignIn,
            UserVerificationRequirement = UserVerificationRequirement.Preferred,
            Tenant = tenant,
            TimeToLive = TimeSpan.FromMinutes(2),
            CreatedBy = "System"
        };

    public static AuthenticationConfigurationDto StepUp(string tenant) =>
        new()
        {
            Purpose = SignInPurpose.StepUp,
            UserVerificationRequirement = UserVerificationRequirement.Preferred,
            Tenant = tenant,
            TimeToLive = TimeSpan.FromMinutes(2),
            CreatedBy = "System"
        };

    public AuthenticationConfiguration ToResponse() => new(
        Purpose.Value,
        Convert.ToInt32(TimeToLive.TotalSeconds),
        UserVerificationRequirement.ToEnumMemberValue(),
        CreatedBy,
        CreatedOn,
        EditedBy,
        EditedOn,
        LastUsedOn);
}