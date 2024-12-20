using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Register;

[Collection(ApiCollectionFixture.Fixture)]
public class RegisterTokenTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
{
    [Fact]
    public async Task UserIdAndDisplayNameIsTheOnlyRequiredProperties()
    {
        // Arrange
        var payload = new { UserId = "1", Username = "test" };

        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });

        using var client = api.CreateClient();
        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUserIdReturnsError(string? userid)
    {
        // Arrange
        Faker<RegisterToken> registerTokenGenerator = new Faker<RegisterToken>();
        registerTokenGenerator.RuleFor(x => x.Username, "username");
        registerTokenGenerator.RuleFor(x => x.UserId, userid);
        var registerToken = registerTokenGenerator.Generate();

        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {
            TestOutput = testOutput
        });

        using var client = api.CreateClient();
        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);

        // Act
        using var response = await client.PostAsJsonAsync("register/token", registerToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var actual = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(actual);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", actual!.Type);
        Assert.Equal("One or more validation errors occurred.", actual.Title);
        Assert.Equal(400, actual.Status);
        Assert.Equal("{\"userId\":[\"The UserId field is required.\"]}", actual.Extensions["errors"]!.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUsernameReturnsError(string? input)
    {
        // Arrange
        Faker<RegisterToken> registerTokenGenerator = new Faker<RegisterToken>();
        registerTokenGenerator.RuleFor(x => x.UserId, "userId");
        registerTokenGenerator.RuleFor(x => x.Username, input);
        var registerToken = registerTokenGenerator.Generate();

        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {
            TestOutput = testOutput
        });

        using var client = api.CreateClient();
        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);

        // Act
        using var response = await client.PostAsJsonAsync("register/token", registerToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var actual = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(actual);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", actual!.Type);
        Assert.Equal("One or more validation errors occurred.", actual.Title);
        Assert.Equal(400, actual.Status);
        Assert.Equal("{\"username\":[\"The Username field is required.\"]}", actual.Extensions["errors"]!.ToString());
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("other")]
    public async Task OtherAssertionIsNotAccepted(string attestation)
    {
        var payload = new { UserId = "1", Username = "test", attestation };

        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {
            TestOutput = testOutput
        });

        using var client = api.CreateClient();
        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var actual = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(actual);
        Assert.Equal("https://docs.passwordless.dev/guide/errors.html#invalid_attestation", actual!.Type);
        Assert.Equal("Attestation type not supported", actual.Title);
        Assert.Equal(400, actual.Status);
        Assert.Equal("invalid_attestation", actual.Extensions["errorCode"]!.ToString());
    }

    [Theory]
    [InlineData("none")]
    [InlineData("")]
    [InlineData("None")]
    public async Task NoneAssertionIsAccepted(string attestation)
    {
        // Arrange
        var payload = new RegisterToken
        {
            UserId = "1",
            Username = "test",
            Attestation = attestation
        };

        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {
            TestOutput = testOutput
        });

        using var client = api.CreateClient();
        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}