using Microsoft.AspNetCore.Mvc;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class CreateApiKeyModel
{
    public readonly string SelectedScopesField = nameof(SelectedScopes);

    public CreateApiKeyModel(IReadOnlyCollection<string> scopes)
    {
        Scopes = scopes;
    }

    public IReadOnlyCollection<string> Scopes { get; }

    [BindProperty] public List<string> SelectedScopes { get; } = new();
}