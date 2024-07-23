using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings;

public partial class Settings : BaseApplicationPage
{
    public bool IsAttestationAllowed => CurrentContext.Features.AllowAttestation;

    public Application? Application { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        Application = await DataService.GetApplicationAsync(CurrentContext.AppId!);
        if (Application == null)
        {
            throw new InvalidOperationException("Application not found.");
        }
    }
}