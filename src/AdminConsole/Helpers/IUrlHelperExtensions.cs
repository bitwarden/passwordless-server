namespace Microsoft.AspNetCore.Mvc;

public static class IUrlHelperExtensions
{
    public static string AppPage(this IUrlHelper url, string page, string app)
    {
        return url.Page(page, new { app = app });
    }
}