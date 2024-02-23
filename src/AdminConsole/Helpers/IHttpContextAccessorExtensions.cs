using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Passwordless.AdminConsole.Helpers;

public static class IHttpContextAccessorExtensions
{
    /// <summary>
    /// Detect whether the rendered page is a Razor Page or something else (Blazor).
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    public static bool IsRazorPages(this IHttpContextAccessor httpContextAccessor)
    {
        var metadata = httpContextAccessor.HttpContext?.GetEndpoint()?.Metadata.GetMetadata<PageModelAttribute>();
        return metadata != null;
    }
}