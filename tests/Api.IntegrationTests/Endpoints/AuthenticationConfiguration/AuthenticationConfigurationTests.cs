using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.AuthenticationConfiguration;

public class AuthenticationConfigurationTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture) : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_can_create_an_authentication_configuration()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        using var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        var keysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = client.AddSecretKey(keysCreation!.ApiSecret1);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "auth-configs/add");
        request.Content = new StringContent(
            // lang=json
            """
            {
              "timeToLive": "1.01:01:01",
              "purpose": "purpose",
              "userVerificationRequirement": "Discouraged"
            }
            """,
            Encoding.UTF8,
            MediaTypeNames.Application.Json
        );

        using var enableResponse = await client.SendAsync(request);

        // Assert
        enableResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>("auth-configs/list");
        getConfigResponse.Should().NotBeNull();
        getConfigResponse!.Configurations.Should().Contain(x => x.Purpose == "purpose1");
    }

    [Fact]
    public async Task I_can_get_the_default_sign_in_authentication_configuration_without_changing_anything()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        using var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        var keysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = client.AddSecretKey(keysCreation!.ApiSecret1);


        // Act
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>($"auth-configs/list?purpose={SignInPurposes.SignInName}");

        // Assert
        getConfigResponse.Should().NotBeNull();
        var signIn = getConfigResponse!.Configurations.FirstOrDefault(x => x.Purpose == SignInPurposes.SignInName);
        signIn.Should().NotBeNull();
        signIn!.Should().BeEquivalentTo(AuthenticationConfigurationDto.SignIn(applicationName).ToResponse());
    }

    [Fact]
    public async Task I_can_get_the_default_step_up_authentication_configuration_without_changing_anything()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        using var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        var keysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = client.AddSecretKey(keysCreation!.ApiSecret1);

        // Act
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>($"auth-configs/list?purpose={SignInPurposes.StepUpName}");

        // Assert
        getConfigResponse.Should().NotBeNull();
        var stepUp = getConfigResponse!.Configurations.FirstOrDefault(x => x.Purpose == SignInPurposes.StepUpName);
        stepUp.Should().NotBeNull();
        stepUp!.Should().BeEquivalentTo(AuthenticationConfigurationDto.StepUp(applicationName).ToResponse());
    }

    [Fact]
    public async Task I_can_should_get_an_empty_list_for_a_non_preset_and_nonexistent_configuration()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        using var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        var keysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = client.AddSecretKey(keysCreation!.ApiSecret1);

        // Act
        var getConfigResponse = await client.GetAsync($"auth-configs/list?purpose=random");

        // Assert
        getConfigResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getConfigResponse.Content.ReadFromJsonAsync<GetAuthenticationConfigurationsResult>();
        content!.Configurations.Should().BeEmpty();
    }

    [Fact]
    public async Task I_can_delete_a_configuration_I_created()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        using var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        var keysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = client.AddSecretKey(keysCreation!.ApiSecret1);

        const string purpose = "purpose1";

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "auth-configs/add");
        createRequest.Content = new StringContent(
            // lang=json
            $$"""
              {
                "timeToLive": "1.01:01:01",
                "purpose": "{{purpose}}",
                "userVerificationRequirement": "Discouraged"
              }
              """,
            Encoding.UTF8,
            MediaTypeNames.Application.Json
        );
        await client.SendAsync(createRequest);

        // Act
        using var deleteResponse = await client.PostAsJsonAsync("auth-configs/delete", new DeleteAuthenticationConfigurationRequest
        {
            Purpose = purpose
        });

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_can_edit_a_configuration_I_created()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        using var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        var keysCreation = await appCreationResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = client.AddSecretKey(keysCreation!.ApiSecret1);

        const string purpose = "purpose1";
        const string timeToLiveString = "1.01:01:01";
        var timeToLive = TimeSpan.Parse(timeToLiveString);
        const string uvString = "Discouraged";

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "auth-configs/add");
        createRequest.Content = new StringContent(
            // lang=json
            $$"""
              {
                "timeToLive": "{{timeToLiveString}}",
                "purpose": "{{purpose}}",
                "userVerificationRequirement": "{{uvString}}"
              }
              """,
            Encoding.UTF8,
            MediaTypeNames.Application.Json
        );
        await client.SendAsync(createRequest);

        // Act
        using var editResponse = await client.PostAsJsonAsync("auth-configs",
            new SetAuthenticationConfigurationRequest
            {
                Purpose = purpose,
                TimeToLive = timeToLive.Add(TimeSpan.FromDays(1)),
                UserVerificationRequirement = UserVerificationRequirement.Preferred
            });

        // Assert
        editResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>("auth-configs");
        getConfigResponse.Should().NotBeNull();
        getConfigResponse!.Configurations.Should().Contain(x => x.Purpose == purpose);
        var createdPurpose = getConfigResponse.Configurations.First(x => x.Purpose == purpose);
        createdPurpose.TimeToLive.Should().Be((int)timeToLive.Add(TimeSpan.FromDays(1)).TotalSeconds);
        createdPurpose.UserVerificationRequirement.Should().Be(UserVerificationRequirement.Preferred.ToEnumMemberValue());
    }
}