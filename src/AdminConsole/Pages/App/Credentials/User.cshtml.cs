using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.AuthenticatorData;

namespace Passwordless.AdminConsole.Pages.App.Credentials;

public class UserModel : PageModel
{
    public IReadOnlyCollection<AliasPointer> Aliases { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public UserModel(IScopedPasswordlessClient api)
    {
        PasswordlessClient = api;
    }

    public IScopedPasswordlessClient PasswordlessClient { get; }

    public async Task OnGet()
    {
        Aliases = await PasswordlessClient.ListAliasesAsync(UserId);
    }
}