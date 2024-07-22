using System.ComponentModel.DataAnnotations;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.AuthenticationConfiguration;

public class AuthenticationConfigurationFormModel
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"^[\w\-]*$", ErrorMessage = "Characters are limited to A-z, 0-9, -, or _.")]
    [MaxLength(255)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    public UserVerificationRequirement UserVerificationRequirement { get; set; } = UserVerificationRequirement.Preferred;

    [Required]
    [Range(0, int.MaxValue)]
    public int Seconds { get; set; }

    public TimeSpan TimeToLive => TimeSpan.FromSeconds(Seconds);

    [Required]
    [RegularExpression(@"^[\w\s,]*$")]
    [MaxLength(255)]
    public string HintString { get; set; } = "";

    public IReadOnlyList<PublicKeyCredentialHint> Hints =>
        HintString.Split(',', StringSplitOptions.TrimEntries)
            .Select(s => s.ToEnum<PublicKeyCredentialHint>()).ToArray();

    public static AuthenticationConfigurationFormModel FromResult(Common.Models.Apps.AuthenticationConfiguration dto) =>
        new()
        {
            Purpose = dto.Purpose,
            UserVerificationRequirement = dto.UserVerificationRequirement.ToEnum<UserVerificationRequirement>(),
            Seconds = dto.TimeToLive,
            HintString = dto.Hints
        };
}