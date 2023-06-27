using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class ListModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _api;

    public List<PasswordlessUserSummary> Users { get; set; }

    public ListModel(ILogger<IndexModel> logger, IScopedPasswordlessClient api)
    {
        _logger = logger;
        _api = api;
    }

    public async Task OnGet()
    {
        Users = await _api.ListUsers() ?? new List<PasswordlessUserSummary>();
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _api.VerifyToken(token);
        return new JsonResult(res);
    }
}
