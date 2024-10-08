using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using Fido2NetLib.Objects;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.IntegrationTests.Helpers.User;
using Passwordless.Common.Models.Apps;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.AuthenticationConfiguration;

[Collection(ApiCollectionFixture.Fixture)]
public class AuthenticationConfigurationTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
{
    [Fact]
    public async Task I_can_create_an_authentication_configuration()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

        var request = RequestHelpers.GetSetAuthenticationConfigurationRequest().Generate();

        // Act
        using var response = await client.PostAsJsonAsync("auth-configs/add", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>(
                $"auth-configs/list?purpose={request.Purpose}");
        getConfigResponse.Should().NotBeNull();
        getConfigResponse!.Configurations.Should().Contain(x => x.Purpose == request.Purpose);
        var config = getConfigResponse.Configurations.First(x => x.Purpose == request.Purpose);
        config.Purpose.Should().Be(request.Purpose);
        config.UserVerificationRequirement.Should().Be(request.UserVerificationRequirement.ToEnumMemberValue());
        config.TimeToLive.Should().BeCloseTo((int)request.TimeToLive.TotalSeconds, 1);
        config.Hints.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Enum.Parse<PublicKeyCredentialHint>(x, true)).Should()
            .BeEquivalentTo(request.Hints);
        config.CreatedBy.Should().Be(request.PerformedBy);
        config.CreatedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task I_can_get_the_default_sign_in_authentication_configuration_without_changing_anything()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

        // Act
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>(
                $"auth-configs/list?purpose={SignInPurpose.SignInName}");

        // Assert
        getConfigResponse.Should().NotBeNull();
        var signIn = getConfigResponse!.Configurations.FirstOrDefault(x => x.Purpose == SignInPurpose.SignInName);
        signIn.Should().NotBeNull();
        signIn!.Should().BeEquivalentTo(AuthenticationConfigurationDto.SignIn(applicationName).ToResponse());
    }

    [Fact]
    public async Task I_can_get_the_default_step_up_authentication_configuration_without_changing_anything()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

        // Act
        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>(
                $"auth-configs/list?purpose={SignInPurpose.StepUpName}");

        // Assert
        getConfigResponse.Should().NotBeNull();
        var stepUp = getConfigResponse!.Configurations.FirstOrDefault(x => x.Purpose == SignInPurpose.StepUpName);
        stepUp.Should().NotBeNull();
        stepUp!.Should().BeEquivalentTo(AuthenticationConfigurationDto.StepUp(applicationName).ToResponse());
    }

    [Fact]
    public async Task I_can_should_get_an_empty_list_for_a_non_preset_and_nonexistent_configuration()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

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
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

        var request = RequestHelpers.GetSetAuthenticationConfigurationRequest().Generate();
        using var response = await client.PostAsJsonAsync("auth-configs/add", request);

        // Act
        using var deleteResponse = await client.PostAsJsonAsync("auth-configs/delete",
            new DeleteAuthenticationConfigurationRequest
            {
                Purpose = request.Purpose,
                PerformedBy = new Faker().Person.UserName
            });

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_can_edit_a_configuration_I_created()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

        var request = RequestHelpers.GetSetAuthenticationConfigurationRequest().Generate();

        using var response = await client.PostAsJsonAsync("auth-configs/add", request);

        request.PerformedBy = new Faker().Person.UserName;
        request.UserVerificationRequirement = UserVerificationRequirement.Discouraged;
        request.TimeToLive = TimeSpan.FromSeconds(120);

        // Act
        using var editResponse = await client.PostAsJsonAsync("auth-configs", request);

        // Assert
        editResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getConfigResponse =
            await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>("auth-configs/list");
        getConfigResponse.Should().NotBeNull();
        getConfigResponse!.Configurations.Should().Contain(x => x.Purpose == request.Purpose);

        var editedConfig = getConfigResponse.Configurations.First(x => x.Purpose == request.Purpose);
        editedConfig.UserVerificationRequirement.Should()
            .Be(UserVerificationRequirement.Discouraged.ToEnumMemberValue());
        editedConfig.TimeToLive.Should().BeCloseTo((int)request.TimeToLive.TotalSeconds, 1);
        editedConfig.Hints.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Enum.Parse<PublicKeyCredentialHint>(x, true)).Should()
            .BeEquivalentTo(request.Hints);
        editedConfig.EditedBy.Should().Be(request.PerformedBy);
        editedConfig.EditedOn.Should().NotBeNull();
    }

    [Theory]
    [InlineData(SignInPurpose.SignInName)]
    [InlineData(SignInPurpose.StepUpName)]
    public async Task I_can_edit_a_preset_configuration(string presetPurpose)
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client.AddSecretKey(appCreationResponse.ApiSecret1);

        var request = RequestHelpers.GetSetAuthenticationConfigurationRequest()
            .RuleFor(x => x.Purpose, presetPurpose)
            .Generate();

        // Act
        using var editResponse = await client.PostAsJsonAsync("auth-configs", request);

        // Assert
        editResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getConfigResponse =
            await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>("auth-configs/list");
        getConfigResponse.Should().NotBeNull();
        getConfigResponse!.Configurations.Should().Contain(x => x.Purpose == request.Purpose);

        var editedConfig = getConfigResponse.Configurations.First(x => x.Purpose == request.Purpose);
        editedConfig.UserVerificationRequirement.Should().Be(request.UserVerificationRequirement.ToEnumMemberValue());
        editedConfig.TimeToLive.Should().BeCloseTo((int)request.TimeToLive.TotalSeconds, 1);
        editedConfig.Hints.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Enum.Parse<PublicKeyCredentialHint>(x, true)).Should()
            .BeEquivalentTo(request.Hints);
        editedConfig.EditedBy.Should().Be(request.PerformedBy);
        editedConfig.EditedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task I_can_sign_in_and_the_authentication_configuration_will_have_its_last_used_on_updated()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });

        using var client = api.CreateClient().AddManagementKey();
        var applicationName = CreateAppHelpers.GetApplicationName();

        var appCreationResponse = await client.CreateApplicationAsync(applicationName);
        _ = client
            .AddSecretKey(appCreationResponse.ApiSecret1)
            .AddPublicKey(appCreationResponse.ApiKey1)
            .AddUserAgent();

        using var driver = WebDriverFactory.GetDriver(PasswordlessApi.OriginUrl);
        await client.RegisterNewUserAsync(driver);

        // Act
        using var signInResponse = await client.SignInUser(driver);

        // Assert
        signInResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getConfigResponse = await client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>($"auth-configs/list?purpose={SignInPurpose.SignInName}");
        getConfigResponse.Should().NotBeNull();
        getConfigResponse!.Configurations.Should().Contain(x => x.Purpose == SignInPurpose.SignInName);
        var config = getConfigResponse.Configurations.First(x => x.Purpose == SignInPurpose.SignInName);
        config.LastUsedOn.Should().NotBeNull();
    }
}