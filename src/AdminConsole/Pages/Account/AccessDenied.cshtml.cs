using AdminConsole.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminConsole.Pages.Account;

public class AccessDenied : PageModel
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    public string ReturnUrl { get; set; }

    public AccessDenied(SignInManager<ConsoleAdmin> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task OnGet()
    {
        var user = await _signInManager.UserManager.GetUserAsync(User);
        await _signInManager.RefreshSignInAsync(user);
        var url = Request.Query["ReturnUrl"];
        if (Url.IsLocalUrl(url))
        {
            ReturnUrl = url;
        }
    }
}