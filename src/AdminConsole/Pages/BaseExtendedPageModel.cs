using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.RoutingHelpers;

namespace Passwordless.AdminConsole.Pages;

public abstract class BaseExtendedPageModel : PageModel
{
    public RedirectToPageResult RedirectToApplicationPage(string? pageName, ApplicationPageRoutingContext routeValues)
        => RedirectToPage(pageName, pageHandler: null, routeValues: routeValues, fragment: null);
}