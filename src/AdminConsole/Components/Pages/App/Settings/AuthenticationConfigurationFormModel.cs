using System.ComponentModel.DataAnnotations;
using Fido2NetLib.Objects;
using Passwordless.AdminConsole.Helpers;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings;

public class AuthenticationConfigurationFormModel
{
    [Required, MinLength(1), RegularExpression(@"^[\w\-\.]*$", ErrorMessage = "Characters are limited to A-z, 0-9, -, ., or _.")]
    public string Purpose { get; set; } = string.Empty;

    [Required] public UserVerificationRequirement UserVerificationRequirement { get; set; } = UserVerificationRequirement.Preferred;

    [Required, Range(0, int.MaxValue)] public int Seconds { get; set; }

    [Required, Range(0, int.MaxValue)] public int Minutes { get; set; }

    [Required, Range(0, int.MaxValue)] public int Hours { get; set; }

    [Required, Range(0, int.MaxValue)] public int Days { get; set; }

    public TimeSpan TimeToLive => new TimeSpan()
        .AddSeconds(Seconds)
        .AddMinutes(Minutes)
        .AddHours(Hours)
        .AddDays(Days);

    public static AuthenticationConfigurationFormModel FromDto(AuthenticationConfigurationDto dto) => new()
    {
        Purpose = dto.Purpose.Value,
        UserVerificationRequirement = dto.UserVerificationRequirement,
        Seconds = dto.TimeToLive.Seconds,
        Minutes = dto.TimeToLive.Minutes,
        Hours = dto.TimeToLive.Hours,
        Days = dto.TimeToLive.Days
    };
}