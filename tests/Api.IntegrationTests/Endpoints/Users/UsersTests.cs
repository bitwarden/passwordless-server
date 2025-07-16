using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.IntegrationTests.Helpers.User;
using Passwordless.Api.Models;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Users;

[Collection(ApiCollectionFixture.Fixture)]
public class UsersTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
{

    [Fact]
    public async Task I_can_retrieve_the_list_of_users_for_my_app()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {
            TestOutput = testOutput
        });

        using var client = api.CreateClient().AddUserAgent();
        var app = await client.CreateApplicationAsync();
        client.AddPublicKey(app.ApiKey1).AddSecretKey(app.ApiSecret1);

        using var driver = WebDriverFactory.GetDriver(PasswordlessApi.OriginUrl);
        var userRegistration = RequestHelpers.GetRegisterTokenGeneratorRules().Generate();
        _ = await client.RegisterNewUserAsync(driver, userRegistration);

        // Act
        using var userListResponse = await client.GetAsync("/users/list");

        // Assert
        userListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userList = await userListResponse.Content.ReadFromJsonAsync<ListResponse<UserSummary>>();
        userList.Values.Should().HaveCount(1);
        var user = userList.Values.First();
        user.Aliases.Should().Contain(userRegistration.Aliases.First());
    }
}