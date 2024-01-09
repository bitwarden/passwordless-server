using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Playground;

public class NewAccountModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _passwordlessClient;

    public NewAccountModel(ILogger<IndexModel> logger, IScopedPasswordlessClient passwordlessClient)
    {
        _logger = logger;
        this._passwordlessClient = passwordlessClient;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostToken(string name, string email, string attestation)
    {
        try
        {
            var userId = Guid.NewGuid().ToString();
            var token = await _passwordlessClient.CreateRegisterTokenAsync(new RegisterOptions(userId, $"Playground: {email}")
            {
                DisplayName = name,
                Aliases = new HashSet<string>(1) { email },
                AliasHashing = false
            });

            return new JsonResult(token);
        }
        catch (PasswordlessApiException e)
        {
            return StatusCode(e.Details.Status, e.Details);
        }
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyTokenAsync(token);
        return new JsonResult(res);
    }

    [MaxLength(64)]
    public string Nickname { get; set; }

    public string Attestation { get; set; } = "none";
}