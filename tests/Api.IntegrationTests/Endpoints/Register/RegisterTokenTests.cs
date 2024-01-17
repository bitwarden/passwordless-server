using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Register;

public class RegisterTokenTests : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public RegisterTokenTests(ITestOutputHelper testOutput, PasswordlessApiFactory apiFactory)
    {
        apiFactory.TestOutput = testOutput;
        _client = apiFactory.CreateClient().AddSecretKey();
    }

    [Fact]
    public async Task UserIdAndDisplayNameIsTheOnlyRequiredProperties()
    {
        var payload = new { UserId = "1", Username = "test" };

        using var response = await _client.PostAsJsonAsync("register/token", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUserIdReturnsError(string userid)
    {
        object payload;
        if (userid == "-1")
        {
            payload = new { };
        }
        else
        {
            payload = new { UserId = userid };
        }

        using var response = await _client.PostAsJsonAsync("register/token", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """
            {
              "type": "https://docs.passwordless.dev/guide/errors.html#",
              "title": "Invalid UserId: UserId cannot be null or empty",
              "status": 400,
              "errorCode": null
            }
            """, body);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUsernameReturnsError(string input)
    {
        object payload;
        if (input == "-1")
        {
            payload = new { UserID = "1" };
        }
        else
        {
            payload = new { UserId = "1", Username = input };
        }

        using var response = await _client.PostAsJsonAsync("register/token", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """
            {
              "type": "https://docs.passwordless.dev/guide/errors.html#",
              "title": "Invalid Username: Username cannot be null or empty",
              "status": 400,
              "errorCode": null
            }
            """, body);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("other")]
    public async Task OtherAssertionIsNotAccepted(string attestation)
    {
        var payload = new { UserId = "1", Username = "test", attestation };

        using var response = await _client.PostAsJsonAsync("register/token", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """
             {
               "type": "https://docs.passwordless.dev/guide/errors.html#invalid_attestation",
               "title": "Attestation type not supported",
               "status": 400,
               "errorCode": "invalid_attestation"
             }
             """, body);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("")]
    [InlineData("None")]
    public async Task NoneAssertionIsAccepted(string attestation)
    {
        var payload = new { UserId = "1", Username = "test", attestation };

        using var response = await _client.PostAsJsonAsync("register/token", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}