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
            var headerValue = headerValues.First()!;

            if (headerValue.StartsWith("verify_"))
            {
                yield return $"A verify token was supplied instead of your '{headerName}'.";
            }
            else if (headerValue.StartsWith("register_"))
            {
                yield return $"A register token was supplied instead of your '{headerName}'.";
            }
            else if (headerName == "ApiSecret" && headerValue.Contains(":public:"))
            {
                yield return $"Your ApiSecret header contained a public ApiKey instead of your 'ApiSecret'.";
            }
            else if (headerName == "ApiKey" && headerValue.Contains(":secret:"))
            {
                yield return $"Your ApiKey header contained a ApiSecret instead of your 'ApiKey'.";
            }
            else if ((headerName == "ApiSecret" && !headerValue.Contains("secret")) || headerName == "ApiKey" && !headerValue.Contains("public"))
            {
                // get first 10 characters
                var first10 = headerValue.Substring(0, Math.Min(headerValue.Length, 10));

                yield return
                    $"We don't recognize the value you supplied for your '{headerName}'. It started with: '{first10}'.";
            }
            else
            {
                yield return $"The value of your '{headerName}' is not valid.";
            }

            yield break;
        }


        if (context.Request.Headers.TryGetValue(_alternateHeader, out _))
        {
            yield return $"A '{_alternateHeader}' header was supplied when a '{headerName}' header should have been supplied.";

            yield break;
        }

        yield return $"A valid '{headerName}' header is required.";
    }
}