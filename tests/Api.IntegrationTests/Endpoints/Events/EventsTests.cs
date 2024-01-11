using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.IntegrationTests.Infra;
using Passwordless.Common.Constants;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Models.Apps;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Events;

public class EventsTests : IClassFixture<TestApiFixture>, IDisposable
{
    private readonly TestApi _testApi;

    public EventsTests(ITestOutputHelper testOutput, TestApiFixture testApiFixture)
    {
        _testApi = testApiFixture.CreateApi(testOutput: testOutput);
    }

    [Fact]
    public async Task I_can_view_the_event_for_a_user_retrieving_the_api_keys()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _testApi.Client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _testApi.Client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        await _testApi.Client.EnableEventLogging(applicationName);
        _ = await _testApi.Client.GetAsync($"/admin/apps/{applicationName}/api-keys");

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeysEnumerated.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_a_user_creating_an_api_key()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await _testApi.Client.CreateApplicationAsync(applicationName);
        await _testApi.Client.EnableEventLogging(applicationName);
        using var createApiKeyResponse = await _testApi.Client.PostAsJsonAsync($"/admin/apps/{applicationName}/secret-keys",
            new CreateSecretKeyRequest([SecretKeyScopes.TokenRegister, SecretKeyScopes.TokenVerify]));
        var createApiKey = await createApiKeyResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        _testApi.Client.AddSecretKey(createApiKey!.ApiKey);

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyCreated.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_locking_an_api_key()
    {
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _testApi.Client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _testApi.Client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        await _testApi.Client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await _testApi.Client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToLock = apiKeys!.First(x => x.Type == ApiKeyTypes.Public);
        _ = await _testApi.Client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/lock", null);

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyLocked.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_unlocking_an_api_key()
    {
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _testApi.Client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _testApi.Client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        await _testApi.Client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await _testApi.Client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToLock = apiKeys!.First(x => x.Type == ApiKeyTypes.Public);
        _ = await _testApi.Client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/lock", null);
        _ = await _testApi.Client.PostAsync($"/admin/apps/{applicationName}/api-keys/{keyToLock.Id}/unlock", null);

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyUnlocked.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_deleting_an_api_key()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _testApi.Client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _testApi.Client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _ = await _testApi.Client.EnableEventLogging(applicationName);
        using var getApiKeysResponse = await _testApi.Client.GetAsync($"/admin/apps/{applicationName}/api-keys");
        var apiKeys = await getApiKeysResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<ApiKeyResponse>>();
        var keyToDelete = apiKeys!.First(x => x.Type == ApiKeyTypes.Public);
        _ = await _testApi.Client.DeleteAsync($"/admin/apps/{applicationName}/api-keys/{keyToDelete.Id}");

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyDeleted.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_enabling_the_generate_sign_in_token_endpoint()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        const string user = "a_user";
        using var appCreationResponse = await _testApi.Client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _testApi.Client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _ = await _testApi.Client.EnableEventLogging(applicationName);
        using var enableResponse = await _testApi.Client.PostAsJsonAsync($"admin/apps/{applicationName}/sign-in-generate-token-endpoint/enable",
            new AppsEndpoints.EnableGenerateSignInTokenEndpointRequest(user));

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        var enabledEvent = applicationEvents.Events.FirstOrDefault(x => x.EventType == EventType.AdminGenerateSignInTokenEndpointEnabled.ToString());
        enabledEvent.Should().NotBeNull();
        enabledEvent!.PerformedBy.Should().Be(user);
    }

    [Fact]
    public async Task I_can_view_the_event_for_disabling_the_generate_sign_in_token_endpoint()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        const string user = "a_user";
        using var appCreationResponse = await _testApi.Client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _testApi.Client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _ = await _testApi.Client.EnableEventLogging(applicationName);
        using var enableResponse = await _testApi.Client.PostAsJsonAsync($"admin/apps/{applicationName}/sign-in-generate-token-endpoint/disable",
            new AppsEndpoints.DisableGenerateSignInTokenEndpointRequest(user));

        // Act
        using var getApplicationEventsResponse = await _testApi.Client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        var enabledEvent = applicationEvents.Events.FirstOrDefault(x => x.EventType == EventType.AdminGenerateSignInTokenEndpointDisabled.ToString());
        enabledEvent.Should().NotBeNull();
        enabledEvent!.PerformedBy.Should().Be(user);
    }

    public void Dispose()
    {
        _testApi.Dispose();
    }
}