using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Common.Services.Licensing.Constants;
using Passwordless.Common.Services.Licensing.Exceptions;
using Passwordless.Common.Services.Licensing.Models;
using Passwordless.Common.Services.Licensing.Serializers;

namespace Passwordless.Common.Services.Licensing;

public class LicenseReader(
    ILicenseSerializer serializer,
    ISignatureProvider signatureProvider,
    ILogger<LicenseReader> logger) : ILicenseReader
{
    public async Task<LicenseData> ValidateAsync(string jwt)
    {
        // Load the private key from the certificate
        X509Certificate2 certificate = signatureProvider.PublicKey;
        
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new X509SecurityKey(certificate),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        
        var validationResult = await tokenHandler.ValidateTokenAsync(jwt, validationParameters);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("The license is invalid.");
            throw new InvalidLicenseException(jwt);
        }

        var claims = validationResult.Claims;
        
        var serializedData = (string)claims[CustomClaimTypes.Data];
        var data = serializer.Deserialize<LicenseData>(serializedData);

        if (data == null)
        {
            logger.LogWarning("The license data is empty.");
            throw new InvalidLicenseException(jwt, "The license data is empty.");
        }
        
        return data;
    }
}