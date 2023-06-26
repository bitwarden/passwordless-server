using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class NewAccountModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _passwordlessClient;

    public NewAccountModel(ILogger<IndexModel> logger, IScopedPasswordlessClient passwordlessClient)
    {
        _logger = logger;
        this._passwordlessClient = passwordlessClient;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostToken(string name, string email)
    {
        // Create new account
        var userId = Guid.NewGuid().ToString();
        var token = await _passwordlessClient.CreateRegisterToken(new RegisterOptions()
        {
            UserId = userId,
            Username = "Playground: " + email,
            DisplayName = name,
            Aliases = new HashSet<string>(1) { email },
            AliasHashing = false
        });

        return new JsonResult(token);
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyToken(token);
        return new JsonResult(res);
    }
}