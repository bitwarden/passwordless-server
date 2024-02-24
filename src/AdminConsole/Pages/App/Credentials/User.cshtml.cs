using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Pages.Shared;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.AuthenticatorData;

namespace Passwordless.AdminConsole.Pages.App.Credentials;

public class UserModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _passwordlessClient;

    public CredentialsModel Credentials { get; }
    public IReadOnlyCollection<AliasPointer> Aliases { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public UserModel(
        IAuthenticatorDataProvider authenticatorDataProvider,
        ILogger<IndexModel> logger,
        IScopedPasswordlessClient api)
    {
        Credentials = new CredentialsModel(authenticatorDataProvider);
        _logger = logger;
        _passwordlessClient = api;
    }

    public async Task OnGet()
    {
        var items = await _passwordlessClient.ListCredentialsAsync(UserId);
        Credentials.SetItems(items);
        Aliases = await _passwordlessClient.ListAliasesAsync(UserId);
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyAuthenticationTokenAsync(token);
        return new JsonResult(res);
    }

    public async Task<IActionResult> OnPostRemoveCredential(string credentialId)
    {
        await _passwordlessClient.DeleteCredentialAsync(credentialId);
        return RedirectToPage();
    }
}