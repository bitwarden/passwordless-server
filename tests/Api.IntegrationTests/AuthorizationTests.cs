using System.Net;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Api.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests;

public class AuthorizationTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    // Manual opt out for endpoints that allow anonymous access, all other endpoints are considered to require
    // some kind of authentication
    private static readonly string[] AnonEndpoints =
    [
        "/",
        "/apps/available",
        "/apps/delete/cancel/{appId}",
        "health/http",
        "health/storage",
        "health/throw/api",
        "health/throw/exception",
        "health/version"
    ];


    [Fact]
    public async Task ValidateThatEndpointsHaveProtectionAsync()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
{

    TestOutput = testOutput
});
        using var client = api.CreateClient().AddAcceptApplicationJson();

        // Act
        var endpointDataSource = api.Services.GetRequiredService<EndpointDataSource>();
        foreach (var endpoint in endpointDataSource.Endpoints.OfType<RouteEndpoint>())
        {
            if (AnonEndpoints.Contains(endpoint.RoutePattern.RawText))
            {
                continue;
            }

            var httpMethodMetadata = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
            if (httpMethodMetadata == null)
            {
                continue;
            }

            // Assert
            foreach (var httpMethod in httpMethodMetadata.HttpMethods)
            {
                using var request = new HttpRequestMessage(new HttpMethod(httpMethod), CreateRoute(endpoint.RoutePattern));
                using var response = await client.SendAsync(request);

                Assert.True(HttpStatusCode.Unauthorized == response.StatusCode,
                    $"Expected route: '{endpoint.RoutePattern.RawText}' to response with 401 Unauthorized but it responsed with {response.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task ValidateThatMissingApiSecretThrowsAsync()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
{

    TestOutput = testOutput
});
        using var client = api.CreateClient().AddAcceptApplicationJson();

        // Act
        using var response = await client.GetAsync("/credentials/list?userId=1");

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        AssertHelper.AssertEqualJson("""
        {
            "type": "https://docs.passwordless.dev/guide/errors.html#ApiSecret",
            "title": "A valid 'ApiSecret' header is required.",
            "status": 401,
            "detail": "A valid 'ApiSecret' header is required."
        }
        """, body);
    }

    [Fact]
    public async Task ValidateThatInvalidApiSecretThrowsAsync()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
{

    TestOutput = testOutput
});
        using var client = api.CreateClient().AddAcceptApplicationJson();

        // Act
        using var response = await client
            .AddSecretKey(HttpClientTestExtensions.ApiSecret + "invalid")
            .GetAsync("/credentials/list?userId=1");

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        AssertHelper.AssertEqualJson("""
        {
            "type": "https://docs.passwordless.dev/guide/errors.html#ApiSecret",
            "title": "A valid 'ApiSecret' header is required.",
            "status": 401,
            "detail": "The value of your 'ApiSecret' is not valid."
        }
        """, body);
    }

    [Theory]
    [InlineData("test:public:123", "Your ApiSecret header contained a public ApiKey instead of your 'ApiSecret'.")]
    [InlineData("verify_123", "A verify token was supplied instead of your 'ApiSecret'.")]
    [InlineData("register_13", "A register token was supplied instead of your 'ApiSecret'.")]
    [InlineData("somethingrandom", "We don't recognize the value you supplied for your 'ApiSecret'. It started with: 'somethingr'.")]
    [InlineData("", "A valid 'ApiSecret' header is required.")]
    [InlineData("missing", "A valid 'ApiSecret' header is required.")]
    [InlineData("public-header-instead", "A 'ApiKey' header was supplied when a 'ApiSecret' header should have been supplied.")]
    public async Task ApiSecretGivesHelpfulAdviceAsync(string input, string details)
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
{

    TestOutput = testOutput
});
        using var client = api.CreateClient().AddAcceptApplicationJson();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/credentials/list?userId=1");

        if (input == "public-header-instead")
        {
            request.Headers.Add("ApiKey", "something");
        }
        else if (input != "missing")
        {
            request.Headers.Add("ApiSecret", input);
        }

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        AssertHelper.AssertEqualJson($$"""
        {
            "type": "https://docs.passwordless.dev/guide/errors.html#ApiSecret",
            "title": "A valid 'ApiSecret' header is required.",
            "status": 401,
            "detail": "{{details}}"
        }
        """, body);
    }

    [Theory]
    [InlineData("test:secret:123", "Your ApiKey header contained a ApiSecret instead of your 'ApiKey'.")]
    [InlineData("verify_123", "A verify token was supplied instead of your 'ApiKey'.")]
    [InlineData("register_123", "A register token was supplied instead of your 'ApiKey'.")]
    [InlineData("somethingrandom", "We don't recognize the value you supplied for your 'ApiKey'. It started with: 'somethingr'.")]
    [InlineData("", "A valid 'ApiKey' header is required.")]
    [InlineData("missing", "A valid 'ApiKey' header is required.")]
    [InlineData("secret-header-instead", "A 'ApiSecret' header was supplied when a 'ApiKey' header should have been supplied.")]
    public async Task ApiPublicGivesHelpfulAdviceAsync(string input, string details)
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
{

    TestOutput = testOutput
});
        using var client = api.CreateClient().AddAcceptApplicationJson();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/signin/begin");

        if (input == "secret-header-instead")
        {
            request.Headers.Add("ApiSecret", "something");
        }
        else if (input != "missing")
        {
            request.Headers.Add("ApiKey", input);
        }

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        AssertHelper.AssertEqualJson($$"""
        {
            "type": "https://docs.passwordless.dev/guide/errors.html#ApiKey",
            "title": "A valid 'ApiKey' header is required.",
            "status": 401,
            "detail": "{{details}}"
        }
        """, body);
    }

    private static string? CreateRoute(RoutePattern pattern)
    {
        // TODO: We may have to get smarter with generating this route
        return pattern.RawText;
    }
}