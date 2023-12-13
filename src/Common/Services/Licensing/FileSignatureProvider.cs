using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Passwordless.Common.Services.Licensing.Configuration;

namespace Passwordless.Common.Services.Licensing;

public class FileSignatureProvider : ISignatureProvider
{
    private readonly FileSignatureConfiguration _options;
    
    public FileSignatureProvider(IOptions<FileSignatureConfiguration> options)
    {
        _options = options.Value;
    }
    
    public X509Certificate2 PrivateKey => new (_options.PrivateKey!);
    
    public X509Certificate2 PublicKey => new (_options.PublicKey!);
}