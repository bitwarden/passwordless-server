using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Playground;

public class NewAccountModel(IScopedPasswordlessClient passwordlessClient)
    : PageModel
{
    [MaxLength(64)]
    public string Nickname { get; set; } = "";

    public string Attestation { get; set; } = "none";

    public string Hints { get; set; } = "";

    public async Task<IActionResult> OnPostToken(string name, string email, string attestation, string hints)
    {
        try
        {
            var userId = Guid.NewGuid().ToString();
            var token = await passwordlessClient.CreateRegisterTokenAsync(new RegisterOptions(userId, $"Playground: {email}")
            {
                DisplayName = name,
                Aliases = [email],
                AliasHashing = false,
                Attestation = attestation,
                Hints = hints.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
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
        var res = await passwordlessClient.VerifyAuthenticationTokenAsync(token);
        return new JsonResult(res);
    }
}