using System.Net.Http.Headers;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace HostedFidoApi.IntegrationTests;

[Collection("AccountTestes")]
public class AccountTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private TestWebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public AccountTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("1")]
    public async void CreateAccountWithInvalidName(string name)
    {
        var item = $$"""
                        {
                "AccountName": "{{name}}",
                "AdminEmail": "anders@passwordless.dev" 
            }
            """;
        var header = new MediaTypeHeaderValue("application/json");


        var res = await _client.PostAsync("/account/create", new StringContent(item, header));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateAccountWithValidName()
    {
        var name = "anders";
        var item = $$"""
                        {
                "AccountName": "{{name}}",
                "AdminEmail": "anders@passwordless.dev" 
            }
            """;
        var header = new MediaTypeHeaderValue("application/json");


        var res = await _client.PostAsync("/account/create", new StringContent(item, header));
        using (var scope = _factory.Services.CreateScope())
        {
            var factory = _factory.Services.GetService<IDbTenantContextFactory>();
            var storage = factory.GetExistingTenant(name);
            var info = await storage.GetAccountInformation();

        }
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}