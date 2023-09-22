namespace Passwordless.Api.Helpers;

public static class HttpContextExtensions
{
    /// <summary>
    /// https://github.com/dotnet/aspnetcore/pull/45158
    /// </summary>
    /// <param name="context"></param>
    [Obsolete("Remove and retest on .NET 8")]
    public static void AddProblemDetailsMissingAcceptHeader(this HttpContext context)
    {
#if !NET8_0_OR_GREATER
        if (!context.Request.Headers.Accept.Any())
        {
            context.Request.Headers.Add("Accept", "application/json, application/problem+details");
        }
#endif
    }
}