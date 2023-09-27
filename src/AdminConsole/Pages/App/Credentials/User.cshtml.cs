using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class UserModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _passwordlessClient;

    public IReadOnlyCollection<Credential> Credentials { get; set; }
    public IReadOnlyCollection<AliasPointer> Aliases { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public string RegisterToken { get; set; }

    public UserModel(ILogger<IndexModel> logger, IScopedPasswordlessClient api)
    {
        _logger = logger;
        _passwordlessClient = api;
    }

    public async Task OnGet()
    {
        Credentials = await _passwordlessClient.ListCredentialsAsync(UserId);
        Aliases = await _passwordlessClient.ListAliasesAsync(UserId);
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyTokenAsync(token);
        return new JsonResult(res);
    }
}