using System.Security.Cryptography;
using System.Text;

namespace Passwordless.Common.Services.Licensing;

public class SignatureProvider : ISignatureProvider
{
    public byte[] Sign(RSAParameters privateKey, string data)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(privateKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var cryptoServiceProvider = SHA512.Create();
        return rsa.SignData(dataBytes, cryptoServiceProvider);
    }
    
    public bool Verify(RSAParameters publicKey, string data, byte[] signature)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(publicKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var cryptoServiceProvider = SHA512.Create();
        return rsa.VerifyData(dataBytes, cryptoServiceProvider, signature);
    }
}