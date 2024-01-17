namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class FileVersionProviderExtensions
{
    public static string Get(this IFileVersionProvider provider, string path) =>
        provider.AddFileVersionToPath("", path);

    public static string Get(this IFileVersionProvider provider, IHttpContextAccessor accessor, string path) =>
        provider.AddFileVersionToPath(accessor.HttpContext!.Request.PathBase, path);
}