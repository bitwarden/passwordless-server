using System.Net.Http.Json;
using Passwordless.Net.Helpers;

namespace Passwordless.Net;

internal class PasswordlessDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (request.ShouldSkipErrorHandling())
        {
            return response;
        }

        if (!response.IsSuccessStatusCode
            && string.Equals(response.Content.Headers.ContentType?.MediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
        {
            // Attempt to read problem details
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, cancellationToken: cancellationToken);

            // Throw exception
            throw new PasswordlessApiException(problemDetails!);
        }

        return response;
    }
}
