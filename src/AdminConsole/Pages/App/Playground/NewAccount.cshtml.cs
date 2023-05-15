using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class NewAccountModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PasswordlessClient api;

    public NewAccountModel(ILogger<IndexModel> logger, PasswordlessClient api)
    {
        _logger = logger;
        this.api = api;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostToken(string name, string email)
    {
        // Create new account
        var userId = Guid.NewGuid().ToString();
        var token = await api.CreateRegisterToken(new RegisterOptions()
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
        var res = await api.VerifyToken(token);
        return new JsonResult(res);
    }
}