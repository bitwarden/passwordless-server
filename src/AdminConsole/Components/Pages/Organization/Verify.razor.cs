using Microsoft.AspNetCore.Components;
using Passwordless.Common.Services.Mail.File;

namespace Passwordless.AdminConsole.Components.Pages.Organization;

public partial class Verify : ComponentBase
{
    public bool ShouldShowFileMailPath => WebHostEnvironment.IsDevelopment() && MailOptions.Value.Providers.All(p => p.Name.Equals(FileProviderOptions.Provider, StringComparison.InvariantCultureIgnoreCase));

    public string? FileMailPath { get; set; }

    public bool FileMailPathExists { get; set; }

    public string? FileMailContent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (HttpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false)
        {
            NavigationManager.NavigateTo("/account/useronboarding");
            return;
        }

        if (ShouldShowFileMailPath)
        {
            var fileMailProvider = MailOptions.Value.Providers.First() as FileProviderOptions;
            FileMailPath = Path.GetFullPath(fileMailProvider!.Path ?? FileProvider.DefaultPath);
            FileMailPathExists = File.Exists(FileMailPath);
            if (FileMailPathExists)
            {
                FileMailContent = await File.ReadAllTextAsync(FileMailPath);
            }
        }
    }
}