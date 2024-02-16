using Passwordless.Helpers;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public class ProblemDetailsDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<ProblemDetailsDelegatingHandler> logger;

    public ProblemDetailsDelegatingHandler(ILogger<ProblemDetailsDelegatingHandler> logger)
    {
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (string.Equals(
                response.Content.Headers.ContentType?.MediaType,
                "application/problem+json",
                StringComparison.OrdinalIgnoreCase)
           )
        {
            var problemDetails = await response.Content.ReadFromJsonAsync(
                PasswordlessSerializerContext.Default.PasswordlessProblemDetails,
                cancellationToken
            );

            if (problemDetails is not null)
                throw new PasswordlessApiException(problemDetails);
        }

        return response;
    }
}