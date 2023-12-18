using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Common.Services.Licensing.Constants;
using Passwordless.Common.Services.Licensing.Interpreters;
using Passwordless.Common.Services.Licensing.Models;
using Passwordless.Common.Services.Licensing.Serializers;

namespace Passwordless.Common.Services.Licensing;

public sealed class LicenseWriter(
    ILicenseInterpreterFactory interpreterFactory,
    ILicenseSerializer serializer,
    ISignatureProvider signatureProvider,
    TimeProvider timeProvider)
    : ILicenseWriter
{
    public async Task<SecurityToken> WriteAsync(LicenseParameters parameters)
    {
        // Load the private key from the certificate
        X509Certificate2 certificate = signatureProvider.PrivateKey;
        
        var interpreter = interpreterFactory.Create(parameters);
        var data = await interpreter.GenerateAsync(parameters);
        var serializedData = serializer.Serialize(data);

        // Create a claims identity with your claims
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(new Claim[]
        {
            new (CustomClaimTypes.Data, serializedData, JsonClaimValueTypes.Json)
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