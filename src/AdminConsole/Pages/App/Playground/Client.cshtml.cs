using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Playground;

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
        await InitializeAsync();
    }

    public async Task<IActionResult> OnPost(string token)
    {
        await InitializeAsync();

        var res = await _passwordlessClient.VerifyAuthenticationTokenAsync(token);
        return new JsonResult(res);
    }

    private async Task InitializeAsync()
    {
        Purposes = (await _passwordlessClient.GetAuthenticationConfigurationsAsync()).Configurations.Select(x => x.Purpose);
    }

    public IEnumerable<string> Purposes { get; private set; }
}