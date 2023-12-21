using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Common.Services.Licensing.Interpreters;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Tests.Services.Licensing.Interpreters;

public class LicenseInterpreterFactoryTests
{
    private readonly Fixture _fixture = new();
    
    private readonly Mock<ILogger<LicenseInterpreterFactory>> _loggerMock;
    
    private readonly LicenseInterpreterFactory _sut;
    
    public LicenseInterpreterFactoryTests()
    {
        _loggerMock = new();
        _sut = new(_loggerMock.Object);
    }
    
    [Fact]
    public void Create_Returns_Correct_LicenseWriter()
    {
        // Arrange
        var parameters = _fixture.Build<LicenseParameters>().With(x => x.ManifestVersion, 1).Create();
        
        // Act
        var result = _sut.Create(parameters);
        
        // Assert
        Assert.IsType<LicenseInterpreter>(result);
    }
    
    [Fact]
    public void Create_Throws_ArgumentOutOfRangeException_When_ManifestVersion_Is_Invalid()
    {
        // Arrange
        var parameters = _fixture.Build<LicenseParameters>().With(x => x.ManifestVersion, 0).Create();
        
        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Create(parameters));
        
        // Assert
        Assert.Equal($"No license interpreter found for manifest version '{parameters.ManifestVersion}'. (Parameter 'ManifestVersion')", exception.Message);
    }
}