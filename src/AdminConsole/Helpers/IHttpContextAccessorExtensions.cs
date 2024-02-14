using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Passwordless.AdminConsole.Helpers;

public static class IHttpContextAccessorExtensions
{
    public static bool IsRazorPages(this IHttpContextAccessor httpContextAccessor)
    {
        var metadata = httpContextAccessor.HttpContext?.GetEndpoint()?.Metadata.GetMetadata<PageModelAttribute>();
        return metadata != null;
    }
}