using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.Net;

namespace AdminConsole.Pages;

public class UserModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PasswordlessClient api;

    public List<Credential> Credentials { get; set; }
    public List<AliasPointer> Aliases { get; set; }

    public List<AuditLog> AuditLogs { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public string RegisterToken { get; set; }

    public UserModel(ILogger<IndexModel> logger, PasswordlessClient api)
    {
        _logger = logger;
        this.api = api;
    }

    public async Task OnGet()
    {
        Credentials = await api.ListCredentials(UserId);
        Aliases = await api.ListAliases(UserId);
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
        var res = await api.VerifyToken(token);
        return new JsonResult(res);
    }
}