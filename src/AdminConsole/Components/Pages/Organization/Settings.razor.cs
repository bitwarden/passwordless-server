using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Settings : ComponentBase
{
    private Models.Organization? _organization;

    protected override async Task OnInitializedAsync()
    {
        _organization = await DataService.GetOrganizationWithDataAsync();
    }
}