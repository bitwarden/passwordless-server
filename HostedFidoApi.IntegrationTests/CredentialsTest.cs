using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace HostedFidoApi.IntegrationTests;

[Collection("Credentials")]
public class CredentialsTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private TestWebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public CredentialsTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async void CredentialsList()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var factory = _factory.Services.GetService<IDbTenantContextFactory>();
            var storage = factory?.CreateNewTenant("test");
            var context = factory.GetDbContext("test");

            context.ApiKeys.Add(new Service.Storage.ApiKeyDesc() {
                Id = "cdb5",
                ApiKey = "Ojo30xHbjABSZ65iQfpecA==:jmqnuC9B/tSTAr+B9Ow+fA==",
                Scopes = new [] {"token_register", "token_verify"},
                IsLocked = false
            });
            
            context.Credentials.Add(new EFStoredCredential() {
                DescriptorId = "test"u8.ToArray(),
                UserHandle = "2"u8.ToArray()
            });

            context.SaveChanges();

            _client.DefaultRequestHeaders.Add("ApiSecret", "test:secret:d1a64120edda4006ba8558b02d43cdb5");
            var res = await _client.GetAsync("/credentials/list?userId=1");

            var data = await res.Content.ReadFromJsonAsync<Service.Models.StoredCredential[]>();

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            Assert.NotNull(data);
            Assert.Empty(data);
        }

        
    }
}