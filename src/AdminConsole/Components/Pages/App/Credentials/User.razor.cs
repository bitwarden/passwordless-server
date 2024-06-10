using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Pages.App.Credentials;

public partial class User : BaseApplicationPage
{
    private const bool HideCredentialDetails = false;

    public IReadOnlyCollection<AliasPointer>? Aliases { get; set; }

    [Parameter]
    public required string UserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Aliases = await ApiClient.ListAliasesAsync(UserId);
    }
}