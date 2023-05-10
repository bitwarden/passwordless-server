namespace Passwordless.Api.Authorization;

public class DefaultProblemDetailWriter : IProblemDetailWriter
{
    private readonly string _alternateHeader;

    public DefaultProblemDetailWriter(string alternateHeader)
    {
        _alternateHeader = alternateHeader;
    }
    public IEnumerable<string> GetDetails(HttpContext context, string headerName)
    {
        // Did they even supply the needed header?
        if (context.Request.Headers.TryGetValue(headerName, out var headerValues))
        {
            var header = headerValues.First()!;

            if (header.StartsWith("verify_"))
            {
                yield return $"A verify token was supplied instead of your '{headerName}'";
            }
            else if (header.StartsWith("register_"))
            {
                yield return $"A register token was supplied instead of your '{headerName}'";
            }

            yield break;
        }

        if (context.Request.Headers.TryGetValue(_alternateHeader, out _))
        {
            yield return $"A '{_alternateHeader}' header was supplied when a '{headerName}' should have been supplied";

            yield break;
        }

        yield return $"A valid '{headerName}' header is required.";
    }
}