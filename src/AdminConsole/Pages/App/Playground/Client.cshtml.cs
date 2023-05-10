using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class ClientModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PasswordlessClient api;

    public ClientModel(ILogger<IndexModel> logger, PasswordlessClient api)
    {
        _logger = logger;
        this.api = api;
    }

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await api.VerifyToken(token);
        return new JsonResult(res);
    }
}