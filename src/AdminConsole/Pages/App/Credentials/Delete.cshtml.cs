using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class CredentialDeleteModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PasswordlessClient api;

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string CredentialId { get; set; }

    public Credential Credential { get; set; }

    public CredentialDeleteModel(ILogger<IndexModel> logger, PasswordlessClient api)
    {
        _logger = logger;
        this.api = api;
    }

    public async Task<IActionResult> OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        await api.DeleteCredential(CredentialId);

        return RedirectToPage("/app/credentials/user", null, new { UserId = UserId });
    }
}