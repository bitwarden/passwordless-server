using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Validation;

namespace Passwordless.Service.Tests.Validation;

public class RegisterTokenValidatorTests
{
    [Fact]
    public void ValidateAttestation_Throws_ApiException_WhenAttestationTypeIsEnterprise()
    {
        // arrange
        var token = new RegisterToken
        {
            Attestation = "enterprise",
        };
        var features = new FeaturesContext(false, 0, null, null, true, false);

        // act
        var actual = Record.Exception(() => RegisterTokenValidator.ValidateAttestation(token, features));

        // assert
        Assert.IsType<ApiException>(actual);
        Assert.Equal("invalid_attestation", ((ApiException)actual).ErrorCode);
        Assert.Equal("Attestation type not supported.", ((ApiException)actual).Message);
        Assert.Equal(400, ((ApiException)actual).StatusCode);
    }

    [Fact]
    public void ValidateAttestation_Throws_ApiException_WhenAttestationNotAllowedForPlan()
    {
        // arrange
        var token = new RegisterToken
        {
            Attestation = "direct",
        };
        var features = new FeaturesContext(false, 0, null, null, false, false);

        // act
        var actual = Record.Exception(() => RegisterTokenValidator.ValidateAttestation(token, features));

        // assert
        Assert.IsType<ApiException>(actual);
        Assert.Equal("invalid_attestation", ((ApiException)actual).ErrorCode);
        Assert.Equal("Attestation type not supported on your plan.", ((ApiException)actual).Message);
        Assert.Equal(400, ((ApiException)actual).StatusCode);
    }
}