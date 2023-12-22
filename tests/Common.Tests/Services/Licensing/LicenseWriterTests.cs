using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.Common.Services.Licensing;
using Passwordless.Common.Services.Licensing.Cryptography;
using Passwordless.Common.Services.Licensing.Interpreters;
using Passwordless.Common.Services.Licensing.Models;
using Passwordless.Common.Services.Licensing.Serializers;

namespace Passwordless.Common.Tests.Services.Licensing;

public class LicenseWriterTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<TimeProvider> _timeProviderMock = new();

    private readonly LicenseWriter _sut;

    public LicenseWriterTests()
    {
        var interpreterFactoryLoggerMock = new Mock<ILogger<LicenseInterpreterFactory>>();
        var interpreterFactory = new LicenseInterpreterFactory(interpreterFactoryLoggerMock.Object);

        var serializer = new LicenseSerializer();

        var fileSignatureConfiguration = new FileCryptographyConfiguration
        {
            PrivateKey = "license_dev_private.pem"
        };

        var optionsMock = new Mock<IOptions<FileCryptographyConfiguration>>();
        optionsMock.SetupGet(x => x.Value).Returns(fileSignatureConfiguration);
        var signatureProvider = new FileCryptographyProvider(optionsMock.Object);

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);


        _sut = new LicenseWriter(interpreterFactory, serializer, signatureProvider, _timeProviderMock.Object);
    }

    [Fact]
    public void Write_Generates_JwtWithSameExpiryAsJwtExpiry()
    {
        // Arrange
        var parameters = _fixture.Build<LicenseParameters>()
            .With(x => x.ManifestVersion, 1)
            .With(x => x.ExpiresAt, DateTime.UtcNow.Date.AddYears(10))
            .Create();

        // Act
        var actual = _sut.Write(parameters);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("JWT", actual.Header.Typ);
        Assert.Equal("RS256", actual.Header.Alg);
        Assert.Equal(parameters.ExpiresAt, actual.ValidTo);
    }
}