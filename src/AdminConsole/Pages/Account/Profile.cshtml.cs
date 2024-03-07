using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.Pages.Account;

public class Profile : PageModel
{
    public string UserId => HttpContext.User.GetId();
}