using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests;

[Collection("Public Sign In")]
public class PublicSignInTests : PublicTests
{

    public PublicSignInTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task UnknownCredentialThrows()
    {
        var options = await PostAsync("/signin/begin", new { Origin = "https://localhost", RPID = "localhost" });

        var response = await options.Content.ReadFromJsonAsync<SessionResponse<AssertionOptions>>();

        var payload = new
        {
            Origin = "https://localhost",
            RPID = "localhost",
            Session = response.Session,
            Response = new
            {
                Id = "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
                RawId = "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
                Response = new
                {
                    AuthenticatorData = "8egiesVpgMwlnLcY0N3ldtvrzKGUPb763GkSYC4CzTkFAAAAAA",
                    ClientDataJson =
                        "eyJ0eXBlIjoid2ViYXV0aG4uZ2V0IiwiY2hhbGxlbmdlIjoiYmhPUllmRlR5S1hfdEtDQkFQbVVKdyIsIm9yaWdpbiI6Imh0dHBzOi8vYWRtaW4ucGFzc3dvcmRsZXNzLmRldiIsImNyb3NzT3JpZ2luIjpmYWxzZX0",
                    Signature =
                        "MEUCIQDiTSGMfKb_qZQDB0J8KFFZeAOrvCZEF2yi6MgoUNwkTgIgHYfe8LKPg-INMK9NxJfPaCdNsRUtP2DMhDKraJAOvzk"
                },
                type = "public-key"
            }
        };

        var result = await PostAsync("/signin/complete", payload);
        var body = await result.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        AssertHelper.AssertEqualJson("""
        {
          "type": "https://docs.passwordless.dev/guide/errors.html#unknown_credential",
          "title": "We don't recognize the passkey you sent us.",
          "status": 400,          
          "credentialId": "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
          "errorCode": "unknown_credential"
        }
        """, body);

    }

    [Fact]
    public async Task CredentialsList()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = _factory.CreateDbContext(scope, "test");

        context.Credentials.Add(new EFStoredCredential
        {
            DescriptorId = "test"u8.ToArray(),
            DescriptorType = PublicKeyCredentialType.PublicKey,
            UserHandle = "1"u8.ToArray(),
            Tenant = "test",
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/credentials/list?userId=1");
        request.Headers.Add("ApiSecret", _factory.ApiSecret);
        var httpResponse = await _client.SendAsync(request);
        using var credentialsResponse = await httpResponse.Content.ReadFromJsonAsync<JsonDocument>();
        // Assert
        Assert.NotNull(credentialsResponse);
        var credentials = credentialsResponse.RootElement.GetProperty("values").Deserialize<StoredCredential[]>();
        Assert.NotNull(credentials);
        Assert.Single(credentials);
    }
}