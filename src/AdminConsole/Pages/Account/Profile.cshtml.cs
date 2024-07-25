using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Pages.Account;

public class Profile : PageModel
{
    private readonly ICurrentContext _currentContext;

    public Profile(ICurrentContext currentContext)
    {
        _currentContext = currentContext;
    }

    public string UserId => HttpContext.User.GetId();

    public bool CanDeleteCredentials => _currentContext.Organization!.IsMagicLinksEnabled;
}