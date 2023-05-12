using System.Net;
using Microsoft.AspNetCore.Http;
using Passwordless.IntegrationTests.Helpers;

namespace Passwordless.Api.IntegrationTests;

public class RegisterTokenTests : BasicTests
{
    public RegisterTokenTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }
    
    [Theory]
    [InlineData("direct")]
    [InlineData("indirect")]
    [InlineData("other")]
    public async Task OtherAssertionIsNotAccepted(string attestation)
    {
        var payload = new { UserId = "1", Username = "test", attestation};
        
        var httpResponse = await PostAsync("/register/token", payload);
        
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        var body = await httpResponse.Content.ReadAsStringAsync();
        
        AssertHelper.AssertEqualJson("""
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
        var payload = new { UserId = "1", Username = "test", attestation};
        
        var httpResponse = await PostAsync("/register/token", payload);
        
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}