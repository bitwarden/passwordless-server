using System.Security.Cryptography.X509Certificates;

namespace Passwordless.Common.Services.Licensing;

public interface ISignatureProvider
{
    X509Certificate2 PrivateKey { get; }
    
    X509Certificate2 PublicKey { get; }
}