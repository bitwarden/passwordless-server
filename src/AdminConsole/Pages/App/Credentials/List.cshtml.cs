using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Credentials;

public class ListModel : PageModel
{
    private readonly IScopedPasswordlessClient _api;
    private readonly ICurrentContext _context;
    private readonly ILogger<IndexModel> _logger;

    public ListModel(IScopedPasswordlessClient api, ICurrentContext context, ILogger<IndexModel> logger)
    {
        _api = api;
        _context = context;
        _logger = logger;
    }

    public async Task OnGet()
    {
        Users = await _api.ListUsersAsync() ?? new List<PasswordlessUserSummary>();
        MaxUsers = _context.Features.MaxUsers;
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _api.VerifyAuthenticationTokenAsync(token);
        return new JsonResult(res);
    }

    public long? MaxUsers { get; private set; }

    public IReadOnlyCollection<PasswordlessUserSummary> Users { get; set; }
}