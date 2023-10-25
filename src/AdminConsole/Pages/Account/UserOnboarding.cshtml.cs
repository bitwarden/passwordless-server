using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Passwordless.AdminConsole.Pages.Account;

public class UserOnboarding : PageModel
{
    private readonly IPasswordlessClient _passwordlessClient;

    public UserOnboarding(IPasswordlessClient passwordlessClient)
    {
        _passwordlessClient = passwordlessClient;
    }

    public IReadOnlyCollection<Credential> Credentials { get; set; }

    public async Task OnGet()
    {
        Credentials = await _passwordlessClient.ListCredentialsAsync(HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
    }
}