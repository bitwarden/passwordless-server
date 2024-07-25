using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Pages.Account;

public class UserOnboarding : PageModel
{
    private readonly ICurrentContext _currentContext;

    public UserOnboarding(ICurrentContext currentContext)
    {
        _currentContext = currentContext;
    }

    public string UserId => HttpContext.User.GetId();

    public bool CanDeleteCredentials => _currentContext.Organization!.IsMagicLinksEnabled;

    public bool IsMagicLinksEnabled => _currentContext.Organization!.IsMagicLinksEnabled;
}