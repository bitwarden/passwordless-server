using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages;

public class ListModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _api;

    public IReadOnlyCollection<PasswordlessUserSummary> Users { get; set; }

    public ListModel(ILogger<IndexModel> logger, IScopedPasswordlessClient api, ICurrentContext context)
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