using FluentAssertions;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Validation;

namespace Passwordless.Service.Tests.Validation;

public class TokenValidatorTests
{
    [Fact]
    public void ValidateAttestation_Throws_ApiException_WhenAttestationTypeIsEnterprise()
    {
        // arrange
        var token = new RegisterToken
        {
            UserId = "123",
            Attestation = "enterprise"
        };
        var features = new FeaturesContext(false, 0, null, null, true, false, false);

        // act
        var actual = Record.Exception(() => TokenValidator.ValidateAttestation(token, features));

        // assert
        Assert.IsType<ApiException>(actual);
        Assert.Equal("invalid_attestation", ((ApiException)actual).ErrorCode);
        Assert.Equal("Attestation type not supported", ((ApiException)actual).Message);
        Assert.Equal(400, ((ApiException)actual).StatusCode);
    }

    [Fact]
    public void ValidateAttestation_Throws_ApiException_WhenAttestationNotAllowedForPlan()
    {
        // arrange
        var token = new RegisterToken
        {
            UserId = "123",
            Attestation = "direct"
        };
        var features = new FeaturesContext(false, 0, null, null, false, false, false);

        // act
        var actual = Record.Exception(() => TokenValidator.ValidateAttestation(token, features));

        // assert
        Assert.IsType<ApiException>(actual);
        Assert.Equal("attestation_not_supported_on_plan", ((ApiException)actual).ErrorCode);
        Assert.Equal("Attestation type not supported on your plan", ((ApiException)actual).Message);
        Assert.Equal(400, ((ApiException)actual).StatusCode);
    }

    [Fact]
    public void Validate_GivenUtcNow_WhenTokenExpiredThirtySecondsAgo_ThenApiExceptionThrowsWithMessage()
    {
        // arrange
        var expiresAt = DateTimeOffset.UtcNow.UtcDateTime.AddSeconds(-30);
        var token = new Token { ExpiresAt = expiresAt, TokenId = Guid.NewGuid(), Type = "token_type" };

        // act
        var actual = Record.Exception(() => token.Validate(DateTimeOffset.UtcNow));

        // assert
        Assert.IsType<ApiException>(actual);
        Assert.Equal("expired_token", ((ApiException)actual).ErrorCode);
        Assert.Equal("The token expired 30 seconds ago.", ((ApiException)actual).Message);
        Assert.Equal(403, ((ApiException)actual).StatusCode);
    }

    [Fact]
    public void Validate_GivenUtcNow_WhenTokenExpiredNow_ThenShouldNotThrow()
    {
        // arrange
        var now = DateTimeOffset.UtcNow;
        var token = new Token { ExpiresAt = now.UtcDateTime, TokenId = Guid.NewGuid(), Type = "token_type" };

        // act
        var actual = () => token.Validate(now);

        // assert
        actual.Should().NotThrow();
    }

    [Fact]
    public void Validate_GivenUtcNow_WhenTokenIsNotExpired_ThenShouldNotThrow()
    {
        // arrange
        var now = DateTimeOffset.UtcNow;
        var token = new Token { ExpiresAt = now.AddDays(4).UtcDateTime, TokenId = Guid.NewGuid(), Type = "token_type" };

        // act
        var actual = () => token.Validate(now);

        // assert
        actual.Should().NotThrow();
    }
}