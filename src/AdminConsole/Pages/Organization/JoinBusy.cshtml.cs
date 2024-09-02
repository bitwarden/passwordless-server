using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Passwordless.AdminConsole.Pages.Organization;

public class JoinBusy : PageModel
{
    public string InviteLink { get; set; } = string.Empty;

    public void OnGet(string code)
    {
        InviteLink = Url.PageLink("Join", null, new { code = code }) ?? string.Empty;
    }
}