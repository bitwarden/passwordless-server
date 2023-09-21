using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages.Account;

public class UserOnboarding : PageModel
{
    private readonly IPasswordlessClient _passwordlessClient;

    public UserOnboarding(IPasswordlessClient passwordlessClient)
    {
        _passwordlessClient = passwordlessClient;
    }

    public List<Credential> Credentials { get; set; }
    public async Task OnGet()
    {
        Credentials = await _passwordlessClient.ListCredentialsAsync(HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
    }
}