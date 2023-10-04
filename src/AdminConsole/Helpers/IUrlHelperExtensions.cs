using Microsoft.AspNetCore.Mvc;

namespace Passwordless.AdminConsole.Helpers;

public static class IUrlHelperExtensions
{
    public static string AppPage(this IUrlHelper url, string page, string app)
    {
        return url.Page(page, new { app = app });
    }
}