using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.AuthenticatorData;

namespace Passwordless.AdminConsole.Pages.Account;

public class Profile : PageModel
{
    private readonly IPasswordlessClient _client;
    private readonly UserManager<ConsoleAdmin> _userManager;

    public Profile(
        IPasswordlessClient client,
        UserManager<ConsoleAdmin> userManager,
        IAuthenticatorDataProvider authenticatorDataProvider)
    {
        _client = client;
        _userManager = userManager;
    }

    public IReadOnlyCollection<Credential> Credentials { get; set; }

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
            Credentials = await _client.ListCredentialsAsync(userId);
        }
    }
}