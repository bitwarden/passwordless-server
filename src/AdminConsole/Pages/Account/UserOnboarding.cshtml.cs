using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.Pages.Account;

public class UserOnboarding : PageModel
{
    public string UserId => HttpContext.User.GetId();
}