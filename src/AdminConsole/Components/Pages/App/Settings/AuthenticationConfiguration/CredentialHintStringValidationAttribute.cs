using System.ComponentModel.DataAnnotations;
using Fido2NetLib.Objects;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.AuthenticationConfiguration;

public class CredentialHintStringValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string str)
            return false;

        var hints = str
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
                Enum.TryParse<PublicKeyCredentialHint>(s, true, out var hint)
                    ? hint
                    : default(PublicKeyCredentialHint?))
            .ToArray();

        // All valid
        if (hints.Any(h => h is null))
            return false;

        // No duplicates
        if (hints.Length != hints.Distinct().Count())
            return false;

        return true;
    }
}