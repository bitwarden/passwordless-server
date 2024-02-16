using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
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

        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUserIdReturnsError(string? userid)
    {
        // Arrange
        object payload = userid == "-1"
            ? new { }
            : new { UserId = userid };

        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        AssertHelper.AssertEqualJson(
            // lang=json
            """
            {
              "type": "https://docs.passwordless.dev/guide/errors.html#",
              "title": "Invalid UserId: UserId cannot be null or empty",
              "status": 400,
              "errorCode": null
            }
            """, body);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidUsernameReturnsError(string? input)
    {
        // Arrange
        object payload = input == "-1"
            ? new { UserID = "1" }
            : new { UserId = "1", Username = input };

        await using var api = await apiFixture.CreateApiAsync(testOutput);
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
              "type": "https://docs.passwordless.dev/guide/errors.html#",
              "title": "Invalid Username: Username cannot be null or empty",
              "status": 400,
              "errorCode": null
            }
            """, body);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("other")]
    public async Task OtherAssertionIsNotAccepted(string attestation)
    {
        var payload = new { UserId = "1", Username = "test", attestation };

        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
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
        var payload = new { UserId = "1", Username = "test", attestation };

        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddSecretKey();

        // Act
        using var response = await client.PostAsJsonAsync("register/token", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}