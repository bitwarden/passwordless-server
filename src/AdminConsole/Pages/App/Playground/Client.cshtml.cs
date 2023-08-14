using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class ClientModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IPasswordlessClient _passwordlessClient;

    public ClientModel(ILogger<IndexModel> logger, IPasswordlessClient passwordlessClient)
    {
        _logger = logger;
        this._passwordlessClient = passwordlessClient;
    }

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyTokenAsync(token);
        return new JsonResult(res);
    }
}