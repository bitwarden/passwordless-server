using System.ComponentModel.DataAnnotations;
using Fido2NetLib.Objects;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.AuthenticationConfiguration;

public class CredentialHintStringValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string str)
            return false;

        var hints = str.Split(',', StringSplitOptions.TrimEntries);
        return hints.All(hint => Enum.TryParse<PublicKeyCredentialHint>(hint, out _));
    }
}