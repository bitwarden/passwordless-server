using System.Web;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Helpers;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;

public partial class MagicLinksSection : ComponentBase
{
    public const string FormName = "magic-links-form";

    [SupplyParameterFromForm(FormName = FormName)]
    public SaveFormModel? Form { get; set; }

    public class SaveFormModel
    {
        public bool IsEnabled { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        Form ??= new SaveFormModel { IsEnabled = CurrentContext.Features.IsMagicLinksEnabled };
    }

    private async Task OnFormSubmittedAsync()
    {
        static bool? GetFinalValue(bool originalValue, bool postedValue) =>
            originalValue == postedValue ? null : postedValue;

        var name = HttpContextAccessor.HttpContext!.User.Identity!.Name;
        if (string.IsNullOrWhiteSpace(CurrentContext.AppId) || string.IsNullOrWhiteSpace(name))
        {
            NavigationManager.NavigateTo($"/Error?Message={HttpUtility.UrlEncode("Something unexpected happened.")}");
            return;
        }

        try
        {
            await ScopedPasswordlessClient.SetFeaturesAsync(new SetFeaturesRequest
            {
                PerformedBy = name,
                EnableMagicLinks = GetFinalValue(CurrentContext.Features.IsMagicLinksEnabled, Form.IsEnabled)
            });
            NavigationManager.Refresh();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save settings for {appId}", CurrentContext.AppId);
            NavigationManager.NavigateTo($"/Error?Message={HttpUtility.UrlEncode(ex.Message)}");
        }
    }
}