using System.Security.Cryptography;

namespace Passwordless.Common.Services.Licensing;

public interface ISignatureProvider
{
    byte[] Sign(RSAParameters privateKey, string json);
    
    bool Verify(RSAParameters publicKey, string json, byte[] signature);
}