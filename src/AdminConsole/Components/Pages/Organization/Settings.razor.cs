using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.FeatureManagement;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Settings : ComponentBase
{
    private Models.Organization? _organization;
    private bool _allowDisablingMagicLinks;

    protected override async Task OnInitializedAsync()
    {
        _allowDisablingMagicLinks = await FeatureManager.IsEnabledAsync(FeatureFlags.Organization.AllowDisablingMagicLinks);
        _organization = await DataService.GetOrganizationWithDataAsync();
    }
}