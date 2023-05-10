using AdminConsole.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages.Account;

public class Profile : PageModel
{
    private readonly IPasswordlessClient _client;
    private readonly UserManager<ConsoleAdmin> _userManager;

    public Profile(IPasswordlessClient client, UserManager<ConsoleAdmin> userManager)
    {
        _client = client;
        _userManager = userManager;
    }

    public List<Credential> Credentials { get; set; } = new();

    public async Task<IActionResult> OnPostRemoveCredential(string credentialId)
    {
        await _client.DeleteCredential(credentialId);
        return RedirectToPage();
    }

    public async Task OnGet()
    {
        var userId = _userManager.GetUserId(HttpContext.User);
        if (userId != null)
        {
            Credentials = await _client.ListCredentials(userId);
        }
    }
}