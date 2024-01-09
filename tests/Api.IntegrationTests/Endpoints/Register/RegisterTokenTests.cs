using System.Net;
using System.Net.Http.Json;
using Passwordless.Api.IntegrationTests.Helpers;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Endpoints.Register;

public class RegisterTokenTests : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public RegisterTokenTests(PasswordlessApiFactory factory)
    {
        _client = factory.CreateClient().AddSecretKey();
    }

    [Fact]
    public async Task UserIdAndDisplayNameIsTheOnlyRequiredProperties()
    {
        var payload = new { UserId = "1", Username = "test" };

        var httpResponse = await _client.PostAsJsonAsync("register/token", payload);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
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

        var httpResponse = await _client.PostAsJsonAsync("register/token", payload);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

        var body = await httpResponse.Content.ReadAsStringAsync();

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

        var httpResponse = await _client.PostAsJsonAsync("register/token", payload);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

        var body = await httpResponse.Content.ReadAsStringAsync();

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

        var httpResponse = await _client.PostAsJsonAsync("register/token", payload);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        var body = await httpResponse.Content.ReadAsStringAsync();

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

        var httpResponse = await _client.PostAsJsonAsync("register/token", payload);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}