using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Register;

public class RegisterTokenTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{

    [Fact]
    public async Task UserIdAndDisplayNameIsTheOnlyRequiredProperties()
    {
        // Arrange
        var payload = new { UserId = "1", Username = "test" };

        await using var api = await apiFixture.CreateApiAsync(testOutput: testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUserIdReturnsError(string userid)
    {
        // Arrange
        Faker<RegisterToken> registerTokenGenerator = new Faker<RegisterToken>();
        registerTokenGenerator.RuleFor(x => x.Username, "username");
        registerTokenGenerator.RuleFor(x => x.UserId, userid);
        var registerToken = registerTokenGenerator.Generate();

        await using var api = await apiFixture.CreateApiAsync(testOutput: testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", registerToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"userId":["The UserId field is required."]}}""", body);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUsernameReturnsError(string input)
    {
        // Arrange
        Faker<RegisterToken> registerTokenGenerator = new Faker<RegisterToken>();
        registerTokenGenerator.RuleFor(x => x.UserId, "userId");
        registerTokenGenerator.RuleFor(x => x.Username, input);
        var registerToken = registerTokenGenerator.Generate();

        await using var api = await apiFixture.CreateApiAsync(testOutput: testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", registerToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"username":["The Username field is required."]}}""", body);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("other")]
    public async Task OtherAssertionIsNotAccepted(string attestation)
    {
        var payload = new { UserId = "1", Username = "test", attestation };

        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput: testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """
             {
               "type": "https://docs.passwordless.dev/guide/errors.html#invalid_attestation",
               "title": "Attestation type not supported",
               "status": 400,
               "errorCode": "invalid_attestation"
             }
             """, body);
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

        await using var api = await apiFixture.CreateApiAsync(testOutput: testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}