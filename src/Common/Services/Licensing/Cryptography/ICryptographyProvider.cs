using System.Security.Cryptography;

namespace Passwordless.Common.Services.Licensing.Cryptography;

public interface ICryptographyProvider
{
    RSA PrivateKey { get; }
    RSA PublicKey { get; }
}