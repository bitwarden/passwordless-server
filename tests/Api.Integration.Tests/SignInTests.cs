using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests;

public class SignInTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _httpClient;

    public SignInTests(PasswordlessApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Server_returns_token_to_be_used_for_sign_in()
    {
        var request = new SignInBeginDTO
        {
            Origin = "http://integration-tests.passwordless.dev",
            RPID = Environment.MachineName,
        };
        _httpClient.DefaultRequestHeaders.Add("ApiKey","test:public:2e728aa5986f4ba8b073a5b28a939795");
        
        var response = await _httpClient.PostAsJsonAsync("/signin/begin", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInResponse = await response.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        signInResponse.Should().NotBeNull();
        signInResponse!.Session.Should().StartWith("session_");
        signInResponse.Data.RpId.Should().Be(request.RPID);
    }
    
    
}