using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings;

public partial class Settings : BaseApplicationPage
{
    public Application? Application { get; private set; }

    public bool ShowPlanSection => !CurrentContext.IsPendingDelete;

    public bool ShowApiKeysSection => !CurrentContext.IsPendingDelete;

    public bool ShowMagicLinksSection => !CurrentContext.IsPendingDelete;

    public bool ShowAttestationSection => !CurrentContext.IsPendingDelete && CurrentContext.Features.AllowAttestation;

    public bool ShowManuallyGeneratedAuthenticationTokensSection => !CurrentContext.IsPendingDelete;

    public bool ShowAuthenticationConfigurationSection => !CurrentContext.IsPendingDelete;

    protected override async Task OnInitializedAsync()
    {
        Application = await DataService.GetApplicationAsync(CurrentContext.AppId!);
        if (Application == null)
        {
            throw new InvalidOperationException("Application not found.");
        }
    }
}