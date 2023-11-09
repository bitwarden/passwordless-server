using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests;

public class SignInTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _httpClient;


    const string OriginUrl = "https://bitwarden.com/products/passwordless/";
    const string RpId = "bitwarden.com";

    public SignInTests(PasswordlessApiFactory factory)
    {
        _httpClient = factory.CreateClient().AddPublicKey();
    }

    [Fact]
    public async Task Server_returns_encoded_assertion_options_to_be_used_for_sign_in()
    {
        var request = new SignInBeginDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
        };

        var response = await _httpClient.PostAsJsonAsync("/signin/begin", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInResponse = await response.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        signInResponse.Should().NotBeNull();
        signInResponse!.Session.Should().StartWith("session_");
        signInResponse.Data.RpId.Should().Be(request.RPID);
        signInResponse.Data.Status.Should().Be("ok");
    }
}