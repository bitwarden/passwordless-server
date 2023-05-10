using System.Net;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Api.IntegrationTests;

public class AuthorizationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    // Manual opt out for endpoints that allow anonymous access, all other endpoints are considered to require
    // some kind of authentication
    private static readonly string[] _anonEndpoints = new[]
    {
        "/",
        "/apps/available",
        "/apps/delete/cancel/{appId}",
        "health/http",
        "health/storage",
        "health/throw/api",
        "health/throw/exception",
        "health/version"
    };


    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthorizationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ValidateThatEndpointsHaveProtection()
    {
        var endpointDataSource = _factory.Services.GetRequiredService<EndpointDataSource>();

        foreach (var endpoint in endpointDataSource.Endpoints.OfType<RouteEndpoint>())
        {
            if (_anonEndpoints.Contains(endpoint.RoutePattern.RawText))
            {
                continue;
            }

            var httpMethodMetadata = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
            if (httpMethodMetadata == null)
            {
                continue;
            }

            foreach (var httpMethod in httpMethodMetadata.HttpMethods)
            {
                var request = new HttpRequestMessage(new HttpMethod(httpMethod), CreateRoute(endpoint.RoutePattern));
                var response = await _client.SendAsync(request);
                Assert.True(HttpStatusCode.Unauthorized == response.StatusCode,
                    $"Expected route: '{endpoint.RoutePattern.RawText}' to response with 401 Unauthorized but it responsed with {response.StatusCode}");
            }
        }
    }

    private static string? CreateRoute(RoutePattern pattern)
    {
        // TODO: We may have to get smarter with generating this route
        return pattern.RawText;
    }
}