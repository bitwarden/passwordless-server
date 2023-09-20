using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class UserModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScopedPasswordlessClient _passwordlessClient;

    public List<Credential> Credentials { get; set; }
    public List<AliasPointer> Aliases { get; set; }

    public List<AuditLog> AuditLogs { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public string RegisterToken { get; set; }

    public UserModel(ILogger<IndexModel> logger, IScopedPasswordlessClient api)
    {
        _logger = logger;
        _passwordlessClient = api;
    }

    public async Task OnGet()
    {
        Credentials = await _passwordlessClient.ListCredentialsAsync(UserId);
        Aliases = await _passwordlessClient.ListAliasesAsync(UserId);
        // RegisterToken = await api.CreateRegisterToken(new PasswordlessApi.RegisterOptions {
        //     UserId = UserId,
        //     Username = username,
        //     DisplayName = displayName
        // })
        AuditLogs = new List<AuditLog> {
            new () {Timestamp = DateTime.Now, Level = "info", Message = "Signed in using credential ahjeas-aiwu12-an27s-jnb4-287hn58" },
            new () {Timestamp = DateTime.Now, Level = "danger", Message = "Authentication failed because of invalid signature. CredentialId: ahjeas-aiwu12-an27s-jnb4-287hn58." },
            new () {Timestamp = DateTime.Now, Level = "info", Message = "Signed in using credential ahjeas-aiwu12-an27s-jnb4-287hn58" },
            new () {Timestamp = DateTime.Now, Level = "info", Message = "Signed in using credential ahjeas-aiwu12-an27s-jnb4-287hn58" },
            new () {Timestamp = DateTime.Now, Level = "info", Message = "Registered new credential ahjeas-aiwu12-an27s-jnb4-287hn58" }
        };
    }

    public async Task<IActionResult> OnPost(string token)
    {
        var res = await _passwordlessClient.VerifyTokenAsync(token);
        return new JsonResult(res);
    }
}