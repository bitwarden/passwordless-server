using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Passwordless.AdminConsole.Pages.Account;

public class UserOnboarding : PageModel
{
    public string UserId => HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
}