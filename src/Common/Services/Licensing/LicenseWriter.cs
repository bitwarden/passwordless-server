using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Common.Services.Licensing.Constants;
using Passwordless.Common.Services.Licensing.Cryptography;
using Passwordless.Common.Services.Licensing.Interpreters;
using Passwordless.Common.Services.Licensing.Models;
using Passwordless.Common.Services.Licensing.Serializers;

namespace Passwordless.Common.Services.Licensing;

public sealed class LicenseWriter(
    ILicenseInterpreterFactory interpreterFactory,
    ILicenseSerializer serializer,
    ICryptographyProvider signatureProvider,
    TimeProvider timeProvider)
    : ILicenseWriter
{
    public JwtSecurityToken Write(LicenseParameters parameters)
    {
        var interpreter = interpreterFactory.Create(parameters);
        var data = interpreter.Generate(parameters);
        var serializedData = serializer.Serialize(data);

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(new Claim[]
        {
            new (CustomClaimTypes.Data, serializedData, JsonClaimValueTypes.Json)
        });

        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(signatureProvider.PrivateKey),
            SecurityAlgorithms.RsaSha256);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            IssuedAt = timeProvider.GetUtcNow().UtcDateTime,
            Expires = parameters.ExpiresAt,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);

        return securityToken;

    }
}