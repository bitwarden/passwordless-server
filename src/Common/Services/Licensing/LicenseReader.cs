using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Common.Services.Licensing.Constants;
using Passwordless.Common.Services.Licensing.Cryptography;
using Passwordless.Common.Services.Licensing.Exceptions;
using Passwordless.Common.Services.Licensing.Models;
using Passwordless.Common.Services.Licensing.Serializers;

namespace Passwordless.Common.Services.Licensing;

public class LicenseReader : ILicenseReader
{
    private readonly ILicenseSerializer _serializer;
    private readonly ICryptographyProvider _signatureProvider;
    private readonly ILogger<LicenseReader> _logger;

    public LicenseReader(
        ILicenseSerializer serializer,
        ICryptographyProvider signatureProvider,
        ILogger<LicenseReader> logger)
    {
        _serializer = serializer;
        _signatureProvider = signatureProvider;
        _logger = logger;
    }

    public async Task<LicenseData> ValidateAsync(string jwt)
    {
        var publicKey = new RsaSecurityKey(_signatureProvider.PublicKey);
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = publicKey,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationResult = await tokenHandler.ValidateTokenAsync(jwt, validationParameters);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("The license is invalid.");
            throw new InvalidLicenseException(jwt);
        }

        var claims = validationResult.Claims;

        var jsonElementData = (JsonElement)claims[CustomClaimTypes.Data];
        var data = jsonElementData.Deserialize<LicenseData>(_serializer.Options);
        if (data == null)
        {
            _logger.LogWarning("The license data is empty.");
            throw new InvalidLicenseException(jwt, "The license data is empty.");
        }
        data.ExpiresAt = validationResult.SecurityToken.ValidTo;
        return data;
    }
}