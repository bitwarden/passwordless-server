using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Pages.Shared;
using Passwordless.AdminConsole.Services.AuthenticatorData;

namespace Passwordless.AdminConsole.Pages.Account;

public class Profile : PageModel
{
    private readonly IPasswordlessClient _client;
    private readonly UserManager<ConsoleAdmin> _userManager;

    public Profile(
        IPasswordlessClient client,
        UserManager<ConsoleAdmin> userManager,
        IAuthenticatorDataService authenticatorDataService)
    {
        _client = client;
        _userManager = userManager;
        Credentials = new CredentialsModel(authenticatorDataService);
    }

    public CredentialsModel Credentials { get; }

    public async Task<IActionResult> OnPostRemoveCredential(string credentialId)
    {
        await _client.DeleteCredentialAsync(credentialId);
        return RedirectToPage();
    }

    public async Task OnGet()
    {
        var userId = _userManager.GetUserId(HttpContext.User);
        if (userId != null)
        {
            var items = await _client.ListCredentialsAsync(userId);
            Credentials.SetItemsAsync(items);
        }
        Credentials.HideDetails = false;
    }
}