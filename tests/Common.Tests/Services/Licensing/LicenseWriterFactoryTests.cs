using Passwordless.Common.Services.Licensing;
using Passwordless.Common.Services.Licensing.V1;

namespace Passwordless.Common.Tests.Services.Licensing;

public class LicenseWriterFactoryTests
{
    [Fact]
    public void Constructor_Detects_AllLicenseWriters()
    {
        // Arrange
        var sut = new LicenseWriterFactory();
        
        // Act
        
        // Assert
        Assert.Equal(1, sut.Writers.Count);
    }
    
    [Fact]
    public void Create_Returns_Correct_LicenseWriter()
    {
        // Arrange
        var sut = new LicenseWriterFactory();
        
        // Act
        var result = sut.Create(1);
        
        // Assert
        Assert.IsType<LicenseWriter1>(result);
    }
}