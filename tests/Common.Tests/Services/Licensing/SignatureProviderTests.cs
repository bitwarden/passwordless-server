using System.Security.Cryptography;
using Passwordless.Common.Services.Licensing;

namespace Passwordless.Common.Tests.Services.Licensing;

public class SignatureProviderTests
{
    [Fact]
    public void Sign_Generates_CorrectSignature()
    {
        // Arrange
        using var rsa = new RSACryptoServiceProvider(2048);
        var privateKey = rsa.ExportParameters(true);
        var publicKey = rsa.ExportParameters(false);
        
        var sut = new SignatureProvider();
        
        // Act
        var actual = sut.Sign(privateKey, "test");
        
        // Assert
        Assert.NotNull(actual);
        Assert.True(sut.Verify(publicKey, "test", actual));
    }
    
    [Fact]
    public void Verify_Returns_False_WhenDataIsModified()
    {
        // Arrange
        using var rsa = new RSACryptoServiceProvider(2048);
        var privateKey = rsa.ExportParameters(true);
        var publicKey = rsa.ExportParameters(false);
        
        var sut = new SignatureProvider();
        
        // Act
        var actual = sut.Sign(privateKey, "test");
        
        // Assert
        Assert.NotNull(actual);
        Assert.False(sut.Verify(publicKey, "test2", actual));
    }
}