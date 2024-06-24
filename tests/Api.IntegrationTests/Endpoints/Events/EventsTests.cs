using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Constants;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Events;

public class EventsTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_can_view_the_event_for_a_user_retrieving_the_api_keys()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        await client.EnableEventLogging(applicationName);
        _ = await client.GetAsync($"/admin/apps/{applicationName}/api-keys");

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeysEnumerated.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_a_user_creating_an_api_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        await client.EnableEventLogging(applicationName);
        using var createApiKeyResponse = await client.PostAsJsonAsync($"/admin/apps/{applicationName}/secret-keys",
            new CreateSecretKeyRequest([SecretKeyScopes.TokenRegister, SecretKeyScopes.TokenVerify]));
        var createApiKey = await createApiKeyResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        client.AddSecretKey(createApiKey!.ApiKey);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyCreated.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_locking_an_api_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        await client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToLock = apiKeys!.First(x => x.Type == ApiKeyTypes.Public);
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/lock", null);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyLocked.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_unlocking_an_api_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        await client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToLock = apiKeys!.First(x => x.Type == ApiKeyTypes.Public);
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/lock", null);
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/unlock", null);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyUnlocked.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_deleting_an_api_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        _ = await client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToDelete = apiKeys!.First(x => x.Type == ApiKeyTypes.Public);
        _ = await client.DeleteAsync($"/admin/apps/{applicationName}/api-keys/{keyToDelete.Id}");

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyDeleted.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_enabling_the_generate_sign_in_token_endpoint()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        const string user = "a_user";
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        _ = await client.EnableEventLogging(applicationName);
        _ = await client.EnableManuallyGenerateAccessTokenEndpoint("a_user");

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        var enabledEvent = applicationEvents.Events.FirstOrDefault(x =>
            x.EventType == EventType.AdminGenerateSignInTokenEndpointEnabled.ToString());
        enabledEvent.Should().NotBeNull();
        enabledEvent!.PerformedBy.Should().Be(user);
    }

    [Fact]
    public async Task I_can_view_the_event_for_disabling_the_generate_sign_in_token_endpoint()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        const string user = "a_user";
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        await client.EnableEventLogging(applicationName);
        await client.DisableManuallyGenerateAccessTokenEndpoint(user);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        var enabledEvent = applicationEvents.Events.FirstOrDefault(x =>
            x.EventType == EventType.AdminGenerateSignInTokenEndpointDisabled.ToString());
        enabledEvent.Should().NotBeNull();
        enabledEvent!.PerformedBy.Should().Be(user);
    }

    [Fact]
    public async Task I_can_view_the_event_for_enabling_magic_links()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        const string user = "a_user";
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        _ = await client.EnableEventLogging(applicationName);
        _ = await client.EnableMagicLinks("a_user");

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        var enabledEvent =
            applicationEvents.Events.FirstOrDefault(x => x.EventType == EventType.AdminMagicLinksEnabled.ToString());
        enabledEvent.Should().NotBeNull();
        enabledEvent!.PerformedBy.Should().Be(user);
    }

    [Fact]
    public async Task I_can_view_the_event_for_disabling_magic_links()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        const string user = "a_user";
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        _ = await client.EnableEventLogging(applicationName);
        _ = await client.DisableMagicLinks("a_user");

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        var enabledEvent =
            applicationEvents.Events.FirstOrDefault(x => x.EventType == EventType.AdminMagicLinksDisabled.ToString());
        enabledEvent.Should().NotBeNull();
        enabledEvent!.PerformedBy.Should().Be(user);
    }

    [Fact]
    public async Task I_can_view_the_event_for_using_a_disabled_api_secret()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        await client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToLock = apiKeys!.First(x => x.ApiKey.EndsWith(app.ApiSecret1.GetLast(4)));
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/lock", null);
        _ = await client.GetAsync("credentials/list");
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/unlock", null);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");
        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should()
            .Contain(x => x.EventType == EventType.ApiAuthDisabledSecretKeyUsed.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_using_a_disabled_public_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToLock = apiKeys!.First(x => x.ApiKey.EndsWith(app.ApiKey1.GetLast(4)));
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/lock", null);
        _ = await client.PostAsJsonAsync("/signin/begin",
            new SignInBeginDTO { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });
        _ = await client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/unlock", null);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should()
            .Contain(x => x.EventType == EventType.ApiAuthDisabledPublicKeyUsed.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_using_a_non_existent_api_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey($"{applicationName}:public:invalid-public-key");
        await client.EnableEventLogging(applicationName);
        _ = await client.PostAsJsonAsync("/signin/begin",
            new SignInBeginDTO { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.ApiAuthInvalidPublicKeyUsed.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_using_a_non_existent_api_secret()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddSecretKey($"{applicationName}:secret:invalid-secret-key");
        await client.EnableEventLogging(applicationName);
        _ = await client.GetAsync("credentials/list");
        client.AddSecretKey(app.ApiSecret1);

        // Act
        using var getApplicationEventsResponse = await client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents =
            await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.ApiAuthInvalidSecretKeyUsed.ToString());
    }
}