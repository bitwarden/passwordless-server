using System.Web;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Helpers;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;

public partial class ManuallyGeneratedAuthenticationTokensSection : ComponentBase
{
    public const string FormName = "manually-generated-authentication-tokens-form";

    public SaveFormModel? Form { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Form ??= new SaveFormModel { IsEnabled = CurrentContext.Features.IsGenerateSignInTokenEndpointEnabled };
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
                EnableManuallyGeneratedAuthenticationTokens = GetFinalValue(CurrentContext.Features.IsGenerateSignInTokenEndpointEnabled, Form!.IsEnabled)
            });
            NavigationManager.Refresh();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save settings for {appId}", CurrentContext.AppId);
            NavigationManager.NavigateTo($"/Error?Message={HttpUtility.UrlEncode(ex.Message)}");
        }
    }

    public class SaveFormModel
    {
        public bool IsEnabled { get; set; }
    }
}