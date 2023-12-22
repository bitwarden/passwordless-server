using System.IdentityModel.Tokens.Jwt;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.Common.Services.Licensing;
using Passwordless.Common.Services.Licensing.Cryptography;
using Passwordless.Common.Services.Licensing.Exceptions;
using Passwordless.Common.Services.Licensing.Interpreters;
using Passwordless.Common.Services.Licensing.Models;
using Passwordless.Common.Services.Licensing.Serializers;

namespace Passwordless.Common.Tests.Services.Licensing;

public class LicenseReaderTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<TimeProvider> _timeProviderMock = new();

    private readonly LicenseReader _sut;

    private readonly LicenseWriter _licenseWriter;

    public LicenseReaderTests()
    {
        var interpreterFactoryLoggerMock = new Mock<ILogger<LicenseInterpreterFactory>>();
        var interpreterFactory = new LicenseInterpreterFactory(interpreterFactoryLoggerMock.Object);

        var serializer = new LicenseSerializer();

        var fileSignatureConfiguration = new FileCryptographyConfiguration
        {
            PrivateKey = "license_dev_private.pem",
            PublicKey = "license_dev_public.pem"
        };

        var optionsMock = new Mock<IOptions<FileCryptographyConfiguration>>();
        optionsMock.SetupGet(x => x.Value).Returns(fileSignatureConfiguration);
        var signatureProvider = new FileCryptographyProvider(optionsMock.Object);

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        _licenseWriter = new LicenseWriter(interpreterFactory, serializer, signatureProvider, _timeProviderMock.Object);

        var loggerMock = new Mock<ILogger<LicenseReader>>();
        _sut = new LicenseReader(serializer, signatureProvider, loggerMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_Returns_LicenseData_WhenJwtIsValid()
    {
        // Arrange
        var parameters = _fixture.Build<LicenseParameters>()
            .With(x => x.ManifestVersion, 1)
            .With(x => x.ExpiresAt, DateTime.UtcNow.Date.AddYears(10))
            .Create();
        var jwt = _licenseWriter.Write(parameters);

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var jwtString = jwtSecurityTokenHandler.WriteToken(jwt)!;

        // Act
        var actual = await _sut.ValidateAsync(jwtString);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(parameters.InstallationId, actual.InstallationId);
        Assert.Equal(parameters.ManifestVersion, actual.ManifestVersion);
        Assert.Equal(parameters.ExpiresAt, actual.ExpiresAt);
    }

    [Fact]
    public async Task ValidateAsync_Throws_InvalidLicenseException_WhenSignatureIsInvalid()
    {
        // Arrange
        var jwtString = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjp7Ik1hbmlmZXN0VmVyc2lvbiI6MSwiSW5zdGFsbGF0aW9uSWQiOiIxZjkxNjU0My00NmZiLTQ5MzktYjhjZS1jN2UxZjkxZTM5OTMifSwibmJmIjoxNzAzMjQwNTA4LCJleHAiOjIwMTg4MjI0MDAsImlhdCI6MTcwMzI0MDUwOH0.XYfM2GCCVaHpnngt-J7lBTmKriUwmV_C6LJ8ZICWvUGYp6GgbBt1AgKmk4v2qwnsgY5JDU-PszFvkWM-WQVqMK-oMnjb3T7L2Z799gT95ugQrBdgoIUKGWwS9qHlXc7laTgOc29t3H0pGdf5LNtC8cVrQneH_5YHxmge3utUGpAbMigDCoRqn0VktqRrc-4_4JXaEmaqrvSDzP737JccgdAuArWdADJ0d87E3o0-GDCHaRxNEFEzXlWJl4bP7YxeWhN45v6liegGYbYin_fbBmK_5C0KvXtis-KvOivVoVg5EMTUeldy4S52azS3qd17EYze7iGB5ixldq2ngIxVA";

        // Act
        var actual = await Assert.ThrowsAsync<InvalidLicenseException>(async () =>
            await _sut.ValidateAsync(jwtString));

        // Assert
        Assert.Equal("The license is invalid.", actual.Message);
        Assert.Equal(jwtString, actual.License);
    }
}