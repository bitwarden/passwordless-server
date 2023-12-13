using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Common.Services.Licensing.Constants;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing;

[ManifestVersion(1)]
public sealed class LicenseWriter(
    ISignatureProvider signatureProvider,
    TimeProvider timeProvider)
    : ILicenseWriter
{
    public async Task<SecurityToken> WriteAsync(LicenseParameters parameters)
    {
        // Load the private key from the certificate
        X509Certificate2 certificate = signatureProvider.PrivateKey;

        // Create a claims identity with your claims
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(new Claim[]
        {
            new (ClaimTypes.NameIdentifier, parameters.InstallationId.ToString(), ClaimValueTypes.String),
            new (CustomClaimTypes.ManifestVersion, parameters.ManifestVersion.ToString(), ClaimValueTypes.UInteger32),
            new (CustomClaimTypes.ManifestVersion, parameters.ManifestVersion.ToString(), JsonClaimValueTypes.Json)

            // Add more claims as needed
        });

        // Create a security token descriptor
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            IssuedAt = timeProvider.GetUtcNow().UtcDateTime,
            Expires = parameters.ExpiresAt,
            SigningCredentials = new X509SigningCredentials(certificate)
        };

        // Create a JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Generate the JWT token
        return tokenHandler.CreateToken(securityTokenDescriptor);

        // Convert the token to a string
        //var tokenString = tokenHandler.WriteToken(token);
    }
}