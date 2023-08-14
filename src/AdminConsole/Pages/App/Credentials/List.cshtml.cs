using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class ListModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IPasswordlessClient _api;

    public List<PasswordlessUserSummary> Users { get; set; }

    public ListModel(ILogger<IndexModel> logger, IPasswordlessClient api)
    {
        _logger = logger;
        _api = api;
    }

    public async Task OnGet()
    {
        Users = await _api.ListUsersAsync() ?? new List<PasswordlessUserSummary>();
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _api.VerifyTokenAsync(token);
        return new JsonResult(res);
    }
}