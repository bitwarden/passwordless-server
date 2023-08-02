using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Passwordless.Net;

namespace AdminConsole.Pages.Components.ManagePasskeys;

public class ManagePasskeys : ViewComponent
{
    private readonly IPasswordlessClient _client;

    public ManagePasskeys(IPasswordlessClient client)
    {
        _client = client;
    }

    public async Task<ViewViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        var m = new PasskeysModel();
        if (userId != null)
        {
            m.Credentials = await _client.ListCredentialsAsync(userId);
        }

        return View(m);
    }
}

public class PasskeysModel
{
    public List<Credential> Credentials { get; set; } = new();
    public string? ApiKey { get; set; }
    public string? ApiUrl { get; set; }
}