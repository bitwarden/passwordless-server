using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Pages.Shared;
using Passwordless.AdminConsole.Services.AuthenticatorData;

namespace Passwordless.AdminConsole.Pages.Account;

public class UserOnboarding : PageModel
{
    private readonly IPasswordlessClient _passwordlessClient;

    public UserOnboarding(
        IPasswordlessClient passwordlessClient,
        IAuthenticatorDataProvider authenticatorDataProvider)
    {
        _passwordlessClient = passwordlessClient;
        Credentials = new CredentialsModel(authenticatorDataProvider)
        {
            HideDetails = true
        };
    }

    public CredentialsModel Credentials { get; set; }

    public async Task OnGet()
    {
        var items = await _passwordlessClient.ListCredentialsAsync(HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        Credentials.SetItems(items);
    }
}