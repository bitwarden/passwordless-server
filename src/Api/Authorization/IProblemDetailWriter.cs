namespace Passwordless.Api.Authorization;

public interface IProblemDetailWriter
{
    IEnumerable<string> GetDetails(HttpContext context, string headerName);
}