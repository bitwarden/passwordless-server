using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages;

public class ClientModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _passwordlessClient;

    public ClientModel(ILogger<IndexModel> logger, IScopedPasswordlessClient passwordlessClient)
    {
        _logger = logger;
        this._passwordlessClient = passwordlessClient;
    }

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyToken(token);
        return new JsonResult(res);
    }
}
