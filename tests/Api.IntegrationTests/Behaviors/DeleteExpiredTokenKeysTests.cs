using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Behaviors;

public class DeleteExpiredTokenKeysTests : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly PasswordlessApiFactory _factory;
    private readonly HttpClient _client;

    public DeleteExpiredTokenKeysTests(PasswordlessApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient().AddSecretKey();
    }

    [Fact]
    public async Task An_expired_apps_token_keys_should_be_removed_when_a_request_is_made()
    {
        // Arrange
        var timeProvider = (FakeTimeProvider)_factory.Services.GetRequiredService<TimeProvider>();
        timeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2023, 1, 1)));
        
        var tokenGenerator = new Faker<RegisterToken>()
            .RuleFor(x => x.UserId, Guid.NewGuid().ToString())
            .RuleFor(x => x.DisplayName, x => x.Person.FullName)
            .RuleFor(x => x.Username, x => x.Person.Email)
            .RuleFor(x => x.Attestation, "None")
            .RuleFor(x => x.Discoverable, true)
            .RuleFor(x => x.UserVerification, "Preferred")
            .RuleFor(x => x.Aliases, x => new HashSet<string> { x.Person.FirstName })
            .RuleFor(x => x.AliasHashing, false)
            .RuleFor(x => x.ExpiresAt, DateTime.UtcNow.AddDays(1))
            .RuleFor(x => x.TokenId, Guid.Empty);

        using var scope = _factory.Services.CreateScope();

        var storage = scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>().Create("test");
        await storage.AddTokenKey(new TokenKey { Tenant = "test", KeyMaterial = "randomKeyMaterial", KeyId = 0, CreatedAt = new DateTime(2022, 1, 1) });
        await storage.AddTokenKey(new TokenKey { Tenant = "test", KeyMaterial = "randomKeyMaterial", KeyId = 1, CreatedAt = new DateTime(2022, 2, 1) });
        await storage.AddTokenKey(new TokenKey { Tenant = "test", KeyMaterial = "randomKeyMaterial", KeyId = 2, CreatedAt = DateTime.UtcNow });

        // Act
        _ = await _client.PostAsJsonAsync("register/token", tokenGenerator.Generate());

        // Assert
        var tokenKeys = await storage.GetTokenKeys();
        tokenKeys.Should().NotBeNull();
        tokenKeys.Any(x => x.CreatedAt < (DateTime.UtcNow.AddDays(-30))).Should().BeFalse();
        tokenKeys.Any(x => x.CreatedAt >= (DateTime.UtcNow.AddDays(-30))).Should().BeTrue();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}